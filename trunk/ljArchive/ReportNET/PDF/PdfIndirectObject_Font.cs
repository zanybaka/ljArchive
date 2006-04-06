using System;
using System.Collections;
using System.Drawing;
using System.IO;

// Creation date: 19.01.2006
// Checked: 01.02.2006
// Author: Otto Mayer (mot@root.ch)
// Version: 1.05

// Report.NET copyright © 2002-2006 root-software ag, Bürglen Switzerland - Otto Mayer, Stefan Spirig, all rights reserved
// This library is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation, version 2.1 of the License.
// This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. You
// should have received a copy of the GNU Lesser General Public License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA www.opensource.org/licenses/lgpl-license.html

namespace Root.Reports {
  //------------------------------------------------------------------------------------------01.02.2006
  #region PdfIndirectObject_Font
  //----------------------------------------------------------------------------------------------------

  /// <summary>PDF Indirect Object: Font</summary>
  internal sealed class PdfIndirectObject_Font : PdfIndirectObject {
    /// <summary>Font property</summary>
    private readonly FontProp fontProp;

    //------------------------------------------------------------------------------------------01.02.2006
    /// <summary>Creates a font indirect object.</summary>
    /// <param name="pdfFormatter">PDF formatter</param>
    /// <param name="fontProp">Font property</param>
    internal PdfIndirectObject_Font(PdfFormatter pdfFormatter, FontProp fontProp) : base(pdfFormatter) {
      this.fontProp = fontProp;
    }

    //------------------------------------------------------------------------------------------01.02.2006
    /// <summary>Writes the object to the buffer.</summary>
    internal override void Write() {
      PdfFontPropData pdfFontPropData = (PdfFontPropData)fontProp.fontPropData;
      Type1FontData type1FontData = (Type1FontData)pdfFontPropData.fontData;

      StartObj();
      Dictionary_Start();
      Dictionary_Key("Type");  Name("Font");
      Dictionary_Key("Subtype");  Name("Type1");
      Dictionary_Key("BaseFont");  Name(type1FontData.sFontName);
      if (type1FontData.sFamilyName != "ZapfDingbats" && type1FontData.sFamilyName != "Symbol") {
        Dictionary_Key("Encoding");  Name("WinAnsiEncoding");
      }
      Dictionary_End();
      EndObj();
    }
  }
  #endregion

  //------------------------------------------------------------------------------------------01.02.2006
  #region PdfIndirectObject_FontTtf
  //----------------------------------------------------------------------------------------------------

  /// <summary>PDF Indirect Object: Font (TTF)</summary>
  internal sealed class PdfIndirectObject_FontTtf : PdfIndirectObject {
    /// <summary>Font property</summary>
    private readonly FontProp fontProp;

    //------------------------------------------------------------------------------------------01.02.2006
    /// <summary>Creates a font indirect object.</summary>
    /// <param name="pdfFormatter">PDF formatter</param>
    /// <param name="fontProp">Font property</param>
    internal PdfIndirectObject_FontTtf(PdfFormatter pdfFormatter, FontProp fontProp)
      : base(pdfFormatter) {
      this.fontProp = fontProp;
    }

    //------------------------------------------------------------------------------------------01.02.2006
    /// <summary>Writes the object to the buffer.</summary>
    internal override void Write() {
      PdfFontPropData pdfFontPropData = (PdfFontPropData)fontProp.fontPropData;
      Type1FontData type1FontData = (Type1FontData)pdfFontPropData.fontData;

      StartObj();
      Dictionary_Start();
      Dictionary_Key("Type");  Name("Font");
      Dictionary_Key("Subtype");  Name("Type1");
      Dictionary_Key("BaseFont");
      Name(type1FontData.sFontName);
      if (type1FontData.sFamilyName != "ZapfDingbats" && type1FontData.sFamilyName != "Symbol") {
        Dictionary_Key("Encoding");
        Name("WinAnsiEncoding");
      }
      Dictionary_End();
      EndObj();
    }
  }
  #endregion
}
