using System;
using Gtk;
using Glade;
using System.Text.RegularExpressions;

namespace EF.ljArchive.Gtk.Components
{
	public class CommentsStore : ListStore
	{
		public CommentsStore(EF.ljArchive.Engine.Journal j) : base(typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string))
		{
			if (j == null)
				return;
			foreach (EF.ljArchive.Engine.Journal.CommentsRow cr in j.Comments)
			{
				string subject = cr.IsSubjectNull() ? string.Empty : CleanHTML(cr.Subject);
				string body = cr.IsBodyNull() ? string.Empty : CleanHTML(cr.Body);
				string state = cr.IsStateNull() ? string.Empty : cr.State;
				string username = cr.IsNull("PosterUserName") ? string.Empty : (string) cr["PosterUserName"];
				this.AppendValues(state, cr.ID.ToString(), username, cr.Date.ToString(), subject, body);
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