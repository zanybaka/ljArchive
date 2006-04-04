using System;
using System.Collections;
using sharpPDF;
using sharpPDF.Enumerators;
using sharpPDF.Elements;
using EF.ljArchive.Common;

namespace EF.ljArchive.PDF
{
	/// <summary>
	/// Description of PDFJournalWriter.
	/// </summary>
	public class PDFJournalWriter : Common.IJournalWriter
	{
		public PDFJournalWriter()
		{
		}
		
		#region IJournalWriter Members
		public void WriteJournal(System.IO.Stream s, Journal j, int[] eventIDs, int[] commentIDs, bool header, bool footer)
		{
			pdfDocument pdf = new pdfDocument(j.Options[0].UserName + "'s Journal", j.Options[0].UserName);
			Layout l = new Layout(pdf);
			foreach (Journal.EventsRow er in j.Events)
			{
				if (eventIDs != null && !((IList) eventIDs).Contains(er.ID))
					continue;
				l.AddPage();
				l.Font = pdf.getFontReference(predefinedFont.csCourierBold);
				if (!er.IsSubjectNull())
					l.AddParagraph("Subject: " + er.Subject);
				if (!er.IsDateNull())
					l.AddParagraph("Date: " + er.Date.ToString());
				l.Font = pdf.getFontReference(predefinedFont.csCourier);
				l.LineBreak();
				l.AddParagraph(er.Body);
			}
			pdf.createPDF(s);
		}

		public string Filter
		{
			get { return "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*"; }
		}

		public object Settings
		{
			get { return null; }
			set
			{
//				if (value is XMLJournalWriterSettings)
//					settings = (XMLJournalWriterSettings) value;
//				else
//					throw new ArgumentException("Settings must be of type XMLJournalWriterSettings", "Settings");
			}
		}

		public string Description
		{
			get
			{
				return "Exports to PDF.";
			}
		}

		public string Author
		{
			get { return "Erik Frey"; }
		}

		public string Name
		{
			get { return "PDF Writer"; }
		}

		public string URL
		{
			get { return "http://ljarchive.sourceforge.net/"; }
		}

		public Version Version
		{
			get { return new Version(0, 9, 7, 0); }
		}
		#endregion
	}
}
