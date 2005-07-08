using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace EF.ljArchive.WindowsForms
{
	public class NewJournalArchiveAccountSettings : Genghis.Windows.Forms.WizardPage
	{
		private System.Windows.Forms.CheckBox chkAlternateServer;
		private System.Windows.Forms.TextBox txtServerURL;
		private System.Windows.Forms.GroupBox grpLogin;
		private System.Windows.Forms.TextBox txtLogin;
		private System.Windows.Forms.Label lblLogin;
		private System.Windows.Forms.Label lblAlternateServer;
		private System.Windows.Forms.Label lblPassword;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.GroupBox grpAlternateServer;
		private System.Windows.Forms.CheckBox chkGetComments;
		private System.Windows.Forms.GroupBox grpOptions;
		private System.ComponentModel.IContainer components = null;

		public NewJournalArchiveAccountSettings(string title, string description) : base (title, description)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			Localizer.SetControlText(this.GetType(), grpLogin, lblLogin, lblPassword, grpOptions, chkGetComments,
				grpAlternateServer, lblAlternateServer, chkAlternateServer);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lblAlternateServer = new System.Windows.Forms.Label();
			this.chkAlternateServer = new System.Windows.Forms.CheckBox();
			this.txtServerURL = new System.Windows.Forms.TextBox();
			this.grpLogin = new System.Windows.Forms.GroupBox();
			this.txtLogin = new System.Windows.Forms.TextBox();
			this.txtPassword = new System.Windows.Forms.TextBox();
			this.lblPassword = new System.Windows.Forms.Label();
			this.lblLogin = new System.Windows.Forms.Label();
			this.grpAlternateServer = new System.Windows.Forms.GroupBox();
			this.chkGetComments = new System.Windows.Forms.CheckBox();
			this.grpOptions = new System.Windows.Forms.GroupBox();
			this.grpLogin.SuspendLayout();
			this.grpAlternateServer.SuspendLayout();
			this.grpOptions.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblAlternateServer
			// 
			this.lblAlternateServer.Location = new System.Drawing.Point(8, 16);
			this.lblAlternateServer.Name = "lblAlternateServer";
			this.lblAlternateServer.Size = new System.Drawing.Size(368, 24);
			this.lblAlternateServer.TabIndex = 2;
			this.lblAlternateServer.Text = "If you are trying to connect to a different LiveJournal server, check here:";
			// 
			// chkAlternateServer
			// 
			this.chkAlternateServer.Location = new System.Drawing.Point(24, 48);
			this.chkAlternateServer.Name = "chkAlternateServer";
			this.chkAlternateServer.Size = new System.Drawing.Size(320, 16);
			this.chkAlternateServer.TabIndex = 3;
			this.chkAlternateServer.Text = "I want to connect to the following alternate address:";
			this.chkAlternateServer.CheckedChanged += new System.EventHandler(this.chkAlternateServer_CheckedChanged);
			// 
			// txtServerURL
			// 
			this.txtServerURL.Enabled = false;
			this.txtServerURL.Location = new System.Drawing.Point(24, 72);
			this.txtServerURL.Name = "txtServerURL";
			this.txtServerURL.Size = new System.Drawing.Size(352, 20);
			this.txtServerURL.TabIndex = 4;
			this.txtServerURL.Text = "http://www.livejournal.com";
			// 
			// grpLogin
			// 
			this.grpLogin.Controls.Add(this.txtLogin);
			this.grpLogin.Controls.Add(this.txtPassword);
			this.grpLogin.Controls.Add(this.lblPassword);
			this.grpLogin.Controls.Add(this.lblLogin);
			this.grpLogin.Location = new System.Drawing.Point(8, 64);
			this.grpLogin.Name = "grpLogin";
			this.grpLogin.Size = new System.Drawing.Size(224, 88);
			this.grpLogin.TabIndex = 5;
			this.grpLogin.TabStop = false;
			this.grpLogin.Text = "Login Information";
			// 
			// txtLogin
			// 
			this.txtLogin.Location = new System.Drawing.Point(88, 22);
			this.txtLogin.Name = "txtLogin";
			this.txtLogin.Size = new System.Drawing.Size(128, 20);
			this.txtLogin.TabIndex = 0;
			this.txtLogin.Text = "";
			this.txtLogin.TextChanged += new System.EventHandler(this.txtLogin_TextChanged);
			// 
			// txtPassword
			// 
			this.txtPassword.Location = new System.Drawing.Point(88, 56);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.PasswordChar = '*';
			this.txtPassword.Size = new System.Drawing.Size(128, 20);
			this.txtPassword.TabIndex = 2;
			this.txtPassword.Text = "";
			this.txtPassword.TextChanged += new System.EventHandler(this.txtPassword_TextChanged);
			// 
			// lblPassword
			// 
			this.lblPassword.Location = new System.Drawing.Point(8, 58);
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.Size = new System.Drawing.Size(80, 16);
			this.lblPassword.TabIndex = 3;
			this.lblPassword.Text = "Password:";
			// 
			// lblLogin
			// 
			this.lblLogin.Location = new System.Drawing.Point(8, 24);
			this.lblLogin.Name = "lblLogin";
			this.lblLogin.Size = new System.Drawing.Size(80, 16);
			this.lblLogin.TabIndex = 1;
			this.lblLogin.Text = "Login:";
			// 
			// grpAlternateServer
			// 
			this.grpAlternateServer.Controls.Add(this.txtServerURL);
			this.grpAlternateServer.Controls.Add(this.chkAlternateServer);
			this.grpAlternateServer.Controls.Add(this.lblAlternateServer);
			this.grpAlternateServer.Location = new System.Drawing.Point(8, 160);
			this.grpAlternateServer.Name = "grpAlternateServer";
			this.grpAlternateServer.Size = new System.Drawing.Size(384, 104);
			this.grpAlternateServer.TabIndex = 7;
			this.grpAlternateServer.TabStop = false;
			this.grpAlternateServer.Text = "Alternate Server";
			// 
			// chkGetComments
			// 
			this.chkGetComments.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.chkGetComments.Checked = true;
			this.chkGetComments.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkGetComments.Location = new System.Drawing.Point(8, 24);
			this.chkGetComments.Name = "chkGetComments";
			this.chkGetComments.Size = new System.Drawing.Size(136, 40);
			this.chkGetComments.TabIndex = 8;
			this.chkGetComments.Text = "Download comments";
			this.chkGetComments.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// grpOptions
			// 
			this.grpOptions.Controls.Add(this.chkGetComments);
			this.grpOptions.Location = new System.Drawing.Point(240, 64);
			this.grpOptions.Name = "grpOptions";
			this.grpOptions.Size = new System.Drawing.Size(152, 88);
			this.grpOptions.TabIndex = 9;
			this.grpOptions.TabStop = false;
			this.grpOptions.Text = "Options";
			// 
			// NewJournalArchiveAccountSettings
			// 
			this.Controls.Add(this.grpOptions);
			this.Controls.Add(this.grpAlternateServer);
			this.Controls.Add(this.grpLogin);
			this.Name = "NewJournalArchiveAccountSettings";
			this.Enter += new System.EventHandler(this.NewJournalArchiveAccountSettings_Enter);
			this.Leave += new System.EventHandler(this.NewJournalArchiveAccountSettings_Leave);
			this.Controls.SetChildIndex(this.grpLogin, 0);
			this.Controls.SetChildIndex(this.grpAlternateServer, 0);
			this.Controls.SetChildIndex(this.grpOptions, 0);
			this.grpLogin.ResumeLayout(false);
			this.grpAlternateServer.ResumeLayout(false);
			this.grpOptions.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void chkAlternateServer_CheckedChanged(object sender, System.EventArgs e)
		{
			txtServerURL.Enabled = chkAlternateServer.Checked;
		}

		private void txtLogin_TextChanged(object sender, System.EventArgs e)
		{
			ValidateTextBoxes();
		}

		private void txtPassword_TextChanged(object sender, System.EventArgs e)
		{
			ValidateTextBoxes();
		}

		private void NewJournalArchiveAccountSettings_Enter(object sender, System.EventArgs e)
		{
			ValidateTextBoxes();
		}

		private void ValidateTextBoxes()
		{
			this.WizardSheet.EnableNextButton = txtLogin.Text.Length > 0 && txtPassword.Text.Length > 0;
		}

		private void NewJournalArchiveAccountSettings_Leave(object sender, System.EventArgs e)
		{
			txtServerURL.Text = txtServerURL.Text.TrimEnd('/');
		}

		public string UserName
		{
			get
			{
				return txtLogin.Text;
			}
		}

		public string Password
		{
			get
			{
				return txtPassword.Text;
			}
		}

		public string ServerURL
		{
			get
			{
				return txtServerURL.Text;
			}
		}

		public bool GetComments
		{
			get
			{
				return chkGetComments.Checked;
			}
		}
	}
}

