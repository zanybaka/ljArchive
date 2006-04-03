using System;
using System.Collections;
using System.IO;
using Gtk;
using Glade;

namespace EF.ljArchive.Gtk
{
	public class CommandCollection
	{
		public CommandCollection (Glade.XML gxml, Engine.Collections.PluginCollection plugins)
		{
			gxml.Autoconnect(this);
			InitializeCommands(plugins);
		}
		
		protected void InitializeCommands(Engine.Collections.PluginCollection plugins)
		{
			ArrayList almi = new ArrayList();
			almi.AddRange(new MenuItem[] {
				mniFileNew,
				mniFileOpen,
				mniFileQuit,
				mniEditJournalSettings,
				mniEditHTMLSettings,
				mniEditPreferences,
				mniViewNone,
				mniViewEntries,
				mniViewComments,
				mniViewPlugins,
				mniHelpAbout });
			toolButtons = new ToolButton[] {
				tbBack,
				tbForward,
				tbSync,
				tbStop,
				tbPlugins };
			mnuViewPlugins = new Menu();
			this.plugins = new Hashtable();
			foreach (Common.IPlugin p in plugins)
			{
				ImageMenuItem imi = new ImageMenuItem(p.Title);
				mnuViewPlugins.Append(imi);
				almi.Add(imi);
				imi.Show();
				this.plugins.Add(imi, p);
			}
			mniViewPlugins.Submenu = mnuViewPlugins;
			menuItems = (MenuItem[]) almi.ToArray(typeof(MenuItem));
		}

		protected void UpdateCommands()
		{
			switch(commandState)
			{
				case CommandStates.FileClosed:
					this.tbBack.Sensitive = false;
					this.tbForward.Sensitive = false;
					this.tbSync.Sensitive = false;
					this.tbStop.Sensitive = false;
					this.tbPlugins.Sensitive = false;
					this.mniFileNew.Sensitive = true;
					this.mniFileOpen.Sensitive = true;
					this.mniEditJournalSettings.Sensitive = false;
					this.mniViewNone.Sensitive = false;
					this.mniViewEntries.Sensitive = false;
					this.mniViewComments.Sensitive = false;
					this.mniViewPlugins.Sensitive = false;
					break;
				case CommandStates.Syncing:
					this.tbBack.Sensitive = true;
					this.tbForward.Sensitive = true;
					this.tbSync.Sensitive = false;
					this.tbStop.Sensitive = true;
					this.tbPlugins.Sensitive = false;
					this.mniFileNew.Sensitive = false;
					this.mniFileOpen.Sensitive = false;
					this.mniEditJournalSettings.Sensitive = false;
					this.mniViewNone.Sensitive = true;
					this.mniViewEntries.Sensitive = true;
					this.mniViewComments.Sensitive = true;
					this.mniViewPlugins.Sensitive = false;
					break;
				case CommandStates.FileOpened:
					this.tbBack.Sensitive = true;
					this.tbForward.Sensitive = true;
					this.tbSync.Sensitive = true;
					this.tbStop.Sensitive = false;
					this.tbPlugins.Sensitive = true;
					this.mniFileNew.Sensitive = true;
					this.mniFileOpen.Sensitive = true;
					this.mniEditJournalSettings.Sensitive = true;
					this.mniViewNone.Sensitive = true;
					this.mniViewEntries.Sensitive = true;
					this.mniViewComments.Sensitive = true;
					this.mniViewPlugins.Sensitive = true;
					break;
			}
		}
		
		internal TreeViewStates TreeViewState
		{
			set
			{
				// temporarily unhook eventhandler
				if (menuItem_Activated != null)
				{
					mniViewNone.Activated -= menuItem_Activated;
					mniViewEntries.Activated -= menuItem_Activated;
					mniViewComments.Activated -= menuItem_Activated;
				}
				switch (value)
				{
					case TreeViewStates.Hidden: mniViewNone.Active = true; break;
					case TreeViewStates.Entries: mniViewEntries.Active = true; break;
					case TreeViewStates.Comments: mniViewComments.Active = true; break;
				}
				// rehook
				if (menuItem_Activated != null)
				{
					mniViewNone.Activated += menuItem_Activated;
					mniViewEntries.Activated += menuItem_Activated;
					mniViewComments.Activated += menuItem_Activated;
				}
			}
		}
		
		internal CommandStates CommandState
		{
			get { return commandState; }
			set
			{
				commandState = value;
				UpdateCommands();
			}
		}
		
		public bool IsPlugin(MenuItem mi)
		{
			return plugins.ContainsKey(mi);
		}
		
		public Common.IPlugin GetPlugin(MenuItem mi)
		{
			return (Common.IPlugin) plugins[mi];
		}

		public System.EventHandler MenuItem_Activated
		{
			get { return menuItem_Activated; }
			set
			{
				if (menuItem_Activated != null)
					foreach (MenuItem mi in menuItems)
						mi.Activated -= menuItem_Activated;
				menuItem_Activated = value;
				if (menuItem_Activated != null)
					foreach (MenuItem mi in menuItems)
						mi.Activated += menuItem_Activated;
			}
		}

		public System.EventHandler ToolButton_Clicked
		{
			get { return toolButton_Clicked; }
			set
			{
				if (toolButton_Clicked != null)
					foreach (ToolButton tb in toolButtons)
						tb.Clicked -= toolButton_Clicked;
				toolButton_Clicked = value;
				if (toolButton_Clicked != null)
					foreach (ToolButton tb in toolButtons)
						tb.Clicked += toolButton_Clicked;
			}
		}
		
		[Widget] internal MenuItem mniFileNew;
		[Widget] internal MenuItem mniFileOpen;
		[Widget] internal MenuItem mniFileQuit;
		[Widget] internal MenuItem mniEditJournalSettings;
		[Widget] internal MenuItem mniEditHTMLSettings;
		[Widget] internal MenuItem mniEditPreferences;
		[Widget] internal RadioMenuItem mniViewNone;
		[Widget] internal RadioMenuItem mniViewEntries;
		[Widget] internal RadioMenuItem mniViewComments;
		[Widget] internal MenuItem mniViewPlugins;
		[Widget] internal MenuItem mniHelpAbout;
		[Widget] internal ToolButton tbBack;
		[Widget] internal ToolButton tbForward;
		[Widget] internal ToolButton tbSync;
		[Widget] internal ToolButton tbStop;
		[Widget] internal ToolButton tbPlugins;
		private CommandStates commandState;
		private System.EventHandler menuItem_Activated;
		private System.EventHandler toolButton_Clicked;
		private MenuItem[] menuItems;
		private ToolButton[] toolButtons;
		private Menu mnuViewPlugins;
		private Hashtable plugins;
	}
}