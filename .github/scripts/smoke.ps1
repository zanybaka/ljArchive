# Launch ljArchive, optionally with every file marked as downloaded from the
# Internet the way Explorer marks files unpacked from a downloaded zip, and
# report which window comes up: the main window or the fatal error dialog (#2).
param(
    [Parameter(Mandatory = $true)] [string] $Dir,
    [Parameter(Mandatory = $true)] [ValidateSet('ok', 'error')] [string] $Expect,
    [ValidateSet('internet', 'none')] [string] $Zone = 'internet'
)
$ErrorActionPreference = 'Stop'

$work = Join-Path ([IO.Path]::GetTempPath()) ('ljArchive-smoke-' + [Guid]::NewGuid().ToString('N'))
Copy-Item -Path $Dir -Destination $work -Recurse
$templates = Join-Path $work 'templates'
if (-not (Test-Path $templates)) {
    # the installer puts them next to the exe, a zip packed before the templates step has none
    Copy-Item -Path (Join-Path $PSScriptRoot '..\..\ljArchive\etc\templates') -Destination $templates -Recurse
    Write-Host "templates taken from the checkout, the package has none"
}
$files = Get-ChildItem $work -Recurse -File
Write-Host "config present: $(Test-Path (Join-Path $work 'ljArchive.exe.config'))"
if ($Zone -eq 'internet') {
    foreach ($f in $files) {
        Set-Content -Path $f.FullName -Stream Zone.Identifier -Value "[ZoneTransfer]`r`nZoneId=3"
    }
    $marked = @($files | Where-Object { Get-Item -Path $_.FullName -Stream Zone.Identifier -ErrorAction SilentlyContinue }).Count
    Write-Host "marked $marked of $($files.Count) files with ZoneId=3"
} else {
    Write-Host "no zone mark on $($files.Count) files"
}

Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
public static class Win
{
    delegate bool EnumProc(IntPtr h, IntPtr l);
    [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc cb, IntPtr l);
    [DllImport("user32.dll")] static extern bool EnumChildWindows(IntPtr parent, EnumProc cb, IntPtr l);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr h);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetWindowText(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetClassName(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern IntPtr SendMessage(IntPtr h, uint m, IntPtr w, StringBuilder l);
    const uint WM_GETTEXT = 0x000D;
    const uint WM_GETTEXTLENGTH = 0x000E;
    public static List<IntPtr> TopLevel(uint pid)
    {
        var r = new List<IntPtr>();
        EnumWindows((h, l) => { uint p; GetWindowThreadProcessId(h, out p); if (p == pid && IsWindowVisible(h)) r.Add(h); return true; }, IntPtr.Zero);
        return r;
    }
    public static List<IntPtr> Children(IntPtr parent)
    {
        var r = new List<IntPtr>();
        EnumChildWindows(parent, (h, l) => { r.Add(h); return true; }, IntPtr.Zero);
        return r;
    }
    public static string Title(IntPtr h) { var s = new StringBuilder(512); GetWindowText(h, s, s.Capacity); return s.ToString(); }
    public static string Class(IntPtr h) { var s = new StringBuilder(256); GetClassName(h, s, s.Capacity); return s.ToString(); }
    public static string Text(IntPtr h)
    {
        int n = (int)SendMessage(h, WM_GETTEXTLENGTH, IntPtr.Zero, null);
        var s = new StringBuilder(n + 1);
        SendMessage(h, WM_GETTEXT, (IntPtr)(n + 1), s);
        return s.ToString();
    }
}
'@

# CreateProcess directly, ShellExecute would stop on the marked exe with a security prompt
$psi = New-Object Diagnostics.ProcessStartInfo
$psi.FileName = Join-Path $work 'ljArchive.exe'
$psi.WorkingDirectory = $work
$psi.UseShellExecute = $false
$p = [Diagnostics.Process]::Start($psi)

$known = @('ljArchive', 'Error')
$tops = @()
$titles = @()
$sw = [Diagnostics.Stopwatch]::StartNew()
while ($sw.Elapsed.TotalSeconds -lt 120) {
    Start-Sleep -Seconds 2
    if ($p.HasExited) { break }
    $tops = @([Win]::TopLevel($p.Id))
    $titles = @($tops | ForEach-Object { [Win]::Title($_) })
    if (@($titles | Where-Object { $known -contains $_ }).Count -gt 0) { break }
}
$described = @($tops | ForEach-Object { "'$([Win]::Title($_))' [$([Win]::Class($_))]" }) -join ' | '
Write-Host ("after {0:n0} s, windows: {1}" -f $sw.Elapsed.TotalSeconds, $described)
if ($p.HasExited) { Write-Host "process exited with code $($p.ExitCode)" }

foreach ($t in $tops) {
    foreach ($c in [Win]::Children($t)) {
        if ([Win]::Class($c) -match 'EDIT') {
            $text = [Win]::Text($c)
            if ($text) {
                Write-Host "---- edit text in '$([Win]::Title($t))'"
                Write-Host $text
            }
        }
    }
}
if (-not $p.HasExited) { Stop-Process -Id $p.Id -Force }

$actual = if ($titles -contains 'Error') { 'error' } elseif ($titles -contains 'ljArchive') { 'ok' } else { 'none' }
Write-Host "expected $Expect, got $actual"
if ($actual -ne $Expect) { exit 1 }
