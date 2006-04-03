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
	/// Summary description for CommentCount.
	/// </summary>
	public class CommentCount : IPlugin
	{

		public CommentCount()
		{
			InitializeWindow();
		}

		private void InitializeWindow()
		{
			// gxml
			this.gxml = new Glade.XML(null, "gtkplugins.glade", "CommentCountWindow", null);
			this.gxml.Autoconnect(this);
			
			// ZedGraphControl
			this.zg = new Components.ZedGraphControl();
			this.zg.RotateClockwise = true;
			this.zg.Show();
			
			// vbZG
			this.vbZG.PackStart(zg);
			this.vbZG.ReorderChild(zg, 0);

			
			// sbShowTop
			sbShowTop.ValueChanged += sbShowTop_ValueChanged;

			// btnClose
			this.btnClose.Clicked += btnClose_Clicked;
			
			// CommentCountWindow
			this.CommentCountWindow.DeleteEvent += CommentCountWindow_Delete;
		}

		#region IPlugin Members
		public System.Drawing.Image MenuIcon
		{
			get { return null; }
		}

		public string Description
		{
			get { return "Analyzes comment counts in your journal, both given and received."; }
		}

		public string Author
		{
			get { return "Erik Frey"; }
		}

		public void Go(Journal j)
		{
			this.j = j;
			this.CommentCountWindow.Show();
			using(new Components.Busy(CommentCountWindow))
			{
				c = new EF.ljArchive.Plugins.Core.CommentCount(j);
				zg.GraphPane = c.GetGraph((int) sbShowTop.Value, zg.Height - 1,
					zg.Width - 1);
			}
		}

		public string Title
		{
			get { return "Comment Count Analyzer"; }
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

		private void sbShowTop_ValueChanged(object sender, EventArgs e)
		{
			using(new Components.Busy(CommentCountWindow))
			{
				zg.GraphPane = c.GetGraph((int) sbShowTop.Value, zg.Height - 1,
					zg.Width - 1);
			}
		}
		
		private void btnClose_Clicked(object sender, EventArgs e)
		{
			CommentCountWindow.Hide();
		}
		
		private void CommentCountWindow_Delete(object sender, DeleteEventArgs de)
		{
			de.RetVal = true;
			CommentCountWindow.Hide();
		}

		private Journal j;
		private Glade.XML gxml;
		private Components.ZedGraphControl zg;
		private Plugins.Core.CommentCount c;
		[Widget] private Window CommentCountWindow;
		[Widget] private VBox vbZG;
		[Widget] private SpinButton sbShowTop;
		[Widget] private Button btnClose;
	}
}
