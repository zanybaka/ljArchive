using System;
using System.Collections;
using Gtk;
using Glade;

namespace EF.ljArchive.Plugins.Gtk.Components
{
	public class Busy : IDisposable
	{
		public Busy(Window window)
		{
			this.window = window.GdkWindow;
			this.window.Cursor = new Gdk.Cursor(Gdk.CursorType.Watch);
			while (Application.EventsPending())
				Application.RunIteration(false); // cursor needs time to think!
		}
		
		public void Dispose()
		{
			window.Cursor = null;
		}
		
		private Gdk.Window window;
	}
}