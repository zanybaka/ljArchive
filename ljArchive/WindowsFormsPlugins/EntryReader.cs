using EF.ljArchive.Common;
using System;
using System.Drawing;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace EF.ljArchive.Plugins.WindowsForms
{
	/// <summary>
	/// Summary description for EntryReader.
	/// </summary>
	public class EntryReader : System.Windows.Forms.Form, IPlugin
	{
		private System.Windows.Forms.CheckBox chkShowBalloon;
		private System.Windows.Forms.Button btnUnload;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public EntryReader()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			InitializeDialog();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(EntryReader));
			this.btnUnload = new System.Windows.Forms.Button();
			this.chkShowBalloon = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// btnUnload
			// 
			this.btnUnload.Location = new System.Drawing.Point(8, 80);
			this.btnUnload.Name = "btnUnload";
			this.btnUnload.Size = new System.Drawing.Size(88, 23);
			this.btnUnload.TabIndex = 1;
			this.btnUnload.Text = "Unload";
			this.btnUnload.Click += new System.EventHandler(this.btnUnload_Click);
			// 
			// chkShowBalloon
			// 
			this.chkShowBalloon.Checked = true;
			this.chkShowBalloon.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkShowBalloon.Location = new System.Drawing.Point(112, 48);
			this.chkShowBalloon.Name = "chkShowBalloon";
			this.chkShowBalloon.TabIndex = 4;
			this.chkShowBalloon.Text = "Show Balloon";
			this.chkShowBalloon.CheckedChanged += new System.EventHandler(this.chkShowBalloon_CheckedChanged);
			// 
			// EntryReader
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(226, 114);
			this.Controls.Add(this.chkShowBalloon);
			this.Controls.Add(this.btnUnload);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "EntryReader";
			this.ShowInTaskbar = false;
			this.Text = "Entry Reader Options";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.EntryReader_Closing);
			this.ResumeLayout(false);

		}
		#endregion

		#region IPlugin Members
		public Image MenuIcon
		{
			get { return new System.Drawing.Bitmap(this.GetType(), "res.EntryReader.png"); }
		}

		public string Title
		{
			get { return "Entry Reader"; }
		}

		public object Settings
		{
			get { return settings; }
			set { settings = (EntryReaderSettings) value; }
		}

		public string Description
		{
			get { return "Displays an agent who reads your journal."; }
		}

		public string Author
		{
			get { return "Erik Frey"; }
		}

		public void Go(Journal j)
		{
			this.j = j;
			chkShowBalloon.Checked = settings.ShowBalloon;
		}

		public string URL
		{
			get { return "http://ljarchive.sourceforge.net/"; }
		}

		public Version Version
		{
			get { return new Version(0, 9, 4, 1); }
		}

		public int SelectedEventID
		{
			set
			{
				selectedEventID = value;
			}
		}
		#endregion

		private void InitializeDialog()
		{
			DirectoryInfo diSystem = new DirectoryInfo(Environment.SystemDirectory);
			loaded = false;
			okToUnload = false;
			settings = new EntryReaderSettings(Point.Empty, true);
		}

		private EntryReaderSettings settings;
		private bool loaded;
		private bool okToUnload;
		private Journal j;
		private ArrayList animations;
		private int selectedEventID = 0;

		private void cmbCharacter_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			animations = new ArrayList();
			if (!loaded)
			{
				loaded = true;
			}
		}

		private void a_Command(object sender)
		{
			this.Show();
			this.BringToFront();
		}

		private void a_HideEvent(object sender)
		{
			okToUnload = true;
			this.Close();
		}

		private void btnUnload_Click(object sender, System.EventArgs e)
		{
			okToUnload = true;
			this.Close();
		}

		private void EntryReader_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			this.Hide();
			if (okToUnload)
			{
				okToUnload = false;
				loaded = false;
			}
		}

		private void agentHost_MoveEvent(object sender)
		{
			settings.Location = new Point(0, 0);
		}

		private void chkShowBalloon_CheckedChanged(object sender, System.EventArgs e)
		{
			settings.ShowBalloon = chkShowBalloon.Checked;
		}

		private void chkAutoSpeak_CheckedChanged(object sender, System.EventArgs e)
		{
		}
	}

	#region EntryReaderSettings
	[Serializable()]
	public class EntryReaderSettings
	{
		public EntryReaderSettings(Point location, bool showBalloon)
		{
			this.location = location;
			this.showBalloon = showBalloon;
		}

		public Point Location
		{
			get { return location; }
			set { location = value; }
		}

		public bool ShowBalloon
		{
			get { return showBalloon; }
			set { showBalloon = value; }
		}

		private Point location;
		private bool showBalloon;
	}
	#endregion
}
