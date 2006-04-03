using System;
using System.IO;
using Gtk;
using Glade;

namespace EF.ljArchive.Gtk
{
	public class AboutController
	{
		static AboutController()
		{
			ac = new AboutController();
		}

		public AboutController ()
		{
			InitializeWindow();
		}
		
		private void InitializeWindow()
		{
			// fcdNew
			this.gxml = new Glade.XML(null, "ljarchive.glade", "AboutWindow", null);
			this.gxml.Autoconnect(this);
			
			this.AboutWindow.IconList = new Gdk.Pixbuf[] {
				new Gdk.Pixbuf(null, "App16x16.xpm"),
				new Gdk.Pixbuf(null, "App32x32.xpm")
			};
			
			// btnOK
			this.btnOK.Clicked += btnOK_Clicked;
			
			// lblAbout
			this.lblAbout.Text = string.Format(@"ljArchive {0}

by Erik Frey

http://sourceforge.net/projects/ljarchive", this.GetType().Assembly.GetName().Version.ToString(3));
		}
		
		static public void Go()
		{
			ac.AboutWindow.Show();
		}
		
		private void btnOK_Clicked(object sender, EventArgs e)
		{
			ac.AboutWindow.Hide();
		}
		
		[Widget]
		private Window AboutWindow;
		[Widget]
		private Button btnOK;
		[Widget]
		private Label lblAbout;
		static private AboutController ac;
		private Glade.XML gxml;
	}
}