using System;
using System.IO;
using System.Collections;
using System.Threading;
using Gtk;
using Glade;

namespace EF.ljArchive.Gtk.Components
{
	public class TreeViewAdvanced : TreeView
	{
	
		new public TreeViewColumn[] Columns
		{
			get { return base.Columns; }
			set
			{
				while (base.Columns.Length > 0)
					base.RemoveColumn(base.Columns[0]);
				foreach (TreeViewColumn tvc in value)
					base.AppendColumn(tvc);
			}
		}
	}
}