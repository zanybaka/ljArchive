using System;
using Gtk;
using Glade;

namespace EF.ljArchive.Gtk
{
	public class MainController
	{
		public MainController ()
		{
			InitializeWindow();
			Initialize();
		}
		
		private void InitializeWindow()
		{
			// gxml
			gxml = new Glade.XML(null, "ljarchive.glade", "MainWindow", null);
			gxml.Autoconnect(this);
			
			// MainWindow
			MainWindow.DeleteEvent += MainWindow_Delete;
			
			// TreeView
			TreeViewColumn tc;
			this.TreeView.AppendColumn(string.Empty, new CellRendererText(), "text", 0);
			this.TreeView.AppendColumn("ID", new CellRendererText(), "text", 1);
			this.TreeView.AppendColumn("Date", new CellRendererText(), "text", 2).Resizable = true;
			tc = this.TreeView.AppendColumn("Subject", new CellRendererText(), "text", 3);
			tc.Resizable = true;
			tc.FixedWidth = 100;
			tc.Sizing = TreeViewColumnSizing.Fixed;
			this.TreeView.AppendColumn("Body", new CellRendererText(), "text", 4).Resizable = true;
			this.TreeView.Selection.Changed += TreeView_Selection_Changed;
			this.TreeView.SearchColumn = 3;
			
			// html
			html = new Components.HTMLAdvanced();
			
			// mniFileOpen
			mniFileOpen.Activated += MenuItem_Activated;
			
			// mniFileQuit
			mniFileQuit.Activated += MenuItem_Activated;
			
			// fcd
			fcd = new FileChooserDialog("Open ljArchive file", MainWindow, FileChooserAction.Open);
			fcd.AddButton(Stock.Open, ResponseType.Accept);
			fcd.AddButton(Stock.Cancel, ResponseType.Cancel);
			FileFilter ff = new FileFilter();
			ff.Name = "ljArchive files";
			ff.AddPattern("*.lja");
			fcd.AddFilter(ff);
			ff = new FileFilter();
			ff.Name = "All files";
			ff.AddPattern("*");
			fcd.AddFilter(ff);
			
			// tbSync
			tbSync.Clicked += ToolButton_Clicked;
			
			// StatusBar
			StatusBar.Push(1, "What up.");
			
			ScrolledWindowHTML.Add(html);
			
			MainWindow.ShowAll();
		}
		
		private void Initialize()
		{
			this.TreeView.Selection.SelectPath(new TreePath(new int[] {0}));
		}
		
		protected void Open(string path)
		{
			j = EF.ljArchive.Engine.Journal.Load(path);
			jstore = new Components.JournalStore(j); 
			this.TreeView.Model = jstore;
		}
		
		private void MainWindow_Delete (object sender, DeleteEventArgs e)
		{
			Application.Quit ();
		}
		
		private void TreeView_Selection_Changed(object sender, EventArgs e)
		{
			TreeIter iter;
			TreeModel model;
 
			if (((TreeSelection) sender).GetSelected (out model, out iter))
			{
				int val = int.Parse((string) model.GetValue (iter, 1));
				html.LoadFromString(j.Events.FindByID(val).Body);
			}
		}
		
		private void MenuItem_Activated(object sender, EventArgs e)
		{
			if (sender == mniFileOpen)
			{
				ResponseType rt = (ResponseType) fcd.Run();
				fcd.Hide();
				if (rt == ResponseType.Accept && fcd.Filename != null)
					Open(fcd.Filename);
			}
			else if (sender == mniFileQuit)
			{
				Application.Quit();
			}
		}
		
		private void ToolButton_Clicked(object sender, EventArgs e)
		{
			if (sender == tbSync)
			{
				Engine.Sync.Start(j, new Engine.SyncOperationCallBack(Sync_SyncOperationCallBack));
			}
		}
		
		private void Sync_SyncOperationCallBack(Engine.SyncOperationEventArgs soe)
		{
			if (MainWindow.Visible)
				Application.Invoke(null, soe, StatusChanged);
		}

		protected void StatusChanged(object sender, System.EventArgs e)
		{
			Engine.SyncOperationEventArgs soe = (Engine.SyncOperationEventArgs) e;
			switch (soe.SyncOperation)
			{
				default:
					StatusBar.Pop(1);
					StatusBar.Push(1, soe.SyncOperation.ToString() + "...");
					break;
				case Engine.SyncOperation.Success:
					StatusBar.Pop(1);
					StatusBar.Push(1, soe.SyncOperation.ToString());
					jstore = new Components.JournalStore(j); 
					this.TreeView.Model = jstore;
					break;
			}
		}		

		[Widget]
		private Window MainWindow;
		[Widget]
		private ScrolledWindow ScrolledWindowHTML;
		[Widget]
		private TreeView TreeView;
		[Widget]
		private MenuItem mniFileOpen;
		[Widget]
		private MenuItem mniFileQuit;
		[Widget]
		private ToolButton tbSync;
		[Widget]
		private Statusbar StatusBar;
		private Glade.XML gxml;
		private Components.HTMLAdvanced html;
		private EF.ljArchive.Engine.Journal j;
		private Components.JournalStore jstore;
		private FileChooserDialog fcd;
		private delegate void UpdateStatusDelegate(Engine.SyncOperationEventArgs soe);
	}
}