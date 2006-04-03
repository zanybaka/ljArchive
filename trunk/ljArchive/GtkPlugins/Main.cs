// project created on 4/1/2006 at 5:07 PM
using System;
using Gtk;

class MainClass
{
	public static void Main (string[] args)
	{
		Application.Init ();
		new MyWindow ();
		Application.Run ();
	}
}