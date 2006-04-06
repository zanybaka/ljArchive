using System;
using System.Drawing;

// Creation date: 24.04.2002
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
  /// <summary>Defines the properties (i.e. format and style attributes) of a font.</summary>
  /// <remarks>
  /// Before a text object (e.g. <see cref="Root.Reports.RepString"/>) can be created,
  /// a <see cref="Root.Reports.FontDef"/> and <see cref="Root.Reports.FontProp"/> object must be defined.
  /// </remarks>
  /// <example>Font properties sample:
  /// <code>
  /// using Root.Reports;
  /// using System;
  /// using System.Drawing;
  /// 
  /// public class FontPropSample : Report {
  ///   public static void Main() {
  ///     RT.ViewPDF(new FontPropSample());
  ///   }
  ///
  ///   protected override void Create() {
  ///     FontDef fd = new FontDef(this, FontDef.StandardFont.Helvetica);
  ///     <b>FontProp fp = new FontProp(fd, 45, Color.Red)</b>;
  ///     <b>fp.bBold = true;</b>
  ///     new Page(this);
  ///     page_Cur.AddCB_MM(80, new RepString(<b>fp</b>, "FontProp Sample"));
  ///   }
  /// }
  /// </code>
  /// </example>
  public class FontProp {
    //------------------------------------------------------------------------------------------02.02.2005
    #region FontProp
    //----------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------02.02.2005
    /// <overloads>
    /// <summary>Creates a new font property object.</summary>
    /// <remarks>
    /// After a FontProp object has been created, the format and style attributes of the object can be changed.
    /// The size of the font can be specified in points (1/72 inch): height of the letter "H".
    /// </remarks>
    /// </overloads>
    /// 
    /// <summary>Creates a new font property object with the specified size and color.</summary>
    /// <param name="fontDef">Font definition</param>
    /// <param name="rSize">Size of the font in points (1/72 inch): height of the letter "H".</param>
    /// <param name="color">Color of the font</param>
    /// <remarks>
    /// After a FontProp object has been created, the format and style attributes of the object can be changed.
    /// The size of the font can be specified in points (1/72 inch): height of the letter "H".
    /// </remarks>
    public FontProp(FontDef fontDef, Double rSize, Color color) {
      this._fontDef = fontDef;
      this._rSize = rSize;
      this._color = color;
    }

    //------------------------------------------------------------------------------------------02.02.2005
    /// <summary>Creates a new font property object with the specified size.</summary>
    /// <param name="fontDef">Font definition</param>
    /// <param name="rSize">Size of the font in points (1/72 inch): height of the letter "H".</param>
    /// <remarks>
    /// The default color of the font is black.
    /// After a FontProp object has been created, the format and style attributes of the object can be changed.
    /// The size of the font can be specified in points (1/72 inch): height of the letter "H".
    /// </remarks>
    public FontProp(FontDef fontDef, Double rSize) : this(fontDef, rSize, Color.Black) {
    }
    #endregion

    //------------------------------------------------------------------------------------------03.02.2005
    #region Properties
    //----------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------02.02.2005
    /// <summary>Gets true if the font property object is registered.</summary>
    private Boolean bRegistered {
      get { return Object.ReferenceEquals(_fontProp_Registered, this); } 
    }

    //------------------------------------------------------------------------------------------02.02.2005
    private Boolean _bBold = false;
    /// <summary>Gets or sets the bold style of the font.</summary>
    /// <value><see langword="true"/> if this font is bold; otherwise false</value>
    /// <remarks>This property can be used to change the bold style of the font.</remarks>
    public Boolean bBold {
      get { return _bBold; } 
      set {
        ResetPreparedData();
        _bBold = value;
      }
    }

    //------------------------------------------------------------------------------------------02.02.2005
    private Boolean _bItalic = false;
    /// <summary>Gets or sets the italic style of the font.</summary>
    /// <value><see langword="true"/> if this font is italic; otherwise false</value>
    /// <remarks>This property can be used to change the italic style of the font.</remarks>
    public Boolean bItalic {
      get { return _bItalic; } 
      set {
        ResetPreparedData();
        _bItalic = value;
      }
    }

    //------------------------------------------------------------------------------------------02.02.2005
    private Boolean _bUnderline = false;
    /// <summary>Gets or sets the underline attribute of the font.</summary>
    /// <value><see langword="true"/> if this font is underlined; otherwise false</value>
    /// <remarks>This property can be used to change the underline attribute of the font.</remarks>
    public Boolean bUnderline {
      get { return _bUnderline; } 
      set {
        ResetPreparedData();
        _bUnderline = value;
      }
    }

    //------------------------------------------------------------------------------------------02.02.2005
    private Color _color = Color.Empty;
    /// <summary>Gets or sets the color of the font.</summary>
    /// <value>Color of this font</value>
    /// <remarks>This property can be used to set the color of the font.</remarks>
    public Color color {
      get { return _color; } 
      set {
        ResetPreparedData();
        _color = value;
      }
    }

    //------------------------------------------------------------------------------------------02.02.2005
    private FontDef _fontDef = null;
    /// <summary>Gets or sets the font definition object.</summary>
    /// <value>Font definition object of this font</value>
    /// <remarks>This property can be used to set the font definition object of this font property object.</remarks>
    public FontDef fontDef {
      get { return _fontDef; } 
      set {
        ResetPreparedData();
        _fontDef = value;
      }
    }

    //------------------------------------------------------------------------------------------03.02.2005
    private FontPropData _fontPropData = null;
    /// <summary>Gets the font property data object that contains data of the specific formatter.</summary>
    internal FontPropData fontPropData {
      get {
        if (_fontPropData == null) {
          if (bRegistered) {
            // FontPropData may be created only for registered FontProp objects
            _fontPropData = fontDef.report.formatter.fontPropData_CreateInstance(this);
          }
          else {
            return fontProp_Registered.fontPropData;
          }
        }
        else {
          System.Diagnostics.Debug.Assert(bRegistered);
        }
        return _fontPropData;
      }
    }

    //------------------------------------------------------------------------------------------03.02.2005
    private FontProp _fontProp_Registered = null;
    /// <summary>Gets a reference to the font property object with the same attributes that is registered.</summary>
    /// <remarks>
    /// A text object (e.g. RepString) must reference a registered font property object.
    /// Registered font property objects cannot be changed, they can only be accessed by the internal system.
    /// If null, it has not yet been used and therefore it is not registered.
    /// </remarks>
    internal FontProp fontProp_Registered {
      get {
        if (_fontProp_Registered == null) {
          String sKey = _fontDef.sFontName + ";" + _rSize.ToString("F3") + ";"
            + _color.R + "-" + _color.G + "-" + _color.B + ";" + _color.A + ";"
            + _bBold + ";" + _bUnderline + ";" + _bItalic + ";" + _rAngle.ToString("0.###");
          _fontProp_Registered = (FontProp)_fontDef.report.ht_FontProp[sKey];
          if (_fontProp_Registered == null) {
            _fontProp_Registered = new FontProp(_fontDef, rSize, _color);
            _fontProp_Registered._bBold = _bBold;
            _fontProp_Registered._bUnderline = _bUnderline;
            _fontProp_Registered._bItalic = _bItalic;
            _fontProp_Registered._rAngle = _rAngle;
            _fontProp_Registered._fontProp_Registered = _fontProp_Registered;
            _fontDef.report.ht_FontProp.Add(sKey, _fontProp_Registered);
          }
        }
        return _fontProp_Registered;
      }
    }

    //------------------------------------------------------------------------------------------03.02.2005
    private Double _rAngle = 0.0;
    /// <summary>Gets or sets the angle of the font.</summary>
    /// <value>Angle in degrees</value>
    /// <remarks>The text will be rotated clockwise and relative to the parent container.</remarks>
    public Double rAngle {
      get { return _rAngle; }
      set {
        ResetPreparedData();
        _rAngle = value;
      }
    }

    //------------------------------------------------------------------------------------------03.02.2005
    /// <summary>Gets the default height of the line feed in points (1/72 inch).</summary>
    /// <value>Default height of the line feed in points (1/72 inch)</value>
    /// <remarks>Use this property to get the default height for the line feed.</remarks>
    public Double rLineFeed {
      get { return _rSize * 2.0; } 
    }

    //------------------------------------------------------------------------------------------03.02.2005
    /// <summary>Gets the default height of the line feed in millimeters.</summary>
    /// <value>Default height of the line feed in millimeters</value>
    /// <remarks>Use this property to get the default height for the line feed.</remarks>
    public Double rLineFeedMM {
      get { return RT.rMMFromPoint(rLineFeed); } 
    }

    //------------------------------------------------------------------------------------------02.02.2005
    private Double _rSize;
    /// <summary>Gets or sets the size of the font in points (1/72 inch).</summary>
    /// <value>Size of the font in points (1/72 inch): height of the letter "H"</value>
    /// <remarks>This property can be used to set the size of the font.</remarks>
    public Double rSize {
      get { return _rSize; } 
      set {
        ResetPreparedData();
        _rSize = value;
      }
    }

    //------------------------------------------------------------------------------------------02.02.2005
    /// <summary>Gets or sets the size of the font in millimeters.</summary>
    /// <value>Size of the font in millimeters: height of the letter "H"</value>
    /// <remarks>This property can be used to set the size of the font.</remarks>
    public Double rSizeMM {
      get { return RT.rMMFromPoint(_rSize); }
      set { rSize = RT.rPointFromMM(value); }
    }
    #endregion

    //------------------------------------------------------------------------------------------03.02.2005
    #region Methods
    //----------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------03.02.2005
    /// <summary>Resets the prepared data structures.</summary>
    /// <remarks>This operation is required if any property of the font changes.</remarks>
    private void ResetPreparedData() {
      System.Diagnostics.Debug.Assert(!bRegistered);
      _fontProp_Registered = null;
    }

    //------------------------------------------------------------------------------------------03.02.2005
    /// <summary>Returns the height of the font in points (1/72 inch).</summary>
    /// <returns>Height of the font in points (1/72 inch)</returns>
    public Double rHeight() {
      return fontPropData.fontData.rHeight(fontProp_Registered);
    }

    //------------------------------------------------------------------------------------------03.02.2005
    /// <summary>Returns the height of the font in millimeters.</summary>
    /// <returns>Height of the font in millimeters</returns>
    public Double rHeightMM() {
      return RT.rMMFromPoint(rHeight());
    }

    //------------------------------------------------------------------------------------------03.02.2005
    /// <summary>Returns the width of the specified text in points (1/72 inch).</summary>
    /// <param name="sText">Text</param>
    /// <returns>Width of the text in points (1/72 inch)</returns>
    public Double rWidth(String sText) {
      return fontPropData.fontData.rWidth(fontProp_Registered, sText);
    }

    //------------------------------------------------------------------------------------------03.02.2005
    /// <summary>Returns the width of the specified text in millimeters.</summary>
    /// <param name="sText">Text</param>
    /// <returns>Width of the text in millimeters</returns>
    public Double rWidthMM(String sText) {
      return RT.rMMFromPoint(rWidth(sText));
    }

    //------------------------------------------------------------------------------------------06.02.2005
    #region
    /// <summary>Truncates the text to the specified width in points and adds three dots if necessary.</summary>
    /// <param name="sText">Text</param>
    /// <param name="rWidthMax">Width of the text in points (1/72 inch)</param>
    /// <remarks>This method truncates a string to the specified width.</remarks>
    /// <example>
    /// <code>
    /// FontDef fd = new FontDef(this, FontDef.StandardFont.Helvetica);
    /// FontProp fp = new FontProp(fd, 15);
    /// String s = <b>fp.sTruncateText("Hello World!", 100)</b>;
    /// 
    /// Result: "Hello W..."
    /// </code>
    /// </example>
    /// <returns>Truncated text</returns>
    #endregion
    public String sTruncateText(String sText, Double rWidthMax) {
      if (rWidthMax <= 0) {
        return "";
      }
      Double rAvgCharWidth = rWidth("njfo") / 4;
      Int32 iSubLength = (Int32)(rWidthMax / rAvgCharWidth);
      if (iSubLength > sText.Length) {
        iSubLength = sText.Length;
      }
      String sSubText = sText.Substring(0, iSubLength);
      Double rSubWidth = rWidth(sSubText);
      if (iSubLength == sText.Length && rSubWidth <= rWidthMax) {
        return sText;
      }

      rWidthMax -= rWidth("...");
      Int32 iMinLength = 0;
      Int32 iMaxLength = sText.Length;
      while (true) {
        if (rSubWidth <= rWidthMax) {
          iMinLength = iSubLength;
          iSubLength += (Int32)((rWidthMax - rSubWidth) / rAvgCharWidth) + 1;
          if (iSubLength > iMaxLength) {
            iSubLength = iMaxLength;
          }
        }
        else {
          iMaxLength = iSubLength - 1;
          iSubLength -= (Int32)((rSubWidth - rWidthMax) / rAvgCharWidth) + 1;
          if (iSubLength < iMinLength) {
            iSubLength = iMinLength;
          }
        }
        if (iMinLength == iMaxLength) {
          break;
        }
        sSubText = sText.Substring(0, iSubLength);
        rSubWidth = rWidth(sSubText);
      }
      return sText.Substring(0,iMaxLength) + "...";
    }

    //------------------------------------------------------------------------------------------06.02.2005
    #region
    /// <summary>Truncates the text to the specified width in millimeters and adds three dots if necessary.</summary>
    /// <param name="sText">Text</param>
    /// <param name="rWidthMaxMM">Width of the text in millimeters</param>
    /// <remarks>This method truncates a string to the specified width.</remarks>
    /// <example>
    /// <code>
    /// FontDef fd = new FontDef(this, FontDef.StandardFont.Helvetica);
    /// FontProp fp = new FontPropMM(fd, 5);
    /// String s = <b>fp.sTruncateTextMM("Hello World!", 32)</b>;
    /// 
    /// Result: "Hello W..."
    /// </code>
    /// </example>
    /// <returns>Truncated text</returns>
    #endregion
    public String sTruncateTextMM(String sText, Double rWidthMaxMM) {
      return sTruncateText(sText, RT.rPointFromMM(rWidthMaxMM));
    }
    #endregion
  }

  //------------------------------------------------------------------------------------------02.02.2005
  #region FontPropMM
  //----------------------------------------------------------------------------------------------------

  #region
  /// <summary>Defines the properties (i.e. format and style attributes) of a font with metric values.</summary>
  /// <remarks>
  /// Before a text object (e.g. <see cref="Root.Reports.RepString"/>) can be created,
  /// a <see cref="Root.Reports.FontDef"/> and <see cref="Root.Reports.FontProp"/> object must be defined.
  /// </remarks>
  /// <example>Font properties sample:
  /// <code>
  /// using Root.Reports;
  /// using System;
  /// using System.Drawing;
  /// 
  /// public class FontPropSample : Report {
  ///   public static void Main() {
  ///     RT.ViewPDF(new FontPropSample());
  ///   }
  ///
  ///   protected override void Create() {
  ///     FontDef fd = new FontDef(this, FontDef.StandardFont.Helvetica);
  ///     <b>FontProp fp = new FontPropMM(fd, 15, Color.Red)</b>;
  ///     <b>fp.bBold = true</b>;
  ///     new Page(this);
  ///     page_Cur.AddCB_MM(80, new RepString(<b>fp</b>, "FontProp Sample"));
  ///   }
  /// }
  /// </code>
  /// </example>
  #endregion
  public class FontPropMM : FontProp {
    //------------------------------------------------------------------------------------------02.02.2005
    #region
    /// <overloads>
    /// <summary>Creates a new font property object with metric values.</summary>
    /// <remarks>
    /// After a FontProp object has been created, the format and style attributes of the object can be changed.
    /// The size of the font can be specified in millimeters: height of the letter "H".
    /// </remarks>
    /// </overloads>
    /// 
    /// <summary>Creates a new font property object with the specified size and color.</summary>
    /// <param name="fontDef">Font definition</param>
    /// <param name="rSizeMM">Size of the font in millimeters: height of the letter "H".</param>
    /// <param name="color">Color of the font</param>
    /// <remarks>
    /// After a FontProp object has been created, the format and style attributes of the object can be changed.
    /// The size of the font can be specified in millimeters: height of the letter "H".
    /// </remarks>
    #endregion
    public FontPropMM(FontDef fontDef, Double rSizeMM, Color color) : base(fontDef, RT.rPointFromMM(rSizeMM), color) {
    }

    //------------------------------------------------------------------------------------------02.02.2005
    /// <summary>Creates a new font property object with the specified size.</summary>
    /// <param name="fontDef">Font definition</param>
    /// <param name="rSizeMM">Size of the font in millimeters: height of the letter "H".</param>
    /// <remarks>
    /// The default color of the font is black.
    /// After a FontProp object has been created, the format and style attributes of the object can be changed.
    /// The size of the font can be specified in millimeters: height of the letter "H".
    /// </remarks>
    public FontPropMM(FontDef fontDef, Double rSizeMM) : this(fontDef, rSizeMM, Color.Black) {
    }
  }
  #endregion
}
