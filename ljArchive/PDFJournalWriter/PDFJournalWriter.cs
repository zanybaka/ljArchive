using System;
using System.Collections;
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
			JournalReport jr = new JournalReport(j);
			foreach (Journal.EventsRow er in j.Events)
			{
				jr.AddEvent(er);
			}
			jr.formatter.Create(jr, s);
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
