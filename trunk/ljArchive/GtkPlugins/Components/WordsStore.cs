using System;
using System.Collections;
using Gtk;
using Glade;

namespace EF.ljArchive.Plugins.Gtk.Components
{
	public class WordsStore : ListStore
	{
		public WordsStore(Hashtable ht, int minWordSize, int minWordOccurrence) : base(typeof(string), typeof(int))
		{
			if (ht == null)
				return;

			IDictionaryEnumerator ide = ht.GetEnumerator();
			while (ide.MoveNext())
			{
				string s = (string) ide.Key;
				if (s.Length >= minWordSize && (int) ide.Value >= minWordOccurrence)
					this.AppendValues(s, ide.Value);
			}
		}
	}
}