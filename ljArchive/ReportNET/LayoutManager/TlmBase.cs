//#define Test

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;

// Creation date: 05.11.2002
// Checked: 30.05.2003
// Author: Otto Mayer (mot@root.ch)
// Version: 1.01

// Report.NET copyright 2002-2004 root-software ag, Bürglen Switzerland - O. Mayer, S. Spirig, R. Gartenmann, all rights reserved
// This library is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation, version 2.1 of the License.
// This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. You
// should have received a copy of the GNU Lesser General Public License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA www.opensource.org/licenses/lgpl-license.html

namespace Root.Reports {
  /// <summary>Table Layout Manager Base Class</summary>
  /// <remarks>
  /// <include file='D:\Programs\DotNet03\Root\Reports\Include.xml' path='documentation/class[@name="TlmBaseDefaults"]/*'/>
  /// </remarks>
  public abstract class TlmBase : LayoutManager, IDisposable {
    //----------------------------------------------------------------------------------------------------x
    #region TlmBase
    //----------------------------------------------------------------------------------------------------x

    /// <summary>Report of this table layout manager</summary>
    public readonly Report report;

    /// <summary>Definition of the default properties of a cell of this table</summary>
    public readonly TlmCellDef tlmCellDef_Default;

    /// <summary>Definition of the default properties of a column of this table</summary>
    public readonly TlmColumnDef tlmColumnDef_Default;

    /// <summary>Definition of the default properties of a row of this table</summary>
    public readonly TlmRowDef tlmRowDef_Default;

    /// <summary>Lines will be shortened by this value.</summary>
    /// <remarks>Lines in PDF are sometimes too long.</remarks>
    private const Double rLineDelta = 0.0;

    /// <summary>Tolerance for comparing coordinates.</summary>
    internal const Double rTol = 0.01;

    /// <summary>Header font properties</summary>
    public FontProp fontProp_Header;

    #if (Test)
    private const Double rTest = 3;
    private PenProp pp_Test;
    #else
    private const Double rTest = 0;
    #endif

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    /// <summary>Status of the layout manager</summary>
    private enum Status {
      /// <summary>Initialization mode</summary>
      Init,
      /// <summary>Table open</summary>
      Open,
      /// <summary>Container closed</summary>
      Closed
    }

    /// <summary>Status of the layout manager</summary>
    private Status status = Status.Init;

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Creates a new table layout manager.</summary>
    /// <param name="report">Report of this table layout manager</param>
    internal TlmBase(Report report) {
      this.report = report;
      
      tlmCellDef_Default = new TlmCellDef();
      tlmCellDef_Default.rAlignH = RepObj.rAlignLeft;
      tlmCellDef_Default.rAlignV = RepObj.rAlignTop;
      tlmCellDef_Default.rAngle = 0;
      tlmCellDef_Default.tlmTextMode = TlmTextMode.EllipsisCharacter;
      tlmCellDef_Default.rLineFeed = Double.NaN;

      tlmCellDef_Default.rMarginLeft = 0;
      tlmCellDef_Default.rMarginRight = 0;
      tlmCellDef_Default.rMarginTop = 0;
      tlmCellDef_Default.rMarginBottom = 0;

      tlmCellDef_Default.rIndentLeftMM = 1;
      tlmCellDef_Default.rIndentRightMM = 1;
      tlmCellDef_Default.rIndentTopMM = 1;
      tlmCellDef_Default.rIndentBottomMM = 1;

      tlmCellDef_Default.brushProp_Back = null;

      tlmCellDef_Default.penProp_LineLeft = null;
      tlmCellDef_Default.penProp_LineRight = null;
      tlmCellDef_Default.penProp_LineTop = null;
      tlmCellDef_Default.penProp_LineBottom = null;

      tlmCellDef_Default.iOrderLineLeft = 0;
      tlmCellDef_Default.iOrderLineRight = 0;
      tlmCellDef_Default.iOrderLineTop = 0;
      tlmCellDef_Default.iOrderLineBottom = 0;

      tlmColumnDef_Default = new TlmColumnDef();
      tlmRowDef_Default = new TlmRowDef();
      #if (Test)
      pp_Test = new PenPropMM(report, 0.1, Color.Orange);
      #endif
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Checks whether the layout manager status is 'Init'.</summary>
    /// <exception cref="ReportException">The layout manager status is not 'Init'</exception>
    internal void CheckStatus_Init(String sMsg) {
      if (status != Status.Init) {
        throw new ReportException("The layout manager must be in initialization mode; " + sMsg);
      }
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Checks whether the layout manager is opened.</summary>
    /// <exception cref="ReportException">The layout manager status is not 'ContainerOpen'</exception>
    internal void CheckStatus_Open(String sMsg) {
      if (status != Status.Open) {
        throw new ReportException("The layout manager must be opened; " + sMsg);
      }
    }
    #endregion

    //----------------------------------------------------------------------------------------------------x
    #region Initialization / Definition
    //----------------------------------------------------------------------------------------------------x

    /// <summary>Array of Column Definition Objects</summary>
    internal class AL_TlmColumn : ArrayList {
      /// <summary>Creates the array of column definition objects.</summary>
      internal AL_TlmColumn() : base(20) {
      }

      /// <summary>Gets the column definition with the specified index.</summary>
      internal new TlmColumn this[Int32 iIndex] {
        get { return (TlmColumn)base[iIndex]; }
      }
    }

    /// <summary>Column definition</summary>
    internal AL_TlmColumn al_TlmColumn = new AL_TlmColumn();

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Scales the current width of the columns to the specified width.</summary>
    /// <param name="rWidthNew">New width (points, 1/72 inch)</param>
    /// <exception cref="ReportException">The layout manager status is not 'Init'</exception>
    public void ScaleWidth(Double rWidthNew) {
      CheckStatus_Init("the width of the columns cannot be scaled.");

      Double rWidthCur = 0;
      foreach (TlmColumn col in al_TlmColumn) {
        rWidthCur += col.rWidth;
      }
      Double rScale = rWidthNew / rWidthCur;
      foreach (TlmColumn col in al_TlmColumn) {
        col.rWidth *=  rScale;
      }
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Scales the current width of the columns to the specified width (metric version).</summary>
    /// <param name="rWidthNewMM">New width (mm)</param>
    /// <exception cref="ReportException">The layout manager status is not 'Init'</exception>
    public void ScaleWidthMM(Double rWidthNewMM) {
      ScaleWidth(RT.rPointFromMM(rWidthNewMM));
    }
    #endregion

    //----------------------------------------------------------------------------------------------------x
    #region Table
    //----------------------------------------------------------------------------------------------------x

    private Double _rWidth = Double.NaN;
    /// <summary>Width of the table (points, 1/72 inch)</summary>
    public Double rWidth {
      get {
        if (Double.IsNaN(_rWidth)) {
          Double r = 0;
          foreach (TlmColumn col in al_TlmColumn) {
            r += col.rWidth;
          }
          return r;
        }
        return _rWidth;
      }
    }

    /// <summary>Width of the table (mm)</summary>
    public Double rWidthMM {
      get { return RT.rMMFromPoint(rWidth); }
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>The layout manager will be opened.</summary>
    public void Open() {
      if (status == Status.Open) {
        throw new ReportException("The layout manager has been opened already; " +
          "it must be in initialization mode or it must have been closed.");
      }

      if (status == Status.Init) {
        // Set position of the columns
        aCellCreateType_New = new TlmBase.CellCreateType[al_TlmColumn.Count];
        _rWidth = 0;
        foreach (TlmColumn col in al_TlmColumn) {
          col._rPosX = rWidth;
          _rWidth += col.rWidth;
          aCellCreateType_New[col.iIndex] = TlmBase.CellCreateType.New;
        }
      }

      Debug.Assert(aTlmRow.iCount == 0);
      Debug.Assert(tlmRow_Committed == null);
      foreach (TlmColumn col in al_TlmColumn) {
        Debug.Assert(col.iRepObjCommitted == 0);
      }

      status = Status.Open;

      CreateNewContainer();
      report.al_PendingTasks.Add(this);
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>The layout manager will be closed.</summary>
    public void Close() {
      if (status == Status.Init ||  status == Status.Closed) {
        return;
      }

      if (aTlmRow.iCount > 0) {
        WriteAll();
      }

      Debug.Assert(aTlmRow.iCount == 0);
      Debug.Assert(tlmRow_Committed == null);
      foreach (TlmColumn col in al_TlmColumn) {
        Debug.Assert(col.iRepObjCommitted == 0);
      }

      status = Status.Closed;
      report.al_PendingTasks.Remove(this);
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>The layout manager will be closed.</summary>
    public void Dispose() {
      Close();
    }
    #endregion

    //----------------------------------------------------------------------------------------------------x
    #region ArrayTlmRow
    //----------------------------------------------------------------------------------------------------x

    /// <summary>Array of TlmRows</summary>
    internal class ArrayTlmRow {
      internal ArrayList _al_TlmRow = new ArrayList(20);
      internal ArrayList al_TlmRow {
        get {
          #if (Checked)
          DebugTools.CheckMethodCall(new DebugTools.Method[] {
            new DebugTools.Method(typeof(TlmBase), "InsertRow"),
            new DebugTools.Method(typeof(TlmBase), "RemoveCommittedRowsAndRepObjs"),
            new DebugTools.Method(typeof(TlmRow), "get_iIndex"),
            new DebugTools.Method(typeof(TlmRow), "set_iIndex")
          });
          #endif
          return _al_TlmRow;
        }
      }

      internal ArrayTlmRow() {
      }

      internal TlmRow this[Int32 iIndex] {
        get { return (TlmRow)_al_TlmRow[iIndex]; }
      }

      internal Int32 iCount {
        get { return _al_TlmRow.Count; }
      }
    }

    /// <summary>Lines will be shortened by this value.</summary>
    /// <remarks>Lines in PDF are sometimes too long.</remarks>
    public Double rMarginTop = RT.rPointFromMM(1);

    internal void InsertRow(TlmRow tlmRow_Prev, TlmRow row_New) {
      Int32 iIndex = (tlmRow_Prev == null ? 0 : tlmRow_Prev.iIndex + 1);
      aTlmRow.al_TlmRow.Insert(iIndex, row_New);

      for (;  iIndex < aTlmRow.iCount;  iIndex++) {
        TlmRow row = aTlmRow[iIndex];
        row.iIndex = iIndex;
      }
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Removes all committed rows and report objects.</summary>
    internal void RemoveCommittedRowsAndRepObjs() {
      Debug.Assert(tlmRow_Committed != null);
      Boolean bFullRow = true;
      foreach (TlmCell cell in tlmRow_Committed.tlmCellEnumerator) {
        if (cell.iRepObjCount != cell.tlmColumn_Start.iRepObjCommitted) {
          bFullRow = false;
          break;
        }
      }

      if (bFullRow) {
        aTlmRow.al_TlmRow.RemoveRange(0, tlmRow_Committed.iIndex + 1);
      }
      else {
        aTlmRow.al_TlmRow.RemoveRange(0, tlmRow_Committed.iIndex);

        // remove committed report objects and get vertical position of the top-most report object
        Double rY = 0;
        foreach (TlmCell cell in tlmRow_Committed.tlmCellEnumerator) {
          if (cell.tlmColumn_Start.iRepObjCommitted > 0) {
            cell.RemoveRange(0, cell.tlmColumn_Start.iRepObjCommitted);
            cell.tlmColumn_Start.iRepObjCommitted = 0;
          }
          if (cell.iRepObjCount > 0) {
            RepObj repObj = cell.repObj_Get(0);
            Double rPosTop = repObj.rPosTop;
            if (rPosTop > rY) {
              rY = rPosTop;
            }
          }
          cell.tlmRow_Start = tlmRow_Committed;
        }
        rY -= rMarginTop;

        // adjust vertical position of all uncommitted report objects
        foreach (TlmCell cell in tlmRow_Committed.tlmCellEnumerator) {
          for (Int32 iRepObj = 0;  iRepObj < cell.iRepObjCount;  iRepObj++) {
            RepObj repObj = cell.repObj_Get(iRepObj);
            repObj.matrixD.rDY -= rY;
          }
          cell.rCurY -= rY;
          if (cell.rCurY < rMarginTop) {
            cell.rCurY = rMarginTop;
          }
        }
      }
      tlmRow_Committed = null;

      // Reset index of rows
      Int32 iIndex = 0;
      foreach (TlmRow row in aTlmRow.al_TlmRow) {
//        if (!Double.IsNaN(row.rPosBottom)) {
//          row.rPosBottom -= rDiffY;
//        }
        row.iIndex = iIndex++;
        row.rPosTop = 0;
        row.rPosBottom = Double.NaN;
      }

      if (aTlmRow.iCount > 0) {
        TlmRow row = aTlmRow[0];
        row.rPosTop = 0;
      }
    }
    #endregion

    //----------------------------------------------------------------------------------------------------x
    #region Rows and Report Objects
    //----------------------------------------------------------------------------------------------------x

    /// <summary>Array of all rows of the table</summary>
    internal ArrayTlmRow aTlmRow = new ArrayTlmRow();

    /// <summary>Default cell creation definition</summary>
    internal CellCreateType[] aCellCreateType_New;

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Gets the current row.</summary>
    public TlmRow tlmRow_Cur {
      get {
        if (aTlmRow.iCount == 0) {
          return null;
        }
        return aTlmRow[aTlmRow.iCount - 1];
      }
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>This method will create a new row (without commit).</summary>
    public TlmRow tlmRow_New() {
      return new TlmRow(this, tlmRow_Cur, aCellCreateType_New);
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>This method will commit all rows and after that it will create a new row.</summary>
    /// <remarks>The layout manager will be opened if required.</remarks>
    public void NewRow() {
      if (status != Status.Open) {
        Open();
      }
      TlmRow tlmRow = tlmRow_Cur;
      if (tlmRow != null && tlmRow.bAutoCommit) {
        Commit();
      }
      new TlmRow(this, tlmRow_Cur, aCellCreateType_New);
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Creates a new row after the specified row.</summary>
    /// <param name="tlmRow_Prev">The new row will be inserted after this row or at the beginning of list.</param>
    public TlmRow tlmRow_New(TlmRow tlmRow_Prev) {
      CheckStatus_Open("cannot create a new row");
      return new TlmRow(this, tlmRow_Prev, aCellCreateType_New);
    }

    //-----------------------------------------------------------------------------------------------------
    /// <summary>This method will create a new row.</summary>
    /// <param name="tlmRow_Prev">The new row will be inserted after this row or at the beginning of list.</param>
    /// <param name="aCellCreateType"></param>
    public TlmRow tlmRow_New(TlmRow tlmRow_Prev, CellCreateType[] aCellCreateType) {
      if (aCellCreateType == null) {
        aCellCreateType = aCellCreateType_New;
      }

      return new TlmRow(this, tlmRow_Prev, aCellCreateType);
    }

    //----------------------------------------------------------------------------------------------------x

    //----------------------------------------------------------------------------------------------------x
    /// <summary>This method will be called after a new row has been created.</summary>
    /// <param name="row">New row</param>
    internal protected virtual void OnNewRow(TlmRow row) {
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>This method will be called before the row will be closed.</summary>
    /// <param name="row">Row that will be closed</param>
    internal protected virtual void OnClosingRow(TlmRow row) {
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Gets the cell of the current row according to the column index.</summary>
    /// <param name="sMsg">Error message</param>
    /// <param name="iCol">Index of the column</param>
    /// <exception cref="ReportException">No row available, row is not open or the column index is out of range.</exception>
    private TlmCell tlmCell_FromColumnIndex(String sMsg, Int32 iCol) {
      CheckStatus_Open(sMsg);
      TlmRow tlmRow = tlmRow_Cur;
      if (tlmRow == null) {
        throw new ReportException("No row has been opened; " + sMsg);
      }
      if (tlmRow.status != TlmRow.Status.Open) {
        throw new ReportException("Row is not open; " + sMsg);
      }
      if (iCol < 0 || iCol >= al_TlmColumn.Count) {
        throw new ReportException("Column index out of range; " + sMsg);
      }
      return tlmRow.aTlmCell[iCol];
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Adds a report object to the specified cell of the current row.</summary>
    /// <param name="iCol">Index of the column</param>
    /// <param name="repObj">Report object that will be added</param>
    /// <exception cref="ReportException">No row available, row is not open or the column index is out of range.</exception>
    public void Add(Int32 iCol, RepObj repObj) {
      TlmCell tlmCell = tlmCell_FromColumnIndex("cannot add a report object.", iCol);
      tlmCell.Add(repObj);
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Makes a new line within the specified cell of the current row.</summary>
    /// <param name="iCol">Index of the column</param>
    /// <param name="rLineFeed">Height of the line feed (points, 1/72 inch)</param>
    /// <exception cref="ReportException">No row available, row is not open or the column index is out of range.</exception>
    public void NewLine(Int32 iCol, Double rLineFeed) {
      TlmCell tlmCell = tlmCell_FromColumnIndex("cannot make a new line.", iCol);
      tlmCell.NewLine(rLineFeed);
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Makes a new line within the specified cell of the current row (metric version).</summary>
    /// <param name="iCol">Index of the column</param>
    /// <param name="rLineFeedMM">Height of the line feed (mm)</param>
    /// <exception cref="ReportException">No row available, row is not open or the column index is out of range.</exception>
    public void NewLineMM(Int32 iCol, Double rLineFeedMM) {
      NewLine(iCol, RT.rPointFromMM(rLineFeedMM));
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Makes a new line within the specified cell of the current row.</summary>
    /// <param name="iCol">Index of the column</param>
    /// <exception cref="ReportException">No row available, row is not open or the column index is out of range.</exception>
    public void NewLine(Int32 iCol) {
      TlmCell tlmCell = tlmCell_FromColumnIndex("cannot make a new line.", iCol);
      tlmCell.NewLine();
    }
    #endregion

    //----------------------------------------------------------------------------------------------------x
    #region Write Objects to Report
    //----------------------------------------------------------------------------------------------------x

    /// <summary>Break Mode</summary>
    public enum BreakMode {
      /// <summary>Break on commited position</summary>
      Commit,
      /// <summary>Break on commited position or whole rows</summary>
      Row,
      /// <summary>Break on commited position or whole lines</summary>
      Line
    }

    /// <summary>Break mode</summary>
    public BreakMode breakMode = BreakMode.Commit;

    /// <summary>Break mode overflow handling</summary>
    /// <remarks>Commit: prints the whole committed block on a new page, overwriting the bottom margin if the committed block is lager than the container</remarks>
    /// <remarks>Row: prints whole rows on a new page, overwriting the bottom margin if a row is larger than the container</remarks>
    /// <remarks>Line: breaks after any line</remarks>
    public BreakMode breakMode_Overflow = BreakMode.Line;

    //----------------------------------------------------------------------------------------------------x
    private TlmRow _tlmRow_Committed;
    /// <summary>All rows up to this one have been committed</summary>
    internal TlmRow tlmRow_Committed {
      get { return _tlmRow_Committed; }
      set {
        #if (Checked)
          DebugTools.CheckMethodCall(new DebugTools.Method[] {
            new DebugTools.Method(typeof(TlmBase), "RemoveCommittedRowsAndRepObjs"),
            new DebugTools.Method(typeof(TlmBase), "bCommit")
          });
        #endif
        _tlmRow_Committed = value;
        if (_tlmRow_Committed == null) {
          foreach (TlmColumn col in al_TlmColumn) {
            col.iRepObjCommitted = 0;  // !!!
          }
        }
        else {
          foreach (TlmCell cell in _tlmRow_Committed.aTlmCell) {
            if (cell != null) {
              cell.tlmColumn_Start.iRepObjCommitted = cell.iRepObjCount;
            }
          }
        }
      }
    }

    private Double _rCurY = 0;
    /// <summary>Current vertical position (points, 1/72 inch)</summary>
    public Double rCurY {
      get { return _rCurY; }
    }

    /// <summary>Current vertical position (mm)</summary>
    public Double rCurY_MM {
      get { return RT.rMMFromPoint(rCurY); }
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Writes the committed horizontal lines.</summary>
    /// <param name="bOrderTop">Order</param>
    private void WriteCommittedHorLines(Boolean bOrderTop) {
      for (Int32 iRow = 0;  iRow <= tlmRow_Committed.iIndex;  iRow++) {
        TlmRow row = aTlmRow[iRow];
        // top line
        for (Int32 iCol = 0;  iCol < al_TlmColumn.Count;  iCol++) {
          TlmCell tlmCell = row.aTlmCell[iCol];
          if (tlmCell == null || tlmCell.bVisibleLineTop(iCol) != bOrderTop) {
            continue;
          }
          if (tlmCell.tlmRow_Start.iIndex != iRow || tlmCell.penProp_LineTop == null) {
            iCol = tlmCell.tlmColumn_End.iIndex;
            continue;
          }
          Int32 iColEnd = iCol;
          OptimizeTopLine(bOrderTop, tlmCell, ref iColEnd);
          Double rPosStart = (iCol == tlmCell.tlmColumn_Start.iIndex ? tlmCell.rPosMarginLeft : al_TlmColumn[iCol].rPosX);
          TlmColumn col_End = al_TlmColumn[iColEnd];
          TlmCell cell_End = row.aTlmCell[iColEnd];
          Double rPosEnd = (iCol == col_End.iIndex ? cell_End.rPosMarginRight : col_End.rPosX + col_End.rWidth);
          RepLine repLine = new RepLine(tlmCell.penProp_LineTop, rPosEnd - rPosStart, rTest);
          container_Cur.AddLT(rPosStart, tlmCell.rPosMarginTop, repLine);
          iCol = iColEnd;
        }

        // bottom line
        for (Int32 iCol = 0;  iCol < al_TlmColumn.Count;  iCol++) {
          TlmCell tlmCell = row.aTlmCell[iCol];
          if (tlmCell == null || tlmCell.bVisibleLineBottom(iCol) != bOrderTop) {
            continue;
          }
          if (tlmCell.tlmRow_End.iIndex != iRow || tlmCell.penProp_LineBottom == null) {
            iCol = tlmCell.tlmColumn_End.iIndex;
            continue;
          }
          Int32 iColEnd = iCol;
          OptimizeBottomLine(bOrderTop, tlmCell, ref iColEnd);
          Double rPosStart = (iCol == tlmCell.tlmColumn_Start.iIndex ? tlmCell.rPosMarginLeft : al_TlmColumn[iCol].rPosX);
          TlmColumn col_End = al_TlmColumn[iColEnd];
          TlmCell cell_End = row.aTlmCell[iColEnd];
          Double rPosEnd = (iCol == col_End.iIndex ? cell_End.rPosMarginRight : col_End.rPosX + col_End.rWidth);
          RepLine repLine = new RepLine(tlmCell.penProp_LineBottom, rPosEnd - rPosStart, -rTest);
          container_Cur.AddLT(rPosStart, tlmCell.rPosMarginBottom, repLine);
          iCol = iColEnd;
        }
      }
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>This method can be used to optimize the top line.</summary>
    /// <param name="bOrderTop">Order</param>
    /// <param name="cell">Start cell</param>
    /// <param name="iColEnd">End column</param>
    private void OptimizeTopLine(Boolean bOrderTop, TlmCell cell, ref Int32 iColEnd) {
    }
    
    //----------------------------------------------------------------------------------------------------x
    /// <summary>This method can be used to optimize the bottom line.</summary>
    /// <param name="bOrderTop">Order</param>
    /// <param name="cell">Start cell</param>
    /// <param name="iColEnd">End column</param>
    private void OptimizeBottomLine(Boolean bOrderTop, TlmCell cell, ref Int32 iColEnd) {
    }
    
    //----------------------------------------------------------------------------------------------------x
    /// <summary>Writes the committed vertical lines.</summary>
    /// <param name="bOrderTop">Order</param>
    private void WriteCommittedVertLines(Boolean bOrderTop) {
      for (Int32 iCol = 0;  iCol < al_TlmColumn.Count;  iCol++) {
        TlmColumn col = al_TlmColumn[iCol];
        // left line
        for (Int32 iRow = 0;  iRow <= tlmRow_Committed.iIndex;  iRow++) {
          TlmRow row = aTlmRow[iRow];
          TlmCell cell = row.aTlmCell[iCol];
          if (cell == null || cell.bVisibleLineLeft(iRow) != bOrderTop) {
            continue;
          }
          if (cell.tlmColumn_Start.iIndex != iCol || cell.penProp_LineLeft == null) {
            iRow = cell.tlmRow_End.iIndex;
            continue;
          }
          Int32 iRowEnd = iRow;
          OptimizeLeftLine(bOrderTop, cell, ref iRowEnd);
          Double rPosStart = (iRow == cell.tlmRow_Start.iIndex ? cell.rPosMarginTop : row.rPosTop);
          TlmRow row_End = aTlmRow[iRowEnd];
          TlmCell cell_End = row_End.aTlmCell[iCol];
          Double rPosEnd = (iRow == row_End.iIndex ? cell_End.rPosMarginBottom : row_End.rPosBottom);
          RepLine repLine = new RepLine(cell.penProp_LineLeft, rTest, rPosEnd - rPosStart);
          container_Cur.AddLT(cell.rPosMarginLeft, rPosStart, repLine);
          iRow = iRowEnd;
        }

        // right line
        for (Int32 iRow = 0;  iRow <= tlmRow_Committed.iIndex;  iRow++) {
          TlmRow row = aTlmRow[iRow];
          TlmCell cell = row.aTlmCell[iCol];
          if (cell == null || cell.bVisibleLineRight(iRow) != bOrderTop) {
            continue;
          }
          if (cell.tlmColumn_End.iIndex != iCol || cell.penProp_LineRight == null) {
            iRow = cell.tlmRow_End.iIndex;
            continue;
          }
          Int32 iRowEnd = iRow;
          OptimizeRightLine(bOrderTop, cell, ref iRowEnd);
          Double rPosStart = (iRow == cell.tlmRow_Start.iIndex ? cell.rPosMarginTop : row.rPosTop);
          TlmRow row_End = aTlmRow[iRowEnd];
          TlmCell cell_End = row_End.aTlmCell[iCol];
          Double rPosEnd = (iRow == row_End.iIndex ? cell_End.rPosMarginBottom : row_End.rPosBottom);
          RepLine repLine = new RepLine(cell.penProp_LineRight, -rTest, rPosEnd - rPosStart);
          container_Cur.AddLT(cell.rPosMarginRight, rPosStart, repLine);
          iRow = iRowEnd;
        }
      }
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>This method can be used to optimize the left line.</summary>
    /// <param name="bOrderTop">Order</param>
    /// <param name="cell">Start cell</param>
    /// <param name="iRowEnd">End row</param>
    private void OptimizeLeftLine(Boolean bOrderTop, TlmCell cell, ref Int32 iRowEnd) {
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>This method can be used to optimize the right line.</summary>
    /// <param name="bOrderTop">Order</param>
    /// <param name="cell">Start cell</param>
    /// <param name="iRowEnd">End row</param>
    private void OptimizeRightLine(Boolean bOrderTop, TlmCell cell, ref Int32 iRowEnd) {
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>This method will be called before the report objects will be written to the container.</summary>
    internal protected virtual void OnBeforeWrite() {
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Writes all committed objects to the report.</summary>
    /// <param name="bLastContainer">Last container</param>
    private void WriteCommittedReportObjects(Boolean bLastContainer) {
      if (tlmRow_Committed == null) {
        return;
      }

      OnBeforeWrite();

      Double rRowCommitted_PosBottom_Contents = tlmRow_Committed.rCalculateBottomPos(true);
      Double rRowCommitted_PosBottom_Graphics = rRowCommitted_PosBottom_Contents;
      if (tlmHeightMode == TlmHeightMode.Static || (tlmHeightMode == TlmHeightMode.AdjustLast && !bLastContainer)) {
        rRowCommitted_PosBottom_Graphics = container_Cur.rHeight;
      }

      // background
      tlmRow_Committed.rPosBottom = rRowCommitted_PosBottom_Graphics;

      Byte[,] aaiDone = new Byte[tlmRow_Committed.iIndex + 1, al_TlmColumn.Count];  // 0:init;  1:temp;  2:done
      for (Int32 iRow = 0;  iRow <= tlmRow_Committed.iIndex;  iRow++) {
        TlmRow row = aTlmRow[iRow];
        for (Int32 iCol = 0;  iCol < al_TlmColumn.Count;  iCol++) {
          if (aaiDone[iRow, iCol] == 2) {  // background of this cell has been created before
            continue;
          }
          TlmCell tlmCell = row.aTlmCell[iCol];
          if (tlmCell == null || tlmCell.brushProp_Back == null) {
            continue;
          }
          Int32 iColEnd = iCol;
          Int32 iRowEnd = iRow;
          OptimizeBackground(aaiDone, ref iRowEnd, ref iColEnd);
          Double rPosX1 = (iCol == tlmCell.tlmColumn_Start.iIndex ? tlmCell.rPosMarginLeft : al_TlmColumn[iCol].rPosX);
          Double rPosY1 = (iRow == tlmCell.tlmRow_Start.iIndex ? tlmCell.rPosMarginTop : row.rPosTop);
          TlmRow row_End = aTlmRow[iRowEnd];
          TlmColumn col_End = al_TlmColumn[iColEnd];
          TlmCell cell_End = row_End.aTlmCell[iColEnd];
          Double rPosX2 = (iColEnd == col_End.iIndex ? cell_End.rPosMarginRight : col_End.rPosX + col_End.rWidth);
          Double rPosY2 = (iRowEnd == row_End.iIndex ? cell_End.rPosMarginBottom : row_End.rPosBottom);
          #if (Test)
            container_Cur.AddLT(rPosX1, rPosY1, new RepLine(pp_Test, rPosX2 - rPosX1, rPosY2 - rPosY1));
          #else
          RepRect repRect = new RepRect(tlmCell.brushProp_Back, rPosX2 - rPosX1, rPosY2 - rPosY1);
            container_Cur.AddLT(rPosX1, rPosY1, repRect);
          #endif
        }
      }

      // contents
      tlmRow_Committed.rPosBottom = rRowCommitted_PosBottom_Contents;  // vertically aligned contents must have this bottom position

      for (Int32 iRow = 0;  iRow <= tlmRow_Committed.iIndex;  iRow++) {
        TlmRow row = aTlmRow[iRow];
        for (Int32 iCol = 0;  iCol < al_TlmColumn.Count;  iCol++) {
          TlmCell tlmCell = row.aTlmCell[iCol];
          if (tlmCell == null || iRow != tlmCell.tlmRow_Start.iIndex) {
            continue;
          }
          Boolean bLastRow = (Object.ReferenceEquals(tlmCell.tlmRow_End, tlmRow_Committed));
          Double rMaxY = tlmCell.rCalculateMaxY(bLastRow);
          Double rOfsY = 0;
          if (!RT.bEquals(tlmCell.rAngle, -90, 0.001)) {
            Debug.Assert(!Double.IsNaN(tlmCell.tlmRow_End.rPosBottom));
            rOfsY = (tlmCell.tlmRow_End.rPosBottom - row.rPosTop - rMaxY) * tlmCell.rAlignV;
          }
          Int32 iRepObjCount = (bLastRow ? tlmCell.tlmColumn_Start.iRepObjCommitted : tlmCell.iRepObjCount);
          for (Int32 iRepObj = 0;  iRepObj < iRepObjCount;  iRepObj++) {
            RepObj repObj = tlmCell.repObj_Get(iRepObj);
            repObj.matrixD.rDX += tlmCell.rPosMarginLeft;
            repObj.matrixD.rDY += tlmCell.rPosMarginTop + rOfsY;
            container_Cur.Add(repObj);
          }

          #if (Test)
          Double rX1 = cell.rPosMarginLeft + cell.rIndentLeft;
          Double rY1 = cell.rPosMarginTop + cell.rIndentTop;
          Double rX2 = cell.rPosMarginRight - cell.rIndentRight;
          Double rY2 = cell.rPosMarginBottom - cell.rIndentBottom;
          container_Cur.AddLT(rX1, rY1, new RepRect(pp_Test, rX2 - rX1, rY2 - rY1));
          #endif
          iCol = tlmCell.tlmColumn_End.iIndex;
        }
      }

      // horizontal lines
      tlmRow_Committed.rPosBottom = rRowCommitted_PosBottom_Graphics;

      WriteCommittedHorLines(false);
      WriteCommittedHorLines(true);

      WriteCommittedVertLines(false);
      WriteCommittedVertLines(true);

      // Remove committed report objects
      RemoveCommittedRowsAndRepObjs();

      _container_Cur = null;
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>This method will optimize the background rectangles.</summary>
    /// <param name="aaiDone">Status of the cells</param>
    /// <param name="iRowEnd">End row</param>
    /// <param name="iColEnd">End column</param>
    internal protected virtual void OptimizeBackground(Byte[,] aaiDone, ref Int32 iRowEnd, ref Int32 iColEnd) {
      TlmRow row = aTlmRow[iRowEnd];
      TlmCell cell = row.aTlmCell[iColEnd];
      for (Int32 iR = iRowEnd;  iR <= cell.tlmRow_End.iIndex;  iR++) {
        for (Int32 iC = iColEnd;  iC <= cell.tlmColumn_End.iIndex;  iC++) {
          aaiDone[iR, iC] = 2;
        }
      }
      iRowEnd = cell.tlmRow_End.iIndex;
      iColEnd = cell.tlmColumn_End.iIndex;

    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Commits as many objects as there can be placed into the current container.</summary>
    /// <remarks>All rows will be closed except the last.</remarks>
    /// <returns>True if a new page is required</returns>
    private Boolean bCommit() {
      // Get the index and top position of the first uncommitted row that is not closed
      Int32 iRow = 0;
      Double rY = 0;
      if (tlmRow_Committed != null) {
        iRow = tlmRow_Committed.iIndex;
        if (tlmRow_Committed.status == TlmRow.Status.Closed) {
          iRow++;
        }
        if (iRow > 0) {
          rY = aTlmRow[iRow - 1].rPosBottom;
        }
      }
      if (iRow >= aTlmRow.iCount) {
        return false;
      }

      // Set vertical position of committed rows and close all rows except the last one
      for (;  ;  iRow++) {
        TlmRow row = aTlmRow[iRow];
        row.rPosTop = rY;
        rY = row.rCalculateBottomPos(false);
        if (!Double.IsNaN(row.rPreferredHeight)) {  // set preferred row height
          Double rRowHeight = rY - row.rPosTop;
          if (row.rPreferredHeight > rRowHeight) {
            rY = row.rPosTop + row.rPreferredHeight;
          }
        }
        row.rPosBottom = rY;

        if (rY > container_Cur.rHeight) {  // new container required
          if (breakMode == BreakMode.Row) {
          }
          if (tlmRow_Committed == null) {  // new container required
            tlmRow_Committed = row;
            Boolean bRetVal = false;
            foreach (TlmColumn col in al_TlmColumn) {
              TlmCell cell = tlmRow_Committed.aTlmCell[col];
              col.iRepObjCommitted = 0;
              for (Int32 iRepObj = 0;  iRepObj < cell.iRepObjCount;  iRepObj++) {
                RepObj repObj = cell.repObj_Get(iRepObj);
                if (tlmRow_Committed.rPosTop + repObj.rY + repObj.rHeight > container_Cur.rHeight) {  // new container required
                  bRetVal = true;
                  break;
                }
                col.iRepObjCommitted = iRepObj + 1;
              }
            }
            return bRetVal;
          }
          return true;
        }

        if (iRow >= aTlmRow.iCount - 1) {  // last row
          _rCurY = rY;
          tlmRow_Committed = aTlmRow[iRow];
          return false;
        }
        row.Close();
      }
    }

    //------------------------------------------------------------------------------------------27.12.2003
    /// <summary>Commits the current contents.</summary>
    /// <remarks>All rows will be closed except the last.</remarks>
    public void Commit() {
      while (bCommit()) {
        WriteCommittedReportObjects(false);
        CreateNewContainer();
      }
    }

    //------------------------------------------------------------------------------------------27.12.2003
    /// <summary>Writes all objects to the report.</summary>
    private void WriteAll() {
      Commit();
      WriteCommittedReportObjects(true);
    }
    #endregion

    //----------------------------------------------------------------------------------------------------x
    #region Container
    //----------------------------------------------------------------------------------------------------x

    /// <summary>Default height of the table (points, 1/72 inch)</summary>
    public Double rContainerHeight = 72 * 1000;

    /// <summary>Default height of the table (mm)</summary>
    public Double rContainerHeightMM {
      get { return RT.rMMFromPoint(rContainerHeight); }
      set { rContainerHeight = RT.rPointFromMM(value); }
    }

    private Container _container_Cur;
    /// <summary>Current container</summary>
    public Container container_Cur {
      get { return _container_Cur; }
    }

    //------------------------------------------------------------------------------------------06.01.2004
    /// <summary>Table height mode</summary>
    public TlmHeightMode tlmHeightMode = TlmHeightMode.Adjust;

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Provides data for the NewContainer event</summary>
    public class NewContainerEventArgs : EventArgs {
      /// <summary>Table layout manager</summary>
      public readonly TlmBase tlm;

      /// <summary>New container</summary>
      public readonly Container container;

      /// <summary>Creates a data object for the NewContainer event.</summary>
      /// <param name="tlmBase">Table layout manager</param>
      /// <param name="container">New container: this container must be added to a page or a container.</param>
      internal NewContainerEventArgs(TlmBase tlmBase, Container container) {
        this.tlm = tlmBase;
        this.container = container;
      }
    }

    /// <summary>Represents the method that will handle the NewContainer event.</summary>
    public delegate void NewContainerEventHandler(Object oSender, NewContainerEventArgs ea);

    /// <summary>Occurs when a new container must be created.</summary>
    public event NewContainerEventHandler eNewContainer;

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Raises the NewContainer event.</summary>
    /// <param name="ea">Event argument</param>
    internal protected virtual void OnNewContainer(NewContainerEventArgs ea) {
      if (eNewContainer != null) {
        eNewContainer(this, ea);
      }
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Creates a new container.</summary>
    private void CreateNewContainer() {
      if (_container_Cur == null) {
        _container_Cur = new StaticContainer(rWidth, rContainerHeight);
        NewContainerEventArgs ea = new NewContainerEventArgs(this, _container_Cur);
        OnNewContainer(ea);
        //if (_container_Cur.container == null) {
        //  throw new ReportException("The current container has not been added to the page.");
        //}
      }
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>This method will create a new container that will be added to the parent container at the specified position.</summary>
    /// <param name="container_Parent">Parent container</param>
    /// <param name="rX">X-coordinate of the new container (points, 1/72 inch)</param>
    /// <param name="rY">Y-coordinate of the new container (points, 1/72 inch)</param>
    /// <exception cref="ReportException">The layout manager status is not 'Init'</exception>
    public Container container_Create(Container container_Parent, Double rX, Double rY) {
      if (status != Status.Init && status != Status.Closed) {
        throw new ReportException("The layout manager must be in initialization mode or it must be closed; cannot create a new container.");
      }
      if (_container_Cur != null) {
        throw new ReportException("The container has been defined already.");
      }
      CreateNewContainer();
      if (container_Parent != null) {
        container_Parent.Add(rX, rY, _container_Cur);
      }
      return _container_Cur;
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>This method will creates a new container that will be added to the parent container at the specified position (metric version).</summary>
    /// <param name="container_Parent">Parent container</param>
    /// <param name="rX_MM">X coordinate of the new container (mm)</param>
    /// <param name="rY_MM">Y coordinate of the new container (mm)</param>
    public Container container_CreateMM(Container container_Parent, Double rX_MM, Double rY_MM) {
      return container_Create(container_Parent, RT.rPointFromMM(rX_MM), RT.rPointFromMM(rY_MM));
    }
    #endregion

    //------------------------------------------------------------------------------------------06.01.2004
    #region CellCreateType
    //----------------------------------------------------------------------------------------------------x

    /// <summary>Cell creation type</summary>
    public enum CellCreateType {
      /// <summary>A new cell will be created for this row</summary>
      New,
      /// <summary>The cell will be merged with the previous cell of the column</summary>
      MergedV,
      /// <summary>The cell will be merged with the left cell of the row</summary>
      MergedH,
      /// <summary>The column will have no cell in this row</summary>
      Empty
    }
    #endregion

    //------------------------------------------------------------------------------------------16.02.2006
    #if Compatible_0_8
    //----------------------------------------------------------------------------------------------------

    public enum TextMode {
      /// <summary>Single line text mode, text is trimmed to the nearest character and an ellipsis is inserted at the end of the line.</summary>
      EllipsisCharacter,
      /// <summary>Multiline text mode</summary>
      MultiLine,
      /// <summary>Multiline text mode, each line committed</summary>
      SingleMultiLine,
      /// <summary>Fallback: text mode of the fallback cell definition</summary>
      FallBack
    }

    [Obsolete("use 'TlmHeightMode'")]
    public enum TableHeight {
      /// <summary>Adjust height of last container</summary>
      AdjustLast,
      /// <summary>Adjust height of each container</summary>
      Adjust,
      /// <summary>No adjustment</summary>
      Static
    }

    /// <summary>Table height mode</summary>
    [Obsolete("use 'tlmHeightMode'")]
    public TableHeight tableHeight {
      get {
        switch (tlmHeightMode) {
          case TlmHeightMode.Adjust: { return TableHeight.Adjust; }
          case TlmHeightMode.AdjustLast: { return TableHeight.AdjustLast; }
        }
        return TableHeight.Static;
      }
      set {
        switch (value) {
          case TableHeight.Adjust: { tlmHeightMode = TlmHeightMode.Adjust; break; }
          case TableHeight.AdjustLast: { tlmHeightMode = TlmHeightMode.AdjustLast; break; }
          default: { tlmHeightMode = TlmHeightMode.Static; break; }
        }
      }
    }
    /// <summary>Table height mode (VB version)</summary>
    [Obsolete("use 'tlmHeightMode'")]
    public TableHeight _tableHeight {
      get { return tableHeight; }
      set { tableHeight = value; }
    }

    //------------------------------------------------------------------------------------------06.01.2004
    /// <summary>Definition of the default properties of a column of this table.</summary>
    public class ColumnDef : TlmColumnDef {
      //------------------------------------------------------------------------------------------06.01.2004
      /// <summary>Creates a definition structure for the default values of a column of this table.</summary>
      internal ColumnDef() {
      }
    }

    //------------------------------------------------------------------------------------------06.01.2004
    /// <summary>Definition of the default properties of a row of this table.</summary>
    public class RowDef : TlmRowDef {
      //------------------------------------------------------------------------------------------06.01.2004
      /// <summary>Creates a definition structure for the default values of a row of this table.</summary>
      internal RowDef() {
      }
    }

    /// <summary>Definition of the default properties of a cell of this table.</summary>
    [Obsolete("use 'tlmCellDef_Default'")]
    public TlmCellDef cellDef {
      get { return tlmCellDef_Default; }
    }
    /// <summary>Definition of the default properties of a cell of this table (VB version)</summary>
    [Obsolete("use 'tlmCellDef_Default'")]
    public TlmCellDef _cellDef {
      get { return tlmCellDef_Default; }
    }

    /// <summary>Definition of the default properties of a column of this table (VB version)</summary>
    [Obsolete("use 'tlmColumnDef_Default'")]
    public TlmColumnDef columnDef {
      get { return tlmColumnDef_Default; }
    }

    /// <summary>Definition of the default properties of a column of this table (VB version)</summary>
    [Obsolete("use 'tlmColumnDef_Default'")]
    public TlmColumnDef _columnDef {
      get { return tlmColumnDef_Default; }
    }

    /// <summary>Definition of the default properties of a row of this table (VB version)</summary>
    [Obsolete("use 'tlmRowDef_Default'")]
    public TlmRowDef rowDef {
      get { return tlmRowDef_Default; }
    }

    /// <summary>Definition of the default properties of a row of this table (VB version)</summary>
    [Obsolete("use 'tlmRowDef_Default'")]
    public TlmRowDef _rowDef {
      get { return tlmRowDef_Default; }
    }

    [Obsolete("use 'fontProp_Header'")]
    public FontProp fp_Header {
      get { return fontProp_Header; }
      set { fontProp_Header = value; }
    }
    #endif
  }

  //------------------------------------------------------------------------------------------06.01.2004
  #region TlmTextMode
  //----------------------------------------------------------------------------------------------------x

  /// <summary>Text mode</summary>
  public enum TlmTextMode {
    /// <summary>Single line text mode, text is trimmed to the nearest character and an ellipsis is inserted at the end of the line.</summary>
    EllipsisCharacter,
    /// <summary>Multiline text mode</summary>
    MultiLine,
    /// <summary>Multiline text mode, each line committed</summary>
    SingleMultiLine,
    /// <summary>Fallback: text mode of the fallback cell definition</summary>
    FallBack
  }
  #endregion

  //------------------------------------------------------------------------------------------06.01.2004
  /// <summary>Table height mode</summary>
  public enum TlmHeightMode {
    /// <summary>Adjust height of last container</summary>
    AdjustLast,
    /// <summary>Adjust height of each container</summary>
    Adjust,
    /// <summary>No adjustment</summary>
    Static
  }
}
