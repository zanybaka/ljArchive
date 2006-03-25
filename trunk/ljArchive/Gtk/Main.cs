// project created on 3/25/2006 at 1:04 PM
using System;
using Gtk;

namespace EF.ljArchive.Gtk
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			new MainController ();
			Application.Run ();
		}
	}
}