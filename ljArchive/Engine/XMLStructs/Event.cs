using System;
using CookComputing.XmlRpc;

namespace EF.ljArchive.Engine.XMLStructs
{
	/// <summary>
	/// An XML Event.
	/// </summary>
	public struct Event
	{
		public Event(int itemid, string eventtime, string security, System.Int32 allowmask, string subject,
			string eventText, string poster, EventProps props)
		{
			this.itemid = itemid;
			this.eventtime = eventtime;
			this.security = security;
			this.allowmask = allowmask;
			this.subject = subject;
			this.eventText = eventText;
			this.poster = poster;
			this.props = props;
		}
		public int itemid;
		public string eventtime;
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public string security;
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public System.Int32 allowmask;
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public string subject;
		[XmlRpcMember("event")]
		public string eventText;
		[XmlRpcMissingMapping(MappingAction.Ignore)]
		public string poster;
		public EventProps props;
	}
}
