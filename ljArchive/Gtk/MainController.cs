using System;
using System.IO;
using Gtk;
using Glade;

namespace EF.ljArchive.Gtk
{
	public class MainController
	{
		#region Public Instance Constructors
		public MainController ()
		{
			Initialize();
			InitializeWindow();
		}
		#endregion

		#region Protected Instance Methods
		protected void InitializeWindow()
		{
			// gxml
			this.gxml = new Glade.XML(null, "ljarchive.glade", "MainWindow", null);
			this.gxml.Autoconnect(this);
			
			// cc
			cc = new CommandCollection(gxml, plugins);
			cc.MenuItem_Activated = this.MenuItem_Activated;
			cc.ToolButton_Clicked = this.ToolButton_Clicked;
			cc.CommandState = CommandStates.FileClosed;
			
			// TreeView
			this.tva = new Components.TreeViewAdvanced();
			this.tva.Selection.Changed += TreeView_Selection_Changed;
			this.tva.SearchColumn = 3;
			this.tva.RulesHint = true;
			this.tva.Show();
			
			// html
			this.html = new Components.HTMLAdvanced();
			this.html.LoadFromString("Hey!  Open a file.");
			html.Show();
			
			// cmbSearchType
			this.cmbSearchType.Active = 0;
			
			// fcdOpen
			this.fcdOpen = new FileChooserDialog("Open ljArchive file", MainWindow, FileChooserAction.Open);
			this.fcdOpen.AddButton(Stock.Open, ResponseType.Accept);
			this.fcdOpen.AddButton(Stock.Cancel, ResponseType.Cancel);
			FileFilter ff = new FileFilter();
			ff.Name = "ljArchive files";
			ff.AddPattern("*.lja");
			this.fcdOpen.AddFilter(ff);
			ff = new FileFilter();
			ff.Name = "All files";
			ff.AddPattern("*");
			this.fcdOpen.AddFilter(ff);
			
			// StatusBar
			this.StatusBar.Push(1, "What up.");
			
			// swHTML
			this.swHTML.Add(html);
			
			// swTreeView
			this.swTreeView.Add(tva);
			
			// entriesColumns
			entriesColumns = new TreeViewColumn[] {
				new TreeViewColumn(string.Empty, new CellRendererText(), "text", 0),
				new TreeViewColumn("ID", new CellRendererText(), "text", 1),
				new TreeViewColumn("Date", new CellRendererText(), "text", 2),
				new TreeViewColumn("Subject", new CellRendererText(), "text", 3),
				new TreeViewColumn("Body", new CellRendererText(), "text", 4)
			};
			entriesColumns[0].Sizing = TreeViewColumnSizing.Fixed;
			entriesColumns[0].FixedWidth = 24;
			entriesColumns[1].Sizing = TreeViewColumnSizing.Fixed;
			entriesColumns[1].FixedWidth = 100;
			entriesColumns[1].Resizable = true;
			entriesColumns[2].Sizing = TreeViewColumnSizing.Fixed;
			entriesColumns[2].FixedWidth = 100;
			entriesColumns[2].Resizable = true;
			entriesColumns[3].Sizing = TreeViewColumnSizing.Fixed;
			entriesColumns[3].FixedWidth = 100;
			entriesColumns[3].Resizable = true;
			entriesColumns[4].Sizing = TreeViewColumnSizing.Fixed;
			
			// commentsColumns
			commentsColumns = new TreeViewColumn[] {
				new TreeViewColumn(string.Empty, new CellRendererText(), "text", 0),
				new TreeViewColumn("ID", new CellRendererText(), "text", 1),
				new TreeViewColumn("UserName", new CellRendererText(), "text", 2),
				new TreeViewColumn("Date", new CellRendererText(), "text", 3),
				new TreeViewColumn("Subject", new CellRendererText(), "text", 4),
				new TreeViewColumn("Body", new CellRendererText(), "text", 5)
			};
			commentsColumns[0].Sizing = TreeViewColumnSizing.Fixed;
			commentsColumns[0].FixedWidth = 24;
			commentsColumns[1].Sizing = TreeViewColumnSizing.Fixed;
			commentsColumns[1].FixedWidth = 100;
			commentsColumns[1].Resizable = true;
			commentsColumns[2].Sizing = TreeViewColumnSizing.Fixed;
			commentsColumns[2].FixedWidth = 100;
			commentsColumns[2].Resizable = true;
			commentsColumns[3].Sizing = TreeViewColumnSizing.Fixed;
			commentsColumns[3].FixedWidth = 100;
			commentsColumns[3].Resizable = true;
			commentsColumns[4].Sizing = TreeViewColumnSizing.Fixed;
			commentsColumns[4].FixedWidth = 100;
			commentsColumns[4].Resizable = true;
			commentsColumns[5].Sizing = TreeViewColumnSizing.Fixed;
			
			// TreeViewState
			this.TreeViewState = TreeViewStates.Hidden;
			
			// MainWindow
			this.MainWindow.DeleteEvent += MainWindow_Delete;
			this.MainWindow.IconList = new Gdk.Pixbuf[] {
				new Gdk.Pixbuf(null, "App16x16.xpm"),
				new Gdk.Pixbuf(null, "App32x32.xpm")
			};
			MainWindow.Show();
		}
		
		protected void Initialize()
		{
			string transform;
			using (FileStream fs = File.Open("../../../etc/templates/talkread.ljt", FileMode.Open, FileAccess.Read))
			{
				StreamReader sr = new StreamReader(fs);
				transform = sr.ReadToEnd();
			}
			hjw = new Engine.HTML.HTMLJournalWriter();
			hjw.Transform = transform;
			
			plugins = new Engine.Collections.PluginCollection("plugins");
		}
		
		protected void Open()
		{
			ResponseType rt = (ResponseType) fcdOpen.Run();
			fcdOpen.Hide();
			if (rt == ResponseType.Accept && fcdOpen.Filename != null)
			{
				estore = null;
				cstore = null;
				j = EF.ljArchive.Engine.Journal.Load(fcdOpen.Filename);
				this.TreeViewState = TreeViewStates.Entries;
				tva.Selection.SelectPath(new TreePath(new int[] {0}));
				cc.CommandState = CommandStates.FileOpened;
				MainWindow.Title = string.Format("ljArchive - {0}", j.Options.IsUseJournalNull() ? j.Options.UserName : j.Options.UseJournal);
			}
		}
		
		protected void New()
		{
			if (NewJournalController.Go() == ResponseType.Ok)
			{
				estore = null;
				cstore = null;
				j = new Engine.Journal(NewJournalController.ServerURL,
					NewJournalController.UserName,
					NewJournalController.Password,
					NewJournalController.GetComments,
					NewJournalController.UseJournal);
				j.Save(NewJournalController.Filename);
				this.TreeViewState = TreeViewStates.Entries;
				ToolButton_Clicked(cc.tbSync, EventArgs.Empty); 
			}
		}
		#endregion
		
		internal void UpdateTreeView()
		{
			switch (treeViewState)
			{
				case TreeViewStates.Hidden:
					vbTreeView.Visible = false;
				break;
				case TreeViewStates.Entries:
					vbTreeView.Visible = true;
					tva.Columns = entriesColumns;
					if (estore == null)
						estore = new Components.EntriesStore(j);
					tva.Model = estore;
				break;
				case TreeViewStates.Comments:
					vbTreeView.Visible = true;
					tva.Columns = commentsColumns;
					if (cstore == null)
						cstore = new Components.CommentsStore(j);
					tva.Model = cstore;
				break;
			}
		}

		protected TreeViewStates TreeViewState
		{
			get { return treeViewState; }
			set
			{
				treeViewState = value;
				cc.TreeViewState = value;
				UpdateTreeView();
			}
		}

		#region Private Instance Methods
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
					cc.CommandState = CommandStates.FileOpened;
					break;
				case Engine.SyncOperation.Success:
					j.Save();
					StatusBar.Pop(1);
					StatusBar.Push(1, soe.SyncOperation.ToString());
					estore = new Components.EntriesStore(j); 
					tva.Model = estore;
					cc.CommandState = CommandStates.FileOpened;
					break;
			}
		}

		private void MenuItem_Activated(object sender, EventArgs e)
		{
			if (cc.IsPlugin((MenuItem) sender))
			{
				Common.IPlugin p = cc.GetPlugin((MenuItem) sender);
				p.Go(j);
			}
			else if (sender == cc.mniFileOpen)
			{
				Open();
			}
			else if (sender == cc.mniFileNew)
			{
				New();
			}
			else if (sender == cc.mniViewNone)
			{
				treeViewState = TreeViewStates.Hidden;
				UpdateTreeView();
			}
			else if (sender == cc.mniViewEntries)
			{
				treeViewState = TreeViewStates.Entries;
				UpdateTreeView();
			}
			else if (sender == cc.mniViewComments)
			{
				treeViewState = TreeViewStates.Comments;
				UpdateTreeView();
			}
			else if (sender == cc.mniHelpAbout)
			{
				AboutController.Go();
			}
			else if (sender == cc.mniFileQuit)
			{
				Application.Quit();
			}
		}

		private void ToolButton_Clicked(object sender, EventArgs e)
		{
			if (sender == cc.tbSync)
			{
				cc.CommandState = CommandStates.Syncing;
				Engine.Sync.Start(j, new Engine.SyncOperationCallBack(Sync_SyncOperationCallBack));
			}
		}
		#endregion

		#region Private Instance Fields
		[Widget] private Window MainWindow;
		[Widget] private ScrolledWindow swHTML;
		private Components.TreeViewAdvanced tva;
		[Widget] private Statusbar StatusBar;
		[Widget] private ComboBox cmbSearchType;
		[Widget] private ScrolledWindow swTreeView;
		[Widget] private VBox vbTreeView;
		private TreeViewColumn[] entriesColumns;
		private TreeViewColumn[] commentsColumns;
		private Engine.HTML.HTMLJournalWriter hjw;
		private Glade.XML gxml;
		private Components.HTMLAdvanced html;
		private Engine.Journal j;
		private Components.EntriesStore estore;
		private Components.CommentsStore cstore;
		private FileChooserDialog fcdOpen;
		private Engine.Collections.PluginCollection plugins;
		private CommandCollection cc;
		private TreeViewStates treeViewState;
		private delegate void UpdateStatusDelegate(Engine.SyncOperationEventArgs soe);
		#endregion
	}
}