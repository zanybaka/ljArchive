using System;
using System.Threading;
using System.Globalization;
using System.IO;
using System.Net;
using System.Xml;
using EF.ljArchive.Engine.XMLStructs;
using EF.ljArchive.Engine.Collections;

namespace EF.ljArchive.Engine
{
	/// <summary>
	/// Callback for the <see cref="Sync"/> class.
	/// </summary>
	public delegate void SyncOperationCallBack(SyncOperationEventArgs soe);

	#region SyncOperationEventArgs
	/// <summary>
	/// <see cref="Sync"/> callback event arguments.
	/// </summary>
	/// <remarks><para><see cref="Sync"/> periodically calls a supplied delegate to inform the calling object of its
	/// status.  It provides this class as its arguments.</para>
	/// <para><see cref="Param1"/> typically designates the amount completed in an operation.</para>
	/// <para><see cref="Param2"/> designates the amount total of the operation.</para>
	/// <para>The only exception to this is when the returned <see cref="EF.ljArchive.Engine.SyncOperation"/>
	/// parameter is <see cref="EF.ljArchive.Engine.SyncOperation.Success"/>.  Then, <see cref="Param1"/> designates
	/// the number of journal events downloaded, and <see cref="Param2"/> designates the number of comments downloaded.
	/// </para></remarks>
	public class SyncOperationEventArgs : System.EventArgs
	{
		/// <summary>
		/// Creates a new instance of <see cref="SyncOperationEventArgs"/>.
		/// </summary>
		/// <param name="syncOperation">Describes the operation that <see cref="Sync"/> is currently performing.</param>
		/// <param name="param1">The first parameter.</param>
		/// <param name="param2">The second parameter.</param>
		public SyncOperationEventArgs(SyncOperation syncOperation, int param1, int param2)
		{
			this.syncOperation = syncOperation;
			this.param1 = param1;
			this.param2 = param2;
		}

		/// <summary>
		/// Describes the operation that <see cref="Sync"/> is currently performing.
		/// </summary>
		public SyncOperation SyncOperation  {get { return syncOperation; } set { syncOperation = value;  } }
		/// <summary>
		/// The first parameter.
		/// </summary>
		/// <remarks><para>For every <see cref="EF.ljArchive.Engine.SyncOperation"/> except
		/// <see cref="EF.ljArchive.Engine.SyncOperation.Success"/>, this parameter designates the amount completed
		/// of an operation.</para><para>For <see cref="EF.ljArchive.Engine.SyncOperation.Success"/>, this parameter
		/// designates the number of entries downloaded.</para></remarks>
		public int           Param1			{get { return param1;		 } set { param1 = value;		 } }
		/// <summary>
		/// The second parameter.
		/// </summary>
		/// <remarks><para>For every <see cref="EF.ljArchive.Engine.SyncOperation"/> except
		/// <see cref="EF.ljArchive.Engine.SyncOperation.Success"/>, this parameter designates the amount total
		/// of an operation.</para><para>For <see cref="EF.ljArchive.Engine.SyncOperation.Success"/>, this parameter
		/// designates the number of comments downloaded.</para></remarks>
		public int           Param2			{get { return param2;		 } set { param2 = value;		 } }

		private SyncOperation syncOperation;
		private int param1;
		private int param2;
	}
	#endregion

	#region ExpectedSyncException
	/// <summary>
	/// Represents an expected error that occurs during a sync.
	/// </summary>
	public class ExpectedSyncException : System.Exception
	{
		/// <summary>
		/// Creates a new instance of <see cref="ExpectedSyncException"/>.
		/// </summary>
		/// <param name="expectedError">The expected error category that occurred.</param>
		/// <param name="innerException">The exception that caused this exception.</param>
		public ExpectedSyncException(ExpectedError expectedError, Exception innerException) : base(
			"An expected exception occurred: " + expectedError.ToString(), innerException)
		{
			this.expectedError = expectedError;
		}

		/// <summary>
		/// Gets or sets the expected error category that occurred.
		/// </summary>
		public ExpectedError ExpectedError
		{
			get {return this.expectedError;}
			set {this.expectedError = value;}
		}

		private ExpectedError expectedError;
	}
	#endregion

	/// <summary>
	/// Syncs a <see cref="Journal"/>.
	/// </summary>
	/// <remarks><para>The <see cref="Sync"/> class performs all its operations automatically on a separate thread.</para>
	/// <para>It uses a supplied <see cref="SyncOperationCallBack"/> delegate to update the caller on its
	/// status as it goes through different phases of the sync operation.</para>
	/// <para><see cref="Sync"/> is thread safe, except that the calling class must be careful not to access the
	/// <see cref="Journal"/> it provided while <see cref="Sync"/> is operating.  The <see cref="Journal"/> must not
	/// be databound or accessed until <see cref="Sync"/> return a success or failure.</para></remarks>
	/// <example>Here's an example of using <see cref="Sync"/> to sync a journal:
	/// <code>
	///private void Go()
	///{
	///	Sync.Start(journal, new SyncOperationCallBack(Sync_SyncOperationCallBack));
	///}
	///
	///private void Sync_SyncOperationCallBack(SyncOperationEventArgs soe)
	///{
	///	if (soe.SyncOperation == SyncOperation.Failure)
	///	{
	///		MessageBox.Show("oh no!");
	///	}
	///	else if (soe.SyncOperation == SyncOperation.Success)
	///	{
	///		MessageBox.Show("success!");
	///		journal.Save();
	///	}
	///	else
	///	{
	///		label1.Text = "Current operation: " + soe.SyncOperation.ToString();
	///	}
	///}
	///
	/// </code></example>
	public class Sync
	{
		#region Private Instance Constructors
		/// <summary>
		/// This is a static class.
		/// </summary>
		private Sync() {}
		#endregion
		
		#region Public Static Methods
		/// <summary>
		/// Begins a <see cref="Journal"/> sync.
		/// </summary>
		/// <param name="j">The <see cref="Journal"/> to sync.</param>
		/// <param name="socb">The callback method to use for notification.</param>
		/// <example>Here's an example of using <see cref="Sync"/> to sync a journal:
		/// <code>
		///private void Go()
		///{
		///	Sync.Start(journal, new SyncOperationCallBack(Sync_SyncOperationCallBack));
		///}
		///
		///private void Sync_SyncOperationCallBack(SyncOperationEventArgs soe)
		///{
		///	if (soe.SyncOperation == SyncOperation.Failure)
		///	{
		///		MessageBox.Show("oh no!");
		///	}
		///	else if (soe.SyncOperation == SyncOperation.Success)
		///	{
		///		MessageBox.Show("success!");
		///		journal.Save();
		///	}
		///	else
		///	{
		///		label1.Text = "Current operation: " + soe.SyncOperation.ToString();
		///	}</code></example>
		static public void Start(Journal j, SyncOperationCallBack socb)
		{
			Sync.j = j;
			Sync.socb = socb;
			t = new Thread(new ThreadStart(Sync.ThreadStart));
			t.CurrentCulture = CultureInfo.InvariantCulture;
			t.Start();
		}

		/// <summary>
		/// Aborts a sync.
		/// </summary>
		static public void Abort()
		{
			t.Abort();
		}
		#endregion

		#region Public Static Properties
		/// <summary>
		/// Gets the exception that <see cref="Sync"/> encountered that caused it to fail.
		/// </summary>
		static public Exception SyncException
		{
			get
			{
				return syncException;
			}
		}

		/// <summary>
		/// Returns true if a <see cref="Sync"/> operation is occurring.
		/// </summary>
		static public bool IsAlive
		{
			get { return (t != null && t.IsAlive); }
		}
		#endregion

		#region Private Static Methods
		static private void ThreadStart()
		{
			// The main threaded execution body for performing a sync.
			// This method is chopped up into smaller methods for clarity and structure.
			ILJServer iLJ;
			Journal.OptionsRow or = null;
			SyncItemCollection sic = null, deletedsic = null;
            EventCollection ec = null;
			CommentCollection ccbody = null, ccmeta = null;
			UserMapCollection umc = null;
			LoginResponse lr = new LoginResponse();
			string communityPicURL = null;
			DateTime lastSync = DateTime.MinValue;
			SessionGenerateResponse sgr;
			int serverMaxID, localMaxID;

			try
			{
				// STEP 1: Initialize
				socb(new SyncOperationEventArgs(SyncOperation.Initialize, 0, 0));
				syncException = null;
				or = j.Options;
				iLJ = LJServerFactory.Create(or.ServerURL);
				sic = new SyncItemCollection();
				deletedsic = new SyncItemCollection();
				ec = new EventCollection();
				ccmeta = new CommentCollection();
				ccbody = new CommentCollection();
				umc = new UserMapCollection();

				// STEP 2: Login
				socb(new SyncOperationEventArgs(SyncOperation.Login, 0, 0));
				lr = new LoginResponse();
				Login(or, iLJ, ref lr, ref communityPicURL);

				// STEP 3: SyncItems
				socb(new SyncOperationEventArgs(SyncOperation.SyncItems, 0, 0));
				lastSync = DateTime.MinValue;
				SyncItems(or, iLJ, ref sic, ref deletedsic, ref lastSync);

				// STEP 4: GetEvents
				socb(new SyncOperationEventArgs(SyncOperation.GetEvents, 0, 0));
				GetEvents(or, iLJ, ref sic, ref deletedsic, ref ec);

				if (or.GetComments)
				{
					// STEP 5: SessionGenerate
					socb(new SyncOperationEventArgs(SyncOperation.SessionGenerate, 0, 0));
					sgr = new SessionGenerateResponse();
					SessionGenerate(or, iLJ, ref sgr);

					// STEP 6: ExportCommentsMeta
					socb(new SyncOperationEventArgs(SyncOperation.ExportCommentsMeta, 0, 0));
					localMaxID = serverMaxID = j.GetMaxCommentID();
					ExportCommentsMeta(or, iLJ, sgr, ref serverMaxID, localMaxID, umc, ccmeta);

					// STEP 7: ExportCommentsBody
					socb(new SyncOperationEventArgs(SyncOperation.ExportCommentsBody, 0, 0));
					ExportCommentsBody(or, iLJ, sgr, serverMaxID, localMaxID, ccbody);
				}
			}
			catch (Exception ex)
			{
				ParseException(ex, ref syncException);
				if (ex.GetType() == typeof(ThreadAbortException))
				{
					socb(new SyncOperationEventArgs(SyncOperation.Failure, 0, 0)); // do this before thread terminates
					return;
				}
			}

			// STEP 8: Merge
			try
			{
				if (syncException == null)
				{
					socb(new SyncOperationEventArgs(SyncOperation.Merge, 0, 0));
					Merge(j, ec, ccmeta, ccbody, umc, deletedsic, lr, communityPicURL, lastSync);
					socb(new SyncOperationEventArgs(SyncOperation.Success, ec.Count, ccbody.Count));
				}
				else if (syncException.GetType() == typeof(ExpectedSyncException)
					&& (((ExpectedSyncException) syncException).ExpectedError == ExpectedError.ServerNotResponding
					||  ((ExpectedSyncException) syncException).ExpectedError ==
					      ExpectedError.ExportCommentsNotSupported
					||  ((ExpectedSyncException) syncException).ExpectedError ==
					      ExpectedError.CommunityAccessDenied)
					&& lr.moods != null)
				{
					socb(new SyncOperationEventArgs(SyncOperation.Merge, 0, 0));
					if (sic.Count > 0)
						lastSync = DateTime.Parse(sic.GetOldest().time, CultureInfo.InvariantCulture).AddSeconds(-1);
					Merge(j, ec, ccmeta, ccbody, umc, deletedsic, lr, communityPicURL, lastSync);
					socb(new SyncOperationEventArgs(SyncOperation.PartialSync, ec.Count, ccbody.Count));
				}
				else
				{
					socb(new SyncOperationEventArgs(SyncOperation.Failure, 0, 0));
				}
			}
			catch (Exception ex)
			{
				syncException = ex;
				socb(new SyncOperationEventArgs(SyncOperation.Failure, 0, 0));
			}
		}

		static private void Login(Journal.OptionsRow or, ILJServer iLJ, ref LoginResponse lr, ref string communityPicURL)
		{
			// logging in to the server gets back a lot of assorted metadata we need to store
			GetChallengeResponse gcr;
			string auth_response;
			LoginParams lp;

			gcr = iLJ.GetChallenge();
			auth_response = MD5Hasher.Compute(gcr.challenge + or.HPassword);
			lp = new LoginParams(or.UserName, "challenge", gcr.challenge, auth_response, 1, clientVersion,
				j.GetMaxMoodID(), 0, 1, 1);
			lr = iLJ.Login(lp);
			// if downloading a community, we want the community's default user pic, not the user's
			if (!or.IsUseJournalNull())
				communityPicURL = Server.GetDefaultPicURL(or.UseJournal, or.ServerURL, true);
		}

		static private void SyncItems(Journal.OptionsRow or, ILJServer iLJ, ref SyncItemCollection sic,
			ref SyncItemCollection deletedsic, ref DateTime lastSync)
		{
			// syncitems returns a "meta" list of what events have changed since the last time we called syncitems
			// note that syncitems may be called more than once
			GetChallengeResponse gcr;
			string auth_response;
			SyncItemsParams sip;
			SyncItemsResponse sir;
			int total = -1, count = 0;

			lastSync = (or.IsLastSyncNull() ? DateTime.MinValue : or.LastSync);
			do
			{
				string lastSyncString = (lastSync == DateTime.MinValue ? string.Empty :
					lastSync.ToString(_datetimeformat));
				gcr = iLJ.GetChallenge();
				auth_response = MD5Hasher.Compute(gcr.challenge + or.HPassword);
				sip = new SyncItemsParams(or.UserName, "challenge", gcr.challenge, auth_response, 1,
				                          lastSyncString, (or.IsUseJournalNull() ? string.Empty : or.UseJournal));
				sir = iLJ.SyncItems(sip);
				total = (total == -1 ? sir.total : total);
				count += sir.count;
				sic.AddRangeLog(sir.syncitems);
				deletedsic.AddRangeLog(sir.syncitems);
				if (sic.GetMostRecentTime() > lastSync)
					lastSync = sic.GetMostRecentTime();
				socb(new SyncOperationEventArgs(SyncOperation.SyncItems, count, total));
			} while (sir.count < sir.total);
		}

		static private void GetEvents(Journal.OptionsRow or, ILJServer iLJ, ref SyncItemCollection sic,
			ref SyncItemCollection deletedsic, ref EventCollection ec)
		{
			// for an explanation of this algorithm, see
			// http://www.livejournal.com/community/lj_clients/143312.html
			// note that this is a very painful algorithm.  it will loop an extra time for each
			// deleted syncitem that getevents doesn't return an event for.  if LJ decides to revise
			// how they return syncitems, this algorithm can be made more efficient.
			int total = sic.Count;
			while (sic.Count > 0)
			{
				SyncItem oldest = sic.GetOldest();
				DateTime oldestTime = DateTime.Parse(oldest.time, CultureInfo.InvariantCulture).AddSeconds(-1);
				GetChallengeResponse gcr = iLJ.GetChallenge();
				string auth_response = MD5Hasher.Compute(gcr.challenge + or.HPassword);
				GetEventsParams gep = new GetEventsParams(or.UserName, "challenge", gcr.challenge,
					auth_response, 1, 0, 0, 0, "syncitems", oldestTime.ToString(_datetimeformat), 0, 0, 0, 0,
					string.Empty, 0, "unix", (or.IsUseJournalNull() ? string.Empty : or.UseJournal));
				GetEventsResponse ger;
				socb(new SyncOperationEventArgs(SyncOperation.GetEvents, total - sic.Count, total));
				ger = iLJ.GetEvents(gep);
				// remove this item in case it isn't returned by getevents
				// this signifies that the item has been deleted
				// this also ensures we don't get stuck in an endless loop
				sic.Remove(oldest);
				sic.RemoveDownloaded(ger.events);
				deletedsic.RemoveDownloaded(ger.events);
				ec.AddRange(ger.events);
			}
		}

		static private void SessionGenerate(Journal.OptionsRow or, ILJServer iLJ, ref SessionGenerateResponse sgr)
		{
			// a session needs to be generated to talk to the livejournal web server
			// right now there is no export comments method on xmlrpc, so we get comments the ol' fashioned way -
			// with a web request
			GetChallengeResponse gcr = iLJ.GetChallenge();
			string auth_response = MD5Hasher.Compute(gcr.challenge + or.HPassword);
			SessionGenerateParams sgp = new SessionGenerateParams(or.UserName, "challenge", gcr.challenge,
				auth_response, 1, "long", 0);
			sgr = iLJ.SessionGenerate(sgp);
		}

		static private void ExportCommentsMeta(Journal.OptionsRow or, ILJServer iLJ, SessionGenerateResponse sgr,
			ref int serverMaxID, int localMaxID, UserMapCollection umc, CommentCollection cc)
		{
			// this is a an unfortunately necessary step
			// we call export comments meta is to get the user map and to check if comment state has changed
			// it would be better to be able to provide a lastsync, but alas
			// see http://www.livejournal.com/developer/exporting.bml for more info
			int maxID = -1;
			while (maxID < serverMaxID)
			{
				Uri uri = new Uri(new Uri(or.ServerURL), string.Format(_exportcommentsmetapath, maxID + 1));
				if (!or.IsUseJournalNull())
					uri = new Uri(uri.AbsoluteUri + string.Format("&authas={0}", or.UseJournal));
				HttpWebRequest w = HttpWebRequestFactory.Create(uri.AbsoluteUri, sgr.ljsession);
				using (Stream s = w.GetResponse().GetResponseStream())
				{
					XmlTextReader xtr = new XmlTextReader(s);
					while (xtr.Read())
					{
						if (xtr.NodeType == XmlNodeType.Element && xtr.Name == "usermap")
						{
							string id = xtr.GetAttribute("id");
							string user = xtr.GetAttribute("user");
							if (id != null && user != null && !umc.ContainsID(XmlConvert.ToInt32(id)))
								umc.Add(new UserMap(XmlConvert.ToInt32(id), user));
						}
						else if (xtr.NodeType == XmlNodeType.Element && xtr.Name == "maxid")
						{
							xtr.Read();
							serverMaxID = XmlConvert.ToInt32(xtr.Value);
							socb(new SyncOperationEventArgs(SyncOperation.ExportCommentsMeta, Math.Max(maxID, 0), serverMaxID));
						}
						else if (xtr.NodeType == XmlNodeType.Element && xtr.Name == "comment")
						{
							string id = xtr.GetAttribute("id");
							string posterid = xtr.GetAttribute("posterid");
							string state = xtr.GetAttribute("state");
							if (posterid == null) posterid = "0";
							if (state == null) state = "A";
							if (id != null)
								cc.Add(new Comment(XmlConvert.ToInt32(id), XmlConvert.ToInt32(posterid),
								                   state, 0, 0, string.Empty, string.Empty, DateTime.MinValue));
						}
						else if (xtr.NodeType == XmlNodeType.Element && xtr.Name == "h2")
						{
							xtr.Read();
							if (xtr.Value == "Error" && !or.IsUseJournalNull())
								throw new ExpectedSyncException(ExpectedError.CommunityAccessDenied, null);
						}
					}
					xtr.Close();
				}
				maxID = cc.GetMaxID();
			}
		}

		static private void ExportCommentsBody(Journal.OptionsRow or, ILJServer iLJ, SessionGenerateResponse sgr,
			int serverMaxID, int localMaxID, CommentCollection cc)
		{
			// note that the export comments body web request may be called more than once
			int maxID = localMaxID;
			while (maxID < serverMaxID)
			{
				Uri uri = new Uri(new Uri(or.ServerURL), string.Format(_exportcommentsbodypath, maxID + 1));
				if (!or.IsUseJournalNull())
					uri = new Uri(uri.AbsoluteUri + string.Format("&authas={0}", or.UseJournal));
				HttpWebRequest w = HttpWebRequestFactory.Create(uri.AbsoluteUri, sgr.ljsession);
				socb(new SyncOperationEventArgs(SyncOperation.ExportCommentsBody, maxID - localMaxID,
					serverMaxID - localMaxID));
				using (Stream s = w.GetResponse().GetResponseStream())
				{
					System.Text.Encoding ec;
					if (System.Environment.Version.Major == 1) // .NET 2.0 utf8 cleans strings, so we don't have to
						ec = new UTF8Clean();
					else
						ec = System.Text.Encoding.UTF8;
					StreamReader sr = new StreamReader(s, ec);
					XmlCommentReader xcr = new XmlCommentReader(sr);
					while (xcr.Read())
						cc.Add(xcr.Comment);
					xcr.Close();
				}
				maxID = cc.GetMaxID();
			}
		}
		
		static private void ParseException(Exception ex, ref Exception result)
		{
			if (ex.GetType() == typeof(CookComputing.XmlRpc.XmlRpcFaultException))
			{
				CookComputing.XmlRpc.XmlRpcFaultException xfe = (CookComputing.XmlRpc.XmlRpcFaultException) ex;
				if (xfe.FaultString.IndexOf("repeated requests") > -1)
					result = new ExpectedSyncException(ExpectedError.RepeatedRequests, ex);
				else if (xfe.FaultString.IndexOf("Cannot display this post") > -1)
					result = new ExpectedSyncException(ExpectedError.NoEncodingSettings, ex);
				else if (xfe.FaultString.IndexOf("Invalid password") > -1)
					result = new ExpectedSyncException(ExpectedError.InvalidPassword, ex);
				else if (xfe.FaultString.IndexOf("Unknown method") > -1)
					result = new ExpectedSyncException(ExpectedError.ExportCommentsNotSupported, ex);
			}
			else if (ex.GetType() == typeof(CookComputing.XmlRpc.XmlRpcServerException))
			{
				CookComputing.XmlRpc.XmlRpcServerException xse = (CookComputing.XmlRpc.XmlRpcServerException) ex;
				if (   xse.Message.ToLower().IndexOf("not found") > -1
					|| xse.Message.ToLower().IndexOf("not implemented") > -1
					|| xse.Message.ToLower().IndexOf("not modified") > -1)
					result = new ExpectedSyncException(ExpectedError.XMLRPCNotSupported, ex);
				else
					result = new ExpectedSyncException(ExpectedError.ServerNotResponding, ex);
			}
			else if (ex.GetType() == typeof(ThreadAbortException))
			{
				result = new ExpectedSyncException(ExpectedError.Cancel, ex);
			}
			else if (ex.GetType() == typeof(System.Net.WebException))
			{
				System.Net.WebException wex = (System.Net.WebException) ex;
				if (wex.Status == System.Net.WebExceptionStatus.ProtocolError)
					result = new ExpectedSyncException(ExpectedError.ExportCommentsNotSupported, ex);
				else
					result = new ExpectedSyncException(ExpectedError.ServerNotResponding, ex);
			}
			else if (ex.GetType() == typeof(CookComputing.XmlRpc.XmlRpcIllFormedXmlException))
			{
				result = new ExpectedSyncException(ExpectedError.ServerNotResponding, ex);
			}
			if (result == null)
				result = ex;
		}

		static private void Merge(Journal j, EventCollection ec, CommentCollection ccMeta,
			CommentCollection ccBody, UserMapCollection umc, SyncItemCollection deletedsic, LoginResponse lr,
			string communityPicURL, DateTime lastSync)
		{
			j.AcceptChanges(); // row states must be set to unchanged for loaddatarow to merge properly

			// update moods
			j.Moods.BeginLoadData();
			foreach (Mood m in lr.moods)
				j.Moods.LoadDataRow(new object[] {m.id, m.name, m.parent}, false);
			j.Moods.EndLoadData();

			// update userpics
			j.UserPics.Clear();
			j.UserPics.BeginLoadData();
			for (int i = 0; i < lr.pickws.Length; ++i)
				j.UserPics.AddUserPicsRow(lr.pickws[i], lr.pickwurls[i]);
			j.UserPics.EndLoadData();

			// update users
			j.Users.BeginLoadData();
			foreach (UserMap u in umc)
				j.Users.LoadDataRow(new object[] {u.id, u.user}, false);
			j.Users.LoadDataRow(new object[] {0, "anonymous"}, false);
			j.Users.EndLoadData();

			// update new/updated journal events
			j.Events.BeginLoadData();
			foreach (Event e in ec)
				j.Events.LoadDataRow(new object[] {
										e.itemid,
										DateTime.Parse(e.eventtime, CultureInfo.InvariantCulture),
										e.security,
										e.allowmask,
										e.subject,
										e.eventText,
										e.poster,
										e.anum,
										e.props.current_mood,
										e.props.current_moodid,
										e.props.current_music,
										e.props.opt_preformatted == 1,
										e.props.opt_nocomments == 1,
										e.props.picture_keyword,
										e.props.opt_backdated == 1,
										e.props.opt_noemail == 1,
										e.props.unknown8bit == 1,
										e.props.hasscreened == 1,
										e.props.revnum,
										e.props.commentalter,
										e.props.syn_link,
										e.props.syn_id,
										new DateTime(1970, 1, 1).AddSeconds(e.props.revtime),
										e.props.taglist
				                     }, false);
			j.Events.EndLoadData();

			// update comment meta (posterid and state can change)
			j.Comments.BeginLoadData();
			foreach (Comment c in ccMeta)
			{
				Journal.CommentsRow cr = j.Comments.FindByID(c.id);
				if (cr != null)
				{
					cr.PosterID = c.posterid;
					cr.State = c.state;
				}
			}
			j.Comments.EndLoadData();
			
			// update comment bodies
			j.Comments.BeginLoadData();
			foreach (Comment c in ccBody)
				j.Comments.LoadDataRow(new object[] {
										c.id,
										c.posterid,
										c.state,
										c.jitemid,
										c.parentid,
										c.body,
										c.subject,
										c.date
				                       }, false);
			j.Comments.EndLoadData();

			// update deleted journal events
			if (deletedsic != null)
			{
				foreach (SyncItem s in deletedsic)
				{
					Common.Journal.EventsRow er =
						j.Events.FindByID(int.Parse(s.item.Substring(_syncitemtypelogprefix.Length)));
					if (er != null)
						j.Events.RemoveEventsRow(er);
				}
			}

			// update options
			j.Options.DefaultPicURL = (lr.defaultpicurl != null && lr.defaultpicurl.Length > 0 ?
				lr.defaultpicurl : null);
			j.Options.CommunityPicURL = communityPicURL;
			j.Options.FullName = (lr.fullname != null && lr.fullname.Length > 0 ? lr.fullname : null);
			j.Options.LastSync = lastSync;
		}
		#endregion

		#region Private Static Fields
		static private Thread t;
		static private Journal j;
		static private Exception syncException = null;
		static private SyncOperationCallBack socb;
		static private readonly string _datetimeformat = ConstReader.GetString("_datetimeformat");
		static private readonly string clientVersion = Environment.OSVersion.Platform.ToString() + "-.NET/" +
			System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(2);
		static private readonly string _exportcommentsmetapath = ConstReader.GetString("_exportcommentsmetapath");
		static private readonly string _exportcommentsbodypath = ConstReader.GetString("_exportcommentsbodypath");
		static private readonly string _syncitemtypelogprefix = ConstReader.GetString("_syncitemtypelogprefix");
		#endregion
	}
}
