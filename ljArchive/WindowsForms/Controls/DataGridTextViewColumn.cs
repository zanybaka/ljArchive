using System;
using System.Windows.Forms;
using System.Data;
using System.Text.RegularExpressions;

namespace EF.ljArchive.WindowsForms.Controls
{
	/// <summary>
	/// Summary description for DataGridTextViewColumn.
	/// </summary>
	public class DataGridTextViewColumn : DataGridTextBoxColumn
	{
		public DataGridTextViewColumn(string mappingName, string headerText, int width, bool htmlFormat)
		{
			this.MappingName = mappingName;
			this.HeaderText = headerText;
			this.Width = width;
			this.htmlFormat = htmlFormat;
		}

		public DataGridTextViewColumn(bool htmlFormat)
		{
			this.htmlFormat = htmlFormat;
		}

		public DataGridTextViewColumn() : this(false) {}

		protected override void Edit(CurrencyManager source, int rowNum, System.Drawing.Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			return;
		}

		protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle bounds, CurrencyManager source, int rowNum, System.Drawing.Brush backBrush, System.Drawing.Brush foreBrush, bool alignToRight)
		{
			DataRowView drv = source.List[rowNum] as DataRowView;
			string text;

			if (drv == null)
				text = string.Empty;
			else
                text = drv[this.MappingName].ToString();

			// DrawString fails with a generic GDI+ error on very long strings
			if (text.Length > maxScanChars)
				text = text.Substring(0, maxScanChars);

			if (htmlFormat)
			{
				string[] transformSections = r.Split(text);
				text = string.Join(" ", transformSections).Replace("\n", " ").Trim();
			}

			if (text.Length > maxPaintChars)
				text = text.Substring(0, maxPaintChars);

			g.FillRectangle(backBrush, bounds);
			bounds.Offset(0, 2);
			try
			{
				g.DrawString(text, this.DataGridTableStyle.DataGrid.Font, foreBrush, bounds, System.Drawing.StringFormat.GenericDefault);
			}
			catch (System.Runtime.InteropServices.ExternalException)
			{
				// one bad cell must not take down the grid
			}
		}

		public bool HTMLFormat
		{
			get
			{
				return htmlFormat;
			}
			set
			{
				htmlFormat = value;
			}
		}

		private bool htmlFormat;
		private Regex r = new Regex(@"<.*?>");
		private const int maxScanChars = 4096;
		private const int maxPaintChars = 512;
	}
}
