using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.IO;
using Root.Reports;
using EF.ljArchive.Common;

namespace EF.ljArchive.PDF
{
	/// <summary>
	/// Description of JournalReport.
	/// </summary>
	public class JournalReport : Report
	{

		public JournalReport(Journal j)
		{
			this.j = j;
			fd = new FontDef(this, FontDef.StandardFont.Helvetica);
			fp = new FontPropMM(fd, 2.1);
			fp_Bold = new FontPropMM(fd, 2.1, System.Drawing.Color.Blue);
			fp_Bold.bBold = true;
			fp_Header = new FontPropMM(fd, 2.6);
			flm = new FlowLayoutManager();
			flm.rContainerWidthMM = rPosRight - rPosLeft;
			flm.rContainerHeightMM = rPosBottom - rPosTop;
			flm.eNewContainer += new FlowLayoutManager.NewContainerEventHandler(Flm_NewContainer);
			
			userIcon = GetType().Assembly.GetManifestResourceStream("EF.ljArchive.PDF.res.userinfo.jpg");
			communityIcon = GetType().Assembly.GetManifestResourceStream("EF.ljArchive.PDF.res.community.jpg");
		}
		
		public void AddEvent(Journal.EventsRow er)
		{
			flm.Reset();
			System.Drawing.Bitmap b = null;
			Journal.UserPicsRow ur = null;
			if (!er.IsPictureKeywordNull())
				ur = j.UserPics.FindByPicKeyword(er.PictureKeyword);
			if (ur != null)
				b = new Bitmap(SimpleCache.Cache.GetStream(ur.PicURL));
			else if (!j.Options[0].IsDefaultPicURLNull() && er.Poster == j.Options[0].UserName)
				b = new Bitmap(SimpleCache.Cache.GetStream(j.Options[0].DefaultPicURL));
			if (b != null)
			{
				MemoryStream ms = new MemoryStream();
				b.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
				flm.AddBlock(new RepObj[] {new RepImageMM(ms, Double.NaN, Double.NaN)});
			}
			flm.AddBlock(fp_Header, "Poster: " + er.Poster);
			flm.AddBlock(fp_Header, "Date: " + er.Date.ToString());
			flm.NewLine(fp_Header.rLineFeed);
			foreach (string s in er.Body.Replace("\r", "").Split('\n'))
			{
				flm.AddBlock(Format(s));;
			}
		}
		
		private RepObj[] Format(string s)
		{
			ArrayList al = new ArrayList();
			foreach (string tag in _rtag.Split(s))
			{
				if (tag.StartsWith("<"))
				{
					string id = tag.Substring(1, tag.Length - 2).Trim().ToLower();
					if (id.StartsWith("lj"))
					{
						string mode = id.Substring(2).Trim();
						if (mode.StartsWith("user"))
						{
							string user = mode.Substring(4).Trim(new char[] {' ', '=', '"'});
							al.Add(new RepImageMM(userIcon, 4, Double.NaN));
							al.Add(new RepString(fp_Bold, user));
						}
						if (mode.StartsWith("comm"))
						{
							string comm = mode.Substring(4).Trim(new char[] {' ', '=', '"'});
							al.Add(new RepImageMM(communityIcon, 4, Double.NaN));
							al.Add(new RepString(fp_Bold, comm));
						}
					}
				}
				else
					al.Add(new RepString(fp, tag));
			}
			return (RepObj[]) al.ToArray(typeof(RepObj));
		}
		
		private void Flm_NewContainer(Object oSender, FlowLayoutManager.NewContainerEventArgs ea)
		{
			new Page(this);
		
			// first page with caption
			if (page_Cur.iPageNo == 1)
			{
				string s = j.Options[0].IsUseJournalNull() ? j.Options[0].UserName : j.Options[0].UseJournal;
				FontProp fp_Title = new FontPropMM(fd, 7);
				fp_Title.bBold = true;
				page_Cur.AddCT_MM(rPosLeft + (rPosRight - rPosLeft) / 2, rPosTop,
				                  new RepString(fp_Title, s + "'s Journal"));
				ea.container.rHeightMM -= fp_Title.rLineFeedMM;  // reduce height of table container for the first page
			}
		
			// the new container must be added to the current page
			page_Cur.AddMM(rPosLeft, rPosBottom - ea.container.rHeightMM, ea.container);
		}
		
		private FontDef fd;
		private FontProp fp;
		private FontProp fp_Header;
		private FontProp fp_Bold;
		private FlowLayoutManager flm;
		private Journal j;
		private Double rPosLeft = 20;  // millimeters
		private Double rPosRight = 195;  // millimeters
		private Double rPosTop = 24;  // millimeters
		private Double rPosBottom = 278;  // millimeters
		System.IO.Stream userIcon;
		System.IO.Stream communityIcon;
		static private readonly Regex _rtag = new Regex(@"(<.*?>)");
	}
}
