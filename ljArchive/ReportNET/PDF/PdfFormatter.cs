using System;
#if Framework2
using System.Collections.Generic;
#else
using System.Collections;
#endif
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Drawing.Imaging;

// Creation date: 22.04.2002
// Checked: xx.05.2002
// Author: Otto Mayer (mot@root.ch)
// Version: 1.05

// Report.NET copyright � 2002-2006 root-software ag, B�rglen Switzerland - Otto Mayer, Stefan Spirig, all rights reserved
// This library is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation, version 2.1 of the License.
// This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. You
// should have received a copy of the GNU Lesser General Public License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA www.opensource.org/licenses/lgpl-license.html

namespace Root.Reports {
  /// <summary>PDF-Formatter</summary>
  /// <remarks>This class is used to make a PDF document from the specified report.</remarks>
  public class PdfFormatter : Formatter {
    //------------------------------------------------------------------------------------------xx.01.2006
    #region PdfFormatter
    //----------------------------------------------------------------------------------------------------

    /// <summary>List of all fonts that must be defined in the PDF document</summary>
    #if Framework2
    private Dictionary<String, FontProp> dict_FontProp;
    #else
    private Hashtable dict_FontProp;
    #endif

    /// <summary>position of the xref table</summary>
    internal Int32 iXRefPos;

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Initializes a new instance of the PDF formatter class.</summary>
    /// <example>
    /// <code>
    /// using Root.Report;
    /// using System;
    /// namespace ReportSample {
    /// class PdfPropertiesSample : Report {
    ///   public static void Main() {
    ///     PdfFormatter pf = new PdfFormatter();
    ///     pf.sTitle = "PDF Sample";
    ///     pf.sAuthor = "Otto Mayer, mot@root.ch";
    ///     pf.sSubject = "Sample of some PDF features";
    ///     pf.sKeywords = "Sample PDF Report.NET";
    ///     pf.sCreator = "Report.NET Sample Application";
    ///     pf.dt_CreationDate = new DateTime(2002, 8, 15, 0,0,0,0);
    ///     pf.pageLayout = PageLayout.TwoColumnLeft;
    ///     pf.bHideToolBar = true;
    ///     pf.bHideMenubar = false;
    ///     pf.bHideWindowUI = true;
    ///     pf.bFitWindow = true;
    ///     pf.bCenterWindow = true;
    ///     pf.bDisplayDocTitle = true;
    /// 
    ///     RT.ViewPDF(new PdfPropertiesSample(pf), "PdfPropertiesSample.pdf");
    ///   }
    ///
    ///   public PdfPropertiesSample(Formatter formatter) : base(formatter) {
    ///   }
    ///
    ///   protected override void Create() {
    ///     FontDef fd = new FontDef(this, FontDef.StandardFont.Helvetica);
    ///     FontProp fp = new FontPropMM(fd, 4);
    ///     FontProp fp_Title = new FontPropMM(fd, 11);
    ///     fp_Title.bBold = true;
    ///
    ///     Page page = new Page(this);
    ///     page.AddCenteredMM(40, new RepString(fp_Title, "PDF Properties Sample"));
    ///     fp_Title.rSizeMM = 8;
    ///     page.AddCenteredMM(100, new RepString(fp_Title, "First Page"));
    ///     page.AddCenteredMM(120, new RepString(fp, "Choose &lt;Document Properties, Summary&gt; from the"));
    ///     page.AddCenteredMM(126, new RepString(fp, "File menu to display the document properties"));
    ///
    ///     page = new Page(this);
    ///     page.AddCenteredMM(100, new RepString(fp_Title, "Second Page"));
    ///   }
    /// }
    /// }
    /// </code>
    /// </example>
    public PdfFormatter() {
      sb = new StringBuilder(10000);
      #if Framework2
      dict_FontProp = new Dictionary<String, FontProp>(50);
      #else
      dict_FontProp = new Hashtable(50);
      #endif

      pdfIndirectObject_Catalog = new PdfIndirectObject_Catalog(this);
      pdfIndirectObject_Info = new PdfIndirectObject_Info(this);
    }
    #endregion

    //------------------------------------------------------------------------------------------xx.01.2006
    #region PDF Document Properties
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------x

    /// <summary>Subject of the document</summary>
    public String sSubject;

    /// <summary>Keywords associated with the document</summary>
    public String sKeywords;

    /// <summary>Program that converted the document to PDF</summary>
    internal String sProducer = "Report.NET by root-software ag";

    /// <summary>Modification date and time of  the document</summary>
    internal DateTime dt_ModDate = DateTime.Now;

    //public Trapped bTrapped = Trapped.Unknown;

    /// <summary>Determines the page Mode in the PDF document</summary>
    public PageMode pageMode = PageMode.UseNone;

    /// <summary>Hide toolbar</summary>
    public Boolean bHideToolBar = false;

    /// <summary>Hide menu bar</summary>
    public Boolean bHideMenubar = false;

    /// <summary>Hide window UI</summary>
    public Boolean bHideWindowUI = false;

    /// <summary>Fit window</summary>
    public Boolean bFitWindow = false;

    /// <summary>Center window</summary>
    public Boolean bCenterWindow = false;

    /// <summary>Display document title</summary>
    public Boolean bDisplayDocTitle = false;

    /// <summary>Full screen page mode</summary>
    public NonFullScreenPageMode nonFullScreenPageMode = NonFullScreenPageMode.UseNone;

    /// <summary>Open action URI</summary>
    public String sOpenActionURI = null;

    /// <summary>Open action launch</summary>
    public String sOpenActionLaunch = null;

    /// <summary>File identifier</summary>
    public String sFileIdentifier = Guid.NewGuid().ToString();

    #endregion

    //------------------------------------------------------------------------------------------xx.01.2006
    #region Create RepObjX Objects
    //----------------------------------------------------------------------------------------------------

    internal override Object oCreate_Container() {
      return PdfContainerX.instance;
    }

    internal override Object oCreate_RepArcBase() {
      return PdfRepArcBaseX.instance;
    }

    //------------------------------------------------------------------------------------------03.02.2006
    internal override Object oCreate_RepImage() {
      return PdfRepImageX.instance;
    }

    internal override Object oCreate_RepLine() {
      return PdfRepLineX.instance;
    }

    internal override Object oCreate_RepRect() {
      return PdfRepRectX.instance;
    }

    internal override Object oCreate_RepString() {
      return PdfRepStringX.instance;
    }

    //------------------------------------------------------------------------------------------03.02.2006
    /// <summary>Creates the extended page data object.</summary>
    /// <param name="page">Page</param>
    /// <returns>Extended page data object</returns>
    internal override Object oCreate_PageX(Page page) {
      return new PdfIndirectObject_Page(this, page);
    }
    #endregion

    //------------------------------------------------------------------------------------------xx.01.2006
    #region old
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Builds the xref structure.</summary>
    private void BuildXObjectsX() {
//      foreach (ImageData imageData in report.ht_ImageData.Values) {
//        imageData.stream.Position = 0;
//        //Added By TechnoGuru - jjborie@yahoo.fr - http://www.borie.org/
//        using(Image image = Image.FromStream(imageData.stream)) {
//          // Tiff support
//          //Added By TechnoGuru - jjborie@yahoo.fr - http://www.borie.org/
//          if(image.RawFormat.Equals(ImageFormat.Tiff)) {
//          NewObjId();
//          Debug.Assert(iObjIdCur == (Int32)imageData.object_Data, "invalid object id of viewer preferences object");
//          WriteLine((Int32)imageData.object_Data + " 0 obj");
//          WriteLine("<<");
//          WriteLine("/Type /XObject");
//          WriteLine("/Subtype /Image");
//          WriteLine("/Name /I" + (Int32)imageData.object_Data);
//          WriteLine("/Width " + image.Width);
//          WriteLine("/Height " + image.Height);
//          //Int32 iImageStart = 0;
//          //Int32 iImageLength = 0;
//          //ImageFormat imageFormat = imageData.image.RawFormat;
//          //if (Object.Equals(imageFormat, ImageFormat.Jpeg)) {
//          //  iImageLength = (Int32)imageData.stream.Length;
//          //  WriteLine("/Filter /DCTDecode");
//          //}
//        else if (Object.Equals(imageFormat, ImageFormat.Tiff)) {
//          imageData.stream.Position = 0;
//          BinaryReader br = new BinaryReader(imageData.stream);
//          br.BaseStream.Position = 4;
//          Int32 iPointer = br.ReadInt32();
//          br.BaseStream.Position = iPointer;
//          Int16 nTagCount = br.ReadInt16();
//          Int32 iImage = 0;
//          Int32 iRows = 0;
//          Int32 iStripByteCounts = 0;
//          while (nTagCount > 0) {
//            Int16 nTagType = br.ReadInt16();
//            Int16 nDataType = br.ReadInt16();
//            Int32 iLength = br.ReadInt32();
//            Int32 iData = br.ReadInt32();
//            if (nTagType == 259) {  // Compression
//              if (iData == 1) {
//              }
//              else if (iData == 5) {
//                //WriteLine("/Filter /LZWDecode");
//                //WriteLine("/Filter /CCITTFaxDecode"); //changed
//              }
//              else {
//                Debug.Fail("unknown compression");
//              }
//            }
//            else if (nTagType == 262) {  // Photometric Interpretation
//              //Debug.Assert(iData == 2);  // RGB-Compression
//            }
//            else if (nTagType == 273) {  // Strip Offset
//              iImage = iData;
//              iRows = iLength;
//            }
//            else if (nTagType == 279) {  // Strip Byte Counts
//              iStripByteCounts = iData;
//            }
//            else if (nTagType == 317) {  // Predictor
//              Debug.Assert(iData == 2);  // Horizontal Predictor
//              //WriteLine("/DecodeParms [<< /Predictor 2 >>]");
//            }
//            nTagCount--;
//          }
//          iPointer = br.ReadInt32();
//
//          br.BaseStream.Position = iImage;
//          iImageStart = br.ReadInt32();
//          br.BaseStream.Position = iImage + (iRows - 1) * 4;
//          iImageLength = br.ReadInt32() - iImageStart;
//
//          br.BaseStream.Position = iStripByteCounts + (iRows - 1) * 4;
//          Int32 iStripLength = br.ReadInt32();
//          iImageLength += iStripLength;
//  
//          Console.WriteLine("");
//
//        }
//        else {
//          Debug.Fail("unknown image type: send image to mot@root.ch");
//        }
//        Int32 iBitsPerComponent = 0;
//        String sColorSpace = null;
//        if (imageData.image.PixelFormat == PixelFormat.Format8bppIndexed) {
//          iBitsPerComponent = 8;
//          sColorSpace = "DeviceGray";
//        }
//        else if (imageData.image.PixelFormat == PixelFormat.Format1bppIndexed) {
//          iBitsPerComponent = 1;
//          sColorSpace = "DeviceGray";
//        }
//        else if (imageData.image.PixelFormat == PixelFormat.Format24bppRgb) {
//          iBitsPerComponent = 8;
//          sColorSpace = "DeviceRGB";
//        }
//        else {
//          Debug.Fail("unknown image format: send image to mot@root.ch");
//        }
//        WriteLine("/BitsPerComponent " + iBitsPerComponent.ToString());
//        WriteLine("/ColorSpace /" + sColorSpace);
//        WriteLine("/Length " + iImageLength.ToString());
//        WriteLine(">>");
//        WriteLine("stream");
//        FlushBuffer();
//        
//        imageData.stream.Position = 0;
//        BinaryReader r = new BinaryReader(imageData.stream);
//        if (iImageStart > 0) {
//          r.BaseStream.Position = iImageStart;
//        }
//        Byte[] aByte = r.ReadBytes(iImageLength);
//        r.Close();
//
//        //stream.Flush();
//        bufferedStream.Write(aByte, 0, aByte.Length);
//        iBytesWrittenToStream += aByte.Length;
//        WriteLine("\nendstream");
//        WriteLine("endobj");
//      }
      foreach (ImageData imageData in report.ht_ImageData.Values) {
        imageData.stream.Position = 0;
        //Added By TechnoGuru - jjborie@yahoo.fr - http://www.borie.org/
#if !WindowsCE
        using(Image image = Image.FromStream(imageData.stream)) {
          // Tiff support
          //Added By TechnoGuru - jjborie@yahoo.fr - http://www.borie.org/
          if(image.RawFormat.Equals(ImageFormat.Tiff)) {
//            NewObjId();
            PdfIndirectObject_ImageJpeg pdfIndirectObject_ImageJpeg = (PdfIndirectObject_ImageJpeg)imageData.oImageResourceX;
            WriteLine(pdfIndirectObject_ImageJpeg.iObjectNumber + " 0 obj"); 
            WriteLine("<<"); 
            WriteLine("/Type /XObject"); 
            WriteLine("/Subtype /Image"); 
            WriteLine("/Name /I" + pdfIndirectObject_ImageJpeg.iObjectNumber); 
            WriteLine("/Width " + image.Width); 
            WriteLine("/Height " + image.Height);

            // Handle B&W format with CCIT
            if(image.PixelFormat.Equals(PixelFormat.Format1bppIndexed)) {
              imageData.stream.Position = 0; 
              BinaryReader r = new BinaryReader(imageData.stream);
              //CCIT 4
              //This String contain the startinf sequence of the tiff file for CCIT 4
              string patternTiffFile = "���������";
              string startTiffFile = "";
              int index = 0;
              int i = 0;
              for(index=0; index < 2048; index++) {
                startTiffFile += (char) (((i = r.ReadByte()) == 0?1:i));
              }
              int startPositionTiff = startTiffFile.IndexOf(patternTiffFile);
              //CCIT 3
              if(startPositionTiff == -1) {
                //This String contain the startinf sequence of the tiff file for CCIT 3
                patternTiffFile = "� � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � � �";
                startPositionTiff = startTiffFile.IndexOf(patternTiffFile);
                WriteLine("/Filter [/CCITTFaxDecode]");
                WriteLine("/DecodeParms [<< /Columns " + image.Width + "  /Rows " + image.Height + " /EndOfBlock false/EncodedByteAlign true >>]"); // This line was added to support TIFF 
              }
              else {
                WriteLine("/Filter /CCITTFaxDecode");
                WriteLine("/DecodeParms << /K -1 /Columns " + image.Width + " >>"); // This line was added to support TIFF 
              }
              if(startPositionTiff == -1)startPositionTiff = 512;
              WriteLine("/BitsPerComponent 1");
              WriteLine("/ColorSpace /DeviceGray");
              Int64 lLength = imageData.stream.Length; 
              WriteLine("/Length " + (lLength - startPositionTiff).ToString()); // CHANGED 
              WriteLine(">>"); 
              WriteLine("stream"); 
              FlushBuffer(); 

              imageData.stream.Position = 0;
              r.ReadBytes((Int32)startPositionTiff);
              Byte[] aByte = r.ReadBytes((Int32)lLength - startPositionTiff); // CHANGED 
              r.Close(); 

              //stream.Flush(); 
              bufferedStream.Write(aByte, 0, aByte.Length); 
              iBytesWrittenToStream += aByte.Length; 
              WriteLine("\nendstream"); 
              WriteLine("endobj"); 
            }
            else if(image.PixelFormat.Equals(PixelFormat.Format4bppIndexed)) {
              //Not supported I don't have tiff file to test it =)

              //            WriteLine("/Filter /CCITTFaxDecode");
              //            WriteLine("/DecodeParms << /K -1 /Columns " + image.Width + " >>"); // This line was added to support TIFF 
              //            WriteLine("/BitsPerComponent 4");
              //            WriteLine("/ColorSpace /DeviceGray");
            }
            else {
              //Tiff in gray or color are converted as jpeg in the tiff
              MemoryStream ms = new MemoryStream();
              image.Save(ms,ImageFormat.Jpeg);
              Byte[] aByte = ms.GetBuffer();

              WriteLine("/BitsPerComponent 8");
              WriteLine("/ColorSpace /DeviceRGB");
              WriteLine("/Filter /DCTDecode");
              WriteLine("/Length " + (aByte.Length).ToString()); // CHANGED 
              WriteLine(">>"); 
              WriteLine("stream"); 
              FlushBuffer(); 

              bufferedStream.Write(aByte, 0, aByte.Length);
              iBytesWrittenToStream += aByte.Length;

              WriteLine("\nendstream"); 
              WriteLine("endobj"); 
            }
          }
          imageData.stream.Close();
          imageData.stream = null;
        }
#endif
      }
    }
    #endregion

    //------------------------------------------------------------------------------------------xx.01.2006
    #region Objects
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------x
    #if Framework2
    internal List<PdfIndirectObject> list_PdfIndirectObject = new List<PdfIndirectObject>(50);
    #else
    internal ArrayList list_PdfIndirectObject = new ArrayList(50);
    #endif

    internal PdfIndirectObject_Catalog pdfIndirectObject_Catalog;
    internal PdfIndirectObject_Info pdfIndirectObject_Info;
    internal PdfIndirectObject_ViewerPreferences pdfIndirectObject_ViewerPreferences = null;
    internal PdfIndirectObject_Pages pdfIndirectObject_Pages;

    /// <summary>Prepares the PDF-object structure.</summary>
    private void PrepareObjIds() {
      if (bHideToolBar || bHideMenubar || bHideWindowUI || bFitWindow || bCenterWindow || bDisplayDocTitle
        || nonFullScreenPageMode != NonFullScreenPageMode.UseNone)
      {
        pdfIndirectObject_ViewerPreferences = new PdfIndirectObject_ViewerPreferences(this);
      }

      //     iObjEncoding = iObjId++;

      // search all fonts and prepare the pages
      StringBuilder sb = new StringBuilder(50);

      foreach (Page page in report.enum_Page) {
      }
      // pages
      pdfIndirectObject_Pages = new PdfIndirectObject_Pages(this);
      foreach (Page page in report.enum_Page) {
        PdfIndirectObject_Page pdfIndirectObject_Page = (PdfIndirectObject_Page)page.oRepObjX;
        PrepareObjIdsForContainer(pdfIndirectObject_Page, page);
        pdfIndirectObject_Page.pdfIndirectObject_PageContents = new PdfIndirectObject_PageContents(this, page);
      }
      foreach (ImageData imageData in report.ht_ImageData.Values) {
        imageData.oImageResourceX = new PdfIndirectObject_ImageJpeg(this, imageData);
      }
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Prepares the PDF-object structure for a container.</summary>
    /// <param name="pdfPageData"></param>
    /// <param name="iObjId"></param>
    /// <param name="container"></param>
    private void PrepareObjIdsForContainer(PdfIndirectObject_Page pdfPageData, Container container) {
      foreach (RepObj repObj in container) {
        if (repObj is Container) {
          PrepareObjIdsForContainer(pdfPageData, (Container)repObj);
        }
        else if (repObj is RepArcBase) {
          pdfPageData.bProcSet_PDF = true;
        }
        else if (repObj is RepImage) {
//          RepImage repImage = (RepImage)repObj;
//          ImageFormat imageFormat = repImage.imageData.image.RawFormat;
//          if (Object.Equals(imageFormat, ImageFormat.Jpeg)) {
//            pdfPageData.bProcSet_ImageC = true;
//          }
//          else if (Object.Equals(imageFormat, ImageFormat.Tiff)) {
//            pdfPageData.bProcSet_ImageB = true;
//          }
//          else {
//            Debug.Fail("unknown image type: send image to mot@root.ch");
//          }
          RepImage repImage = repObj as RepImage;
          repImage.imageData.stream.Position = 0;
          
          //Added By TechnoGuru - jjborie@yahoo.fr - http://www.borie.org/
          //To support tiff file
          //I reload Image from stream to be more scalable
          //(Dont't have like that lots of image object on memeory
    #if !WindowsCE
          using (Image image = Image.FromStream(repImage.imageData.stream)) {
            if(image.RawFormat.Equals(ImageFormat.Tiff) && image.PixelFormat.Equals(PixelFormat.Format1bppIndexed)) {
              pdfPageData.bProcSet_ImageI = true; // CHANGED 
            }
            else if(image.RawFormat.Equals(ImageFormat.Tiff) || image.RawFormat.Equals(ImageFormat.Jpeg)) {
              pdfPageData.bProcSet_ImageC = true;
            }
            else {
              Debug.Fail("unknown image type: send image to mot@root.ch");
            }
          }
#endif
        }
        else if (repObj is RepLine) {
          pdfPageData.bProcSet_PDF = true;
        }
        else if (repObj is RepRect) {
          pdfPageData.bProcSet_PDF = true;
        }
        else if (repObj is RepString) {
          FontProp fontProp_String = ((RepString)repObj).fontProp;
          PdfFontPropData pdfFontPropData = (PdfFontPropData)fontProp_String.fontPropData;
          if (pdfFontPropData.pdfIndirectObject_Font == null) {
            pdfFontPropData.pdfIndirectObject_Font = new PdfIndirectObject_Font(this, fontProp_String);
          }
          pdfPageData.RegisterFontProp(fontProp_String);
          pdfPageData.bProcSet_Text = true;
        }
        else {
          throw new ReportException("unknown report object type [" + repObj.GetType().FullName + "]"); 
        }
      }
    }
    #endregion

    //------------------------------------------------------------------------------------------xx.01.2006
    #region Output-Stream / Buffer
    //----------------------------------------------------------------------------------------------------

    /// <summary>number of bytes written to the PDF output stream</summary>
    internal Int32 iBytesWrittenToStream;

    /// <summary>PDF output stream for ASCII text</summary>
    #if !WindowsCE
    internal BufferedStream bufferedStream;
    #else
    private Stream bufferedStream;
    #endif

    /// <summary>output buffer</summary>
    internal StringBuilder sb;

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Copies the contents of the buffer to the PDF output stream.</summary>
    internal void FlushBuffer() {
      Byte[] aByte = System.Text.Encoding.Default.GetBytes(sb.ToString());
      bufferedStream.Write(aByte, 0, aByte.Length);
      iBytesWrittenToStream += sb.Length;
      sb.Length = 0;
      //sb.Remove(0, sb.Length);
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Writes a string directly to the output stream.</summary>
    /// <param name="s">String to write to the output stream</param>
    internal void WriteDirect(String s) {
      Byte[] aByte = System.Text.Encoding.Default.GetBytes(s);
      bufferedStream.Write(aByte, 0, aByte.Length);
      iBytesWrittenToStream += aByte.Length;
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Writes a line of text into the buffer.</summary>
    /// <param name="s">String to append to the buffer</param>
    private void WriteLine(String s) {
      sb.Append(s);
      sb.Append('\n');
    }
    #endregion

    //------------------------------------------------------------------------------------------xx.01.2006
    #region base
    //----------------------------------------------------------------------------------------------------

    //----------------------------------------------------------------------------------------------------x
    /// <summary></summary>
    /// <param name="report"></param>
    /// <param name="stream"></param>
    public override void Create(Report report, Stream stream) {
      this.report = report;
      this.stream = stream;

      foreach (FontProp fp in report.ht_FontProp.Values) {
        ((PdfFontPropData)fp.fontPropData).Init();  // ???
      }

      //this.stream = stream;
      iBytesWrittenToStream = 0;

      #if WindowsCE
      bufferedStream = stream;
      #else
      bufferedStream = new BufferedStream(stream);
      #endif
      // Wichtig: Es muss die Standardeinstellung f�r die Codierung verwendet werden !!!
      try {
        PrepareObjIds();

        sb.Length = 0;

        // Header
        sb.Append("%PDF-1.4\n");
        FlushBuffer();

        // Body
        foreach (PdfIndirectObject pdfIndirectObject in list_PdfIndirectObject) {
          pdfIndirectObject.Write();
        }

        FlushBuffer();
        iXRefPos = iBytesWrittenToStream;
        PdfFileElement_XRef pdfFileElement_XRef = new PdfFileElement_XRef(this);
        pdfFileElement_XRef.Write();

        PdfFileElement_Trailer pdfFileElement_Trailer = new PdfFileElement_Trailer(this);
        pdfFileElement_Trailer.Write();
        FlushBuffer();
      }
      finally {
        bufferedStream.Flush();
        //bufferedStream.Close();
      }
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Creates a font properties data object for this formatter.</summary>
    /// <param name="fontProp"></param>
    internal override FontPropData fontPropData_CreateInstance(FontProp fontProp) {
      StringBuilder sb = new StringBuilder(50);
      sb.Append(fontProp.fontDef.sFontName);
      if (fontProp.bBold) {
        sb.Append(";B");
      }
      if (fontProp.bItalic) {
        sb.Append(";I");
      }
      String sKey = sb.ToString();

      #if Framework2
      FontProp fontProp_Reg;
      dict_FontProp.TryGetValue(sKey, out fontProp_Reg);
      #else
      FontProp fontProp_Reg = (FontProp)dict_FontProp[sKey];
      #endif
      if (fontProp_Reg != null) {  // font registered
        //Debug.Assert(fontProp_Reg.fontPropData != null);
        return fontProp_Reg.fontPropData;
      }

//      Type1FontData afm = afm_Register(fontProp);
      PdfFontPropData pdfFontPropData = new PdfFontPropData(sKey, fontProp);
      dict_FontProp.Add(sKey, fontProp);
      return pdfFontPropData;
    }
    #endregion

    //------------------------------------------------------------------------------------------01.02.2006
    #region PageMode
    //----------------------------------------------------------------------------------------------------

    /// <summary>PDF Page Layout</summary>
    /// <remarks>The page-layout attribute will specify how the document should be displayed when it has been opened.</remarks>
    public enum PageMode {
      /// <summary>Neither document outline nor thumbnail images are visible</summary>
      UseNone,
      /// <summary>Document outlines are visible</summary>
      UseOutlines,
      /// <summary>Thumbnail images are visible</summary>
      UseThumbs,
      /// <summary>Full-screen mode, with no menu bar, window controls, or any other window visible</summary>
      FullScreen,
      /// <summary>Optional content group panel is visible</summary>
      UseOC,
      /// <summary>Attachments panel is visible</summary>
      UseAttachments
    }
    #endregion

    //------------------------------------------------------------------------------------------29.01.2006
    #region NonFullScreenPageMode
    //----------------------------------------------------------------------------------------------------

    /// <summary>PDF Full-Screen Page-Mode</summary>
    /// <remarks>
    /// The page-mode of the document will specify how to display the document on exiting full-screen mode.
    /// This settings will be ignored, if the value of the <see cref="PageMode"/> entry isn't <see cref="FullScreen"/>.
    /// </remarks>
    public enum NonFullScreenPageMode {
      /// <summary>Neither document outline nor thumbnail images visible (default)</summary>
      UseNone,
      /// <summary>Document outline visible</summary>
      UseOutlines,
      /// <summary>Thumbnail images visible</summary>
      UseThumbs,
      /// <summary>Optional content group panel visible</summary>
      UseOC
    }
    #endregion
  }
}
