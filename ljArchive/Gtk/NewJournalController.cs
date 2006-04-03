using System;
using System.IO;
using Gtk;
using Glade;

namespace EF.ljArchive.Gtk
{
	public class NewJournalController
	{
		static NewJournalController()
		{
			njc = new NewJournalController();
		}

		public NewJournalController ()
		{
			InitializeWindow();
		}
		
		private void InitializeWindow()
		{
			// fcdNew
			gxml = new Glade.XML(null, "ljarchive.glade", "NewJournalDialog", null);
			gxml.Autoconnect(this);
			
			// NewJournalDialog
			this.NewJournalDialog.CurrentName = "archive.lja";
			this.NewJournalDialog.KeyReleaseEvent += NewJournalDialog_KeyRelease;
		}
		
		static public ResponseType Go()
		{
			ResponseType rt = (ResponseType) njc.NewJournalDialog.Run();
			njc.NewJournalDialog.Hide();
			return rt;
		}
		
		static public Engine.Journal Journal
		{
			get { return j; }
		}
		
		static public string Filename
		{
			get { return njc.NewJournalDialog.Filename; }
		}
		
		static public string ServerURL
		{
			get { return njc.entServer.Text; }
		}
		
		static public string UserName
		{
			get { return njc.entUserName.Text; }
		}
		
		static public string Password
		{
			get { return njc.entPassword.Text; }
		}
		
		static public bool GetComments
		{
			get { return njc.chkDownloadComments.Active; }
		}
		
		static public string UseJournal
		{
			get
			{
				if (njc.entCommunity.Text.Length > 0)
					return njc.entCommunity.Text;
				else
					return null;
			}
		}
		
		private void NewJournalDialog_KeyRelease(object sender, EventArgs e)
		{
			btnSave.Sensitive = (
			entUserName.Text.Length > 0
			&& entPassword.Text.Length > 0
			&& NewJournalDialog.Filename != null
			&& NewJournalDialog.Filename.Length > 0
			&& !Directory.Exists(NewJournalDialog.Filename));
		}

		[Widget]
		private Entry entUserName;
		[Widget]
		private Entry entPassword;
		[Widget]
		private Entry entServer;
		[Widget]
		private Entry entCommunity;
		[Widget]
		private CheckButton chkDownloadComments;
		[Widget]
		private FileChooserDialog NewJournalDialog;
		[Widget]
		private Button btnSave;
		static private NewJournalController njc;
		private Glade.XML gxml;
		static private Engine.Journal j;
	}
}