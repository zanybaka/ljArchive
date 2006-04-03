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
	/// Summary description for PostFrequency.
	/// </summary>
	public class PostFrequency : IPlugin
	{

		public PostFrequency()
		{
			InitializeWindow();
		}

		private void InitializeWindow()
		{
			// gxml
			this.gxml = new Glade.XML(null, "gtkplugins.glade", "PostFrequencyWindow", null);
			this.gxml.Autoconnect(this);
			
			// ZedGraphControl
			this.zg = new Components.ZedGraphControl();
			this.zg.Show();
			
			// vbZG
			this.vbZG.PackStart(zg);
			this.vbZG.ReorderChild(zg, 0);

			// cmbSplitPosts
			this.cmbSplitPosts.Active = 1;
			this.cmbSplitPosts.Changed += cmbSplitPosts_Changed;
			
			// btnClose
			this.btnClose.Clicked += btnClose_Clicked;
			
			// PostFrequencyWindow
			this.PostFrequencyWindow.DeleteEvent += PostFrequencyWindow_Delete;
		}

		#region IPlugin Members
		public System.Drawing.Image MenuIcon
		{
			get { return null; }
		}

		public string Description
		{
			get { return "Analyzes post frequency over the life of the journal."; }
		}

		public string Author
		{
			get { return "Erik Frey"; }
		}

		public void Go(Journal j)
		{
			this.j = j;
			this.PostFrequencyWindow.Show();
			UpdateGraph();
		}

		public string Title
		{
			get { return "Post Frequency Analyzer"; }
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

		protected void UpdateGraph()
		{
			using(new Components.Busy(PostFrequencyWindow))
			{
				zg.GraphPane = EF.ljArchive.Plugins.Core.PostFrequency.GetGraph(j, zg.Width - 1,
					zg.Height - 1, (EF.ljArchive.Plugins.Core.SplitPosts) cmbSplitPosts.Active);
			}
		}
		
		private void PostFrequencyWindow_Delete(object sender, DeleteEventArgs de)
		{
			de.RetVal = true;
			PostFrequencyWindow.Hide();
		}
		
		private void cmbSplitPosts_Changed(object sender, EventArgs e)
		{
			UpdateGraph();
		}
		
		private void btnClose_Clicked(object sender, EventArgs e)
		{
			PostFrequencyWindow.Hide();
		}

		private Journal j;
		private Glade.XML gxml;
		private Components.ZedGraphControl zg;
		[Widget] private Window PostFrequencyWindow;
		[Widget] private VBox vbZG;
		[Widget] private ComboBox cmbSplitPosts;
		[Widget] private Button btnClose;
	}
}
