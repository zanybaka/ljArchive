using System;
 
// Creation date: 11.10.2002
// Checked: 06.03.2005
// Author: Otto Mayer (mot@root.ch)
// Version: 1.03

// Report.NET copyright © 2002-2006 root-software ag, Bürglen Switzerland - Otto Mayer, Stefan Spirig, all rights reserved
// This library is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation, version 2.1 of the License.
// This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. You
// should have received a copy of the GNU Lesser General Public License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA www.opensource.org/licenses/lgpl-license.html

namespace Root.Reports {
  /// <summary>Font Data</summary>
  /// <remarks>This is the base class of the font types that are supported by the Report.NET library.</remarks>
  internal abstract class FontData {
    /// <summary>Font style</summary>
    private readonly FontDef.Style style;

    /// <summary>Encoding to be applied to this font</summary>
    private readonly Encoding encoding;

    /// <summary><see langword="true"/> if the font is to be embedded in the document</summary>
    private readonly Boolean bEmbed;

    //------------------------------------------------------------------------------------------06.03.2005
    /// <summary></summary>
    /// <param name="style"></param>
    /// <param name="encoding">Encoding to be applied to this font</param>
    /// <param name="bEmbed"><see langword="true"/> if the font is to be embedded in the document</param>
    internal FontData(FontDef.Style style, Encoding encoding) {
      this.style = style;
      this.encoding = encoding;
      this.bEmbed = false;
    }

    //------------------------------------------------------------------------------------------06.03.2005
    /// <summary>Gets the height of the font in points (1/72 inch).</summary>
    /// <param name="fontProp">Font properties</param>
    /// <returns>Height of the font in points (1/72 inch)</returns>
    internal protected abstract Double rHeight(FontProp fontProp);

    //------------------------------------------------------------------------------------------06.03.2005
    /// <summary>Gets the width of the specified text in points (1/72 inch).</summary>
    /// <param name="fontProp">Font properties</param>
    /// <param name="sText">Text</param>
    /// <returns>Width of the text in points (1/72 inch)</returns>
    internal protected abstract Double rWidth(FontProp fontProp, String sText);

    //------------------------------------------------------------------------------------------06.03.2005
    #region
    //----------------------------------------------------------------------------------------------------
    /// <summary>Font encoding</summary>
    internal enum Encoding {
      /// <summary>CP 1250 encoding</summary>
      Cp1250,
      /// <summary>CP 1252 (WIN ANSI) encoding</summary>
      Cp1252,
      /// <summary>CP 1257 encoding</summary>
      Cp1257,
      /// <summary>MAC ROMAN encoding</summary>
      MacRoman
    }
    #endregion
  }
}
