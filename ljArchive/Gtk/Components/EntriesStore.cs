using System;
using Gtk;
using Glade;
using System.Text.RegularExpressions;

namespace EF.ljArchive.Gtk.Components
{
	public class EntriesStore : ListStore
	{
		public EntriesStore(EF.ljArchive.Engine.Journal j) : base(typeof(string), typeof(string), typeof(string), typeof(string), typeof(string))
		{
			if (j == null)
				return;
			foreach (EF.ljArchive.Engine.Journal.EventsRow er in j.Events)
			{
				string subject = er.IsSubjectNull() ? string.Empty : CleanHTML(er.Subject);
				string body = er.IsBodyNull() ? string.Empty : CleanHTML(er.Body);
				string security = er.IsSecurityNull() ? string.Empty : er.Security;
				this.AppendValues(security, er.ID.ToString(), er.Date.ToString(), subject, body);
			}
		}
		
		static private string CleanHTML(string s)
		{
			string ret = r.Replace(s, " ");
			if (ret.Length > maxTextLength)
				ret = ret.Substring(0, maxTextLength - 3) + "...";
			return ret;
		}
		
		static private Regex r = new Regex(@"<.*?>|\n");
		static private readonly int maxTextLength = 256;
	}
}