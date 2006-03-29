using System;
using System.IO;
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
			MainWindow.IconList = new Gdk.Pixbuf[] {
				new Gdk.Pixbuf(null, "App16x16.xpm"),
				new Gdk.Pixbuf(null, "App32x32.xpm")
			};
			
			// TreeView
			TreeViewColumn tc;
			tc = new TreeViewColumn(string.Empty, new CellRendererText(), "text", 0);
			tc.Sizing = TreeViewColumnSizing.Fixed;
			tc.FixedWidth = 24;
			tvEvents.AppendColumn(tc);
			tc = new TreeViewColumn("ID", new CellRendererText(), "text", 1);
			tc.Sizing = TreeViewColumnSizing.Fixed;
			tc.FixedWidth = 100;
			tvEvents.AppendColumn(tc);
			tc = new TreeViewColumn("Date", new CellRendererText(), "text", 2);
			tc.Sizing = TreeViewColumnSizing.Fixed;
			tc.FixedWidth = 100;
			tc.Resizable = true;
			tvEvents.AppendColumn(tc);
			tc = new TreeViewColumn("Subject", new CellRendererText(), "text", 3);
			tc.Sizing = TreeViewColumnSizing.Fixed;
			tc.FixedWidth = 100;
			tc.Resizable = true;
			tvEvents.AppendColumn(tc);
			tc = new TreeViewColumn("Body", new CellRendererText(), "text", 4);
			tc.Sizing = TreeViewColumnSizing.Fixed;
			tvEvents.AppendColumn(tc);
			tvEvents.Selection.Changed += TreeView_Selection_Changed;
			tvEvents.SearchColumn = 3;
			
			// html
			html = new Components.HTMLAdvanced();
			html.LoadFromString("Hey!  Open a file.");
			
			// mniFileOpen
			mniFileOpen.Activated += MenuItem_Activated;
			
			// mniFileQuit
			mniFileQuit.Activated += MenuItem_Activated;
			
			// mniFileNew
			mniFileNew.Activated += MenuItem_Activated;
			
			// cmbSearchType
			cmbSearchType.Active = 0;
			
			// fcdOpen
			fcdOpen = new FileChooserDialog("Open ljArchive file", MainWindow, FileChooserAction.Open);
			fcdOpen.AddButton(Stock.Open, ResponseType.Accept);
			fcdOpen.AddButton(Stock.Cancel, ResponseType.Cancel);
			FileFilter ff = new FileFilter();
			ff.Name = "ljArchive files";
			ff.AddPattern("*.lja");
			fcdOpen.AddFilter(ff);
			ff = new FileFilter();
			ff.Name = "All files";
			ff.AddPattern("*");
			fcdOpen.AddFilter(ff);
			
			// tbSync
			tbSync.Clicked += ToolButton_Clicked;
			
			// StatusBar
			StatusBar.Push(1, "What up.");
			
			swHTML.Add(html);
			
			MainWindow.ShowAll();
		}
		
		private void Initialize()
		{
			string transform;
			using (FileStream fs = File.Open("../../../etc/templates/talkread.ljt", FileMode.Open, FileAccess.Read))
			{
				StreamReader sr = new StreamReader(fs);
				transform = sr.ReadToEnd();
			}
			hjw = new Engine.HTML.HTMLJournalWriter();
			hjw.Transform = transform;
		}
		
		protected void Open(string path)
		{
			j = EF.ljArchive.Engine.Journal.Load(path);
			estore = new Components.EntriesStore(j); 
			tvEvents.Model = estore;
			tvEvents.Selection.SelectPath(new TreePath(new int[] {0}));
		}
		
		private void MainWindow_Delete (object sender, DeleteEventArgs e)
		{
			Application.Quit ();
		}
		
		private void TreeView_Selection_Changed(object sender, EventArgs e)
		{
			TreeIter iter;
			TreeModel model;
			string htmltoload = "Nothing selected.";
			Engine.Journal.EventsRow er;
			
			if (((TreeSelection) sender).GetSelected (out model, out iter))
			{
				int val = int.Parse((string) model.GetValue (iter, 1));
				using (MemoryStream ms = new MemoryStream())
				{
					er = j.Events.FindByID(val);
					if (er != null)
					{
						hjw.WriteJournal(ms, j, new int[] {er.ID}, null, true, true);
						ms.Seek(0, SeekOrigin.Begin);
						using (StreamReader sr = new StreamReader(ms))
							htmltoload = sr.ReadToEnd();
					}
				}
			}
			html.LoadFromString(htmltoload);
		}
		
		private void MenuItem_Activated(object sender, EventArgs e)
		{
			if (sender == mniFileOpen)
			{
				ResponseType rt = (ResponseType) fcdOpen.Run();
				fcdOpen.Hide();
				if (rt == ResponseType.Accept && fcdOpen.Filename != null)
					Open(fcdOpen.Filename);
				tvEvents.Selection.SelectPath(new TreePath(new int[] {0}));
			}
			else if (sender == mniFileNew)
			{
				if (NewJournalController.Go() == ResponseType.Ok)
				{
					j = new Engine.Journal(NewJournalController.ServerURL,
						NewJournalController.UserName,
						NewJournalController.Password,
						NewJournalController.GetComments,
						NewJournalController.UseJournal);
					j.Save(NewJournalController.Filename);
					estore = new Components.EntriesStore(j); 
					tvEvents.Model = estore;
					Engine.Sync.Start(j, new Engine.SyncOperationCallBack(Sync_SyncOperationCallBack));
				}
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
				case Engine.SyncOperation.Failure:
				    StatusBar.Pop(1);
				    StatusBar.Push(1, "FLAGRANT SYSTEM ERROR!!!");
					Console.WriteLine(Engine.Sync.SyncException.ToString());
					break;
				case Engine.SyncOperation.Success:
					StatusBar.Pop(1);
					StatusBar.Push(1, soe.SyncOperation.ToString());
					estore = new Components.EntriesStore(j); 
					tvEvents.Model = estore;
					break;
			}
		}		

		[Widget]
		private Window MainWindow;
		[Widget]
		private ScrolledWindow swHTML;
		[Widget]
		private TreeView tvEvents;
		[Widget]
		private MenuItem mniFileOpen;
		[Widget]
		private MenuItem mniFileQuit;
		[Widget]
		private MenuItem mniFileNew;
		[Widget]
		private ToolButton tbSync;
		[Widget]
		private Statusbar StatusBar;
		[Widget]
		private ComboBox cmbSearchType;
		private Engine.HTML.HTMLJournalWriter hjw;
		private Glade.XML gxml;
		private Components.HTMLAdvanced html;
		private Engine.Journal j;
		private Components.EntriesStore estore;
		private FileChooserDialog fcdOpen;
		private delegate void UpdateStatusDelegate(Engine.SyncOperationEventArgs soe);
	}
}