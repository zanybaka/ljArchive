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
			this.TreeView.AppendColumn("ID", new CellRendererText(), "text", 0);
			this.TreeView.AppendColumn("Subject", new CellRendererText(), "text", 1).Resizable = true;
			this.TreeView.AppendColumn("Body", new CellRendererText(), "text", 2).Resizable = true;
			this.TreeView.Selection.Changed += TreeView_Selection_Changed;
			
			// html
			html = new HTML();
			html.LoadFromString("<h1>WHAT UP INTERNET!</h1>");
			
			ScrolledWindowHTML.Add(html);
			
			MainWindow.ShowAll();
		}
		
		private void Initialize()
		{
			Open("/home/erik/downloads/archive.lja");
		}
		
		protected void Open(string path)
		{
			j = EF.ljArchive.Engine.Journal.Load(path);
			Populate();
		}
		
		protected void Populate()
		{
			ListStore store = new ListStore(typeof(string), typeof(string), typeof(string));
			foreach (EF.ljArchive.Engine.Journal.EventsRow er in j.Events)
			{
				string body = er.IsBodyNull() ? string.Empty : er.Body;
				if (body.Length > 10)
					body = body.Substring(0, 10);
				store.AppendValues(er.ID.ToString(), (er.IsSubjectNull() ? string.Empty : er.Subject), body);
			}
			this.TreeView.Model = store;
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
				int val = int.Parse((string) model.GetValue (iter, 0));
				html.LoadFromString(j.Events.FindByID(val).Body);
			}
		}

		[Widget]
		private Window MainWindow;
		[Widget]
		private ScrolledWindow ScrolledWindowHTML;
		[Widget]
		private TreeView TreeView;
		private Glade.XML gxml;
		private HTML html;
		private EF.ljArchive.Engine.Journal j;
	}
}