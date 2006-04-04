using System;
using System.Collections;
using sharpPDF;
using sharpPDF.Collections;
using sharpPDF.Enumerators;
using sharpPDF.Elements;
using sharpPDF.Fonts;

namespace EF.ljArchive.PDF
{
	/// <summary>
	/// Description of Layout.
	/// </summary>
	public class Layout
	{
		public Layout(pdfDocument pdf) : this(pdf, pdf.getFontReference(predefinedFont.csCourier), pdfColor.Black,
		                                      12, 14, 595, 842, 12)
		{
		}
		
		public Layout(pdfDocument pdf, pdfAbstractFont font, pdfColor fontColor, int fontSize, int lineHeight, int pageWidth, int pageHeight, int pageMargin)
		{
			this.pdf = pdf;
			this.font = font;
			this.fontColor = fontColor;
			this.fontSize = fontSize;
			this.lineHeight = lineHeight;
			this.pageWidth = pageWidth;
			this.pageHeight = pageHeight;
			this.pageMargin = pageMargin;
		}
		
		public void AddParagraph(string paragraph)
		{
			string txt = paragraph;
			while (txt != null && txt.Length > 0)
			{
				int parWidth = pageWidth - pageMargin * 2;
				int parHeight = pageHeight - pageMargin * 2 - vcursor;
				int x = pageMargin;
				int y = pageHeight - vcursor;
				paragraphElement p = new paragraphElement(new paragraphLineList(textAdapter.formatParagraph(
					ref txt, fontSize, font, parWidth,
					Convert.ToInt32(Math.Floor(((double)parHeight/(double)lineHeight))), lineHeight,
					predefinedAlignment.csLeft)), parWidth, lineHeight, fontSize, font, x, y, fontColor);
				page.addElement(p);
				vcursor += p.height;
				if (txt != null && txt.Length > 0)
					AddPage();
			}			
		}
		
		public void AddPage()
		{
			page = pdf.addPage(pageHeight, pageWidth);
			vcursor = pageMargin;
		}
		
		public void LineBreak()
		{
			vcursor += lineHeight;
		}
		
		public pdfDocument PDF
		{
			get { return pdf; }
		}
		
		public pdfPage Page
		{
			get { return page; }
		}
		
		public pdfAbstractFont Font
		{
			get { return font; }
			set { font = value; }
		}
		
		public pdfColor FontColor
		{
			get { return fontColor; }
			set { fontColor = value; }
		}
		
		public int LineHeight
		{
			get { return lineHeight; }
			set { lineHeight = value; }
		}		
		
		public int FontSize
		{
			get { return fontSize; }
			set { fontSize = value; }
		}
		
		public int PageWidth {
			get { return pageWidth; }
			set { pageWidth = value; }
		}
		
		public int PageHeight {
			get { return pageHeight; }
			set { pageHeight = value; }
		}
		
		public int PageMargin
		{
			get { return pageMargin; }
			set { pageMargin = value; }
		}
		
		private pdfDocument pdf;
		private pdfPage page;
		private pdfAbstractFont font;
		private pdfColor fontColor;
		private int lineHeight;
		private int fontSize;
		private int pageWidth;
		private int pageHeight;
		private int pageMargin;
		private int vcursor;
	}
}
