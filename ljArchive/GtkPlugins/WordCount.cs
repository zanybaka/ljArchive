using System;
using System.IO;
using System.Collections;
using Gtk;
using Glade;
using EF.ljArchive.Common;
using EF.ljArchive.Plugins.Gtk;

namespace EF.ljArchive.Plugins.Gtk
{
	/// <summary>
	/// Summary description for WordCount.
	/// </summary>
	public class WordCount : IPlugin
	{

		public WordCount()
		{
			InitializeWindow();
		}

		private void InitializeWindow()
		{
			// gxml
			this.gxml = new Glade.XML(null, "gtkplugins.glade", "WordCountWindow", null);
			this.gxml.Autoconnect(this);
			
			// tv
			TreeViewColumn tvc = new TreeViewColumn("Word", new CellRendererText(), "text", 0);
			tvc.Resizable = true;
			tvc.Sizing = TreeViewColumnSizing.Fixed;
			tvc.FixedWidth = 180;
			tvc.Clickable = true;
			tvc.SortColumnId = 0;
			this.tv.AppendColumn(tvc);
			tvc = new TreeViewColumn("Count", new CellRendererText(), "text", 1);
			tvc.Sizing = TreeViewColumnSizing.Fixed;
			tvc.Clickable = true;
			tvc.SortColumnId = 1;
			this.tv.AppendColumn(tvc);
			
			// chkCountComments
			this.chkCountComments.Toggled += CheckButton_Toggled;
			
			// chkIgnoreCommonWords
			this.chkIgnoreCommonWords.Toggled += CheckButton_Toggled;
			
			// sbMinimumWordSize
			this.sbMinimumWordSize.ValueChanged += SpinButton_ValueChanged;

			// sbMinimumWordOccurrence
			this.sbMinimumWordOccurrence.ValueChanged += SpinButton_ValueChanged;
			
			// btnClose
			this.btnClose.Clicked += btnClose_Clicked;
			
			// this.WordCountWindow
			this.WordCountWindow.DeleteEvent += this.WordCountWindow_Delete;
		}

		#region IPlugin Members
		public System.Drawing.Image MenuIcon
		{
			get { return null; }
		}

		public string Description
		{
			get { return "Analyzes word counts in your journal."; }
		}

		public string Author
		{
			get { return "Erik Frey"; }
		}

		public void Go(Journal j)
		{
			this.j = j;
			this.WordCountWindow.Show();
			UpdateWords();
		}

		public string Title
		{
			get { return "Word Count Analyzer"; }
		}

		public string URL
		{
			get { return "http://ljarchive.sourceforge.net/"; }
		}

		public Version Version
		{
			get { return new Version(0, 9, 7, 0); }
		}

		public object Settings
		{
			get { return null; }
			set { }
		}

		public int SelectedEventID
		{
			set { }
		}
		#endregion

		protected void UpdateWords()
		{
			using (new Components.Busy(this.WordCountWindow))
			{
				words = Plugins.Core.WordCount.GetWordCount(j, chkCountComments.Active, chkIgnoreCommonWords.Active);
				UpdateTreeView();
			}
		}

		protected void UpdateTreeView()
		{
			ws = new Components.WordsStore(words, (int) sbMinimumWordSize.Value, (int) sbMinimumWordOccurrence.Value);
			ws.SetSortColumnId(1, SortType.Descending);
			tv.Model = ws;
		}
		
		private void CheckButton_Toggled(object sender, EventArgs e)
		{
			UpdateWords();
		}
		
		private void SpinButton_ValueChanged(object sender, EventArgs e)
		{
			UpdateTreeView();
		}
		
		private void btnClose_Clicked(object sender, EventArgs e)
		{
			WordCountWindow.Hide();
		}
		
		private void WordCountWindow_Delete(object sender, DeleteEventArgs de)
		{
			de.RetVal = true;
			WordCountWindow.Hide();
		}

		private Journal j;
		private Hashtable words;
		private Glade.XML gxml;
		private Components.WordsStore ws;
		[Widget] private Window WordCountWindow;
		[Widget] private CheckButton chkCountComments;
		[Widget] private CheckButton chkIgnoreCommonWords;
		[Widget] private SpinButton sbMinimumWordSize;
		[Widget] private SpinButton sbMinimumWordOccurrence;
		[Widget] private TreeView tv;
		[Widget] private Button btnClose;
	}
}
