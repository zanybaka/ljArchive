using System;
 
// Creation date: 11.10.2002
// Checked: 07.02.2005
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
  /// <summary>Font Properties Data Class</summary>
  /// <remarks>
  /// A font property data object can be referenced by many font property objects.
  /// Therefore no reference to the font property object can be stored in this object.
  /// </remarks>
  internal abstract class FontPropData {
    internal readonly FontData fontData;

    //------------------------------------------------------------------------------------------07.02.2005
    /// <summary>Creates a new font properties data object.</summary>
    internal FontPropData(FontProp fontProp) {
      FontDef.Style style;
      if (fontProp.bBold) {
        style = (fontProp.bItalic ? FontDef.Style.BoldItalic : FontDef.Style.Bold);
      }
      else {
        style = (fontProp.bItalic ? FontDef.Style.Italic : FontDef.Style.Standard);
      }
      fontData = fontProp.fontDef.aFontData[style];
    }
  }
}