using System;

namespace EF.ljArchive.Gtk
{
	public enum CommandStates
	{
		FileClosed,
		Syncing,
		FileOpened
	}
	
	public enum TreeViewStates
	{
		Entries,
		Comments,
		Hidden
	}
}