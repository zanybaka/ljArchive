using System;
using System.Diagnostics;
using System.Text;
using System.Collections;
using System.Drawing;

// Creation date: 17.10.2002
// Checked: xx.05.2002
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
  /// <summary>Flow Layout Manager</summary>
  public class FlowLayoutManager : LayoutManager {
    /// <summary>Current horizontal position</summary>
    public Double rX_Cur = 0;

    /// <summary>Current vertical position</summary>
    public Double rY_Cur = 0;

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
    /// <summary>Creates a new flow layout manager.</summary>
    public FlowLayoutManager() {
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Creates a new flow layout manager.</summary>
    /// <param name="container">Container that must be bound to this layout manager</param>
    public FlowLayoutManager(Container container) : this() {
      this._container_Cur = container;
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Gets the report object of this layout manager.</summary>
    public Report report {
      get { return _container_Cur.report; }
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Gets or sets the current horizontal position in millimeters.</summary>
    public Double rX_CurMM {
      get { return RT.rMMFromPoint(rX_Cur); }
      set { rX_Cur = RT.rPointFromMM(value); }
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Gets or sets the current vertical position in millimeters.</summary>
    public Double rY_CurMM {
      get { return RT.rMMFromPoint(rY_Cur); }
      set { rY_Cur = RT.rPointFromMM(value); }
    }
    
    public void AddBlock(FontProp fp, string s)
    {
    	AddBlock(new RepObj[] {new RepString(fp, s)});
    }

    /// <summary>
    /// Adds lines of report objects to the current container at the current position.  Breaks
    /// RepString objects across multiple lines.
    /// </summary>
    /// <param name="repObjs"></param>
    public void AddBlock(RepObj[] repObjs)
    {
    	if (status == Status.Init && _container_Cur == null)
			CreateNewContainer();

    	while (repObjs != null)
    		repObjs = AddLine(repObjs);
    }
    
    /// <summary>
    /// Adds a line of report objects to the current container at the current position.
    /// </summary>
    /// <param name="repObjs">Array of report objects to add to the container</param>
    /// <returns>Leftover report objects that didn't fit in the line.</returns>
    public RepObj[] AddLine(RepObj[] repObjs)
    {
		if (status == Status.Init && _container_Cur == null)
			CreateNewContainer();
		double rMaxHeight = 0;
		double rMaxStringHeight = 0;
		double rWidth = 0;
		rX_Cur = 0;
		ArrayList line = new ArrayList();
		ArrayList leftover = new ArrayList();
		foreach(RepObj repObj in repObjs)
		{
			if (rWidth + RealWidth(repObj) <= container_Cur.rWidth)
			{
				line.Add(repObj);
			}
			else if (rWidth < container_Cur.rWidth && repObj is RepString)
			{
				RepString repString = (RepString) repObj;
				RepString repStringLeftover;
				BreakByWord(ref repString, out repStringLeftover, container_Cur.rWidth - rWidth);
				if (repString != null)
				{
					line.Add(repString);
				}
				if (repStringLeftover != null)
					leftover.Add(repStringLeftover);
			}
			else
			{
				leftover.Add(repObj);
			}
			rWidth += RealWidth(repObj);
		}
		// if line.Count is zero and leftover.count isn't, we've got a single RepObj bigger than
		// its container
		if (line.Count == 0 && leftover.Count > 0)
		{
			RepObj repObj = (RepObj) leftover[0];
			if (repObj is RepString) // can't break by word, so break by letter
			{
				RepString repStringLeftover;
				RepString repString = (RepString) repObj;
				BreakByLetter(ref repString, out repStringLeftover, container_Cur.rWidth);
				if (repString != null)
					line.Add(repString);
				leftover.RemoveAt(0);
				if (repStringLeftover != null)
					leftover.Insert(0, repStringLeftover);
			}
			else
			{
				line.Add(leftover[0]);
				leftover.RemoveAt(0);
			}
		}
		// compute max hight and max line hight for alignment
		foreach(RepObj repObj in line)
		{
			if (repObj is RepString && RealHight(repObj) > rMaxStringHeight)
				rMaxStringHeight = RealHight(repObj);
			if (RealHight(repObj) > rMaxHeight)
				rMaxHeight = RealHight(repObj);
		}
		rY_Cur += rMaxHeight;
		rMaxStringHeight /= 4; // poor man's line leading
		if (rY_Cur > container_Cur.rHeight)
		{
            _container_Cur = null;
            CreateNewContainer();
            rY_Cur += rMaxHeight;
        }
		foreach(RepObj repObj in line)
		{
			if (repObj is RepString)
				container_Cur.Add(rX_Cur, rY_Cur - rMaxStringHeight, repObj);
			else
				container_Cur.Add(RealX(repObj, rX_Cur), RealY(repObj, rY_Cur), repObj);
			rX_Cur += RealWidth(repObj);
		}
		if (leftover.Count > 0)
			return (RepObj[]) leftover.ToArray(typeof(RepObj));
		else
			return null;
    }

    protected double RealY(RepObj repObj, double r_Y)
    {
    	if (repObj is RepRect)
    	{
    		RepRect repRect = (RepRect) repObj;
    		return r_Y - repRect.penProp.rWidth / 2;
    	}
    	else
    		return r_Y;
    }
    
    protected double RealX(RepObj repObj, double r_X)
    {
    	if (repObj is RepRect)
    	{
    		RepRect repRect = (RepRect) repObj;
    		return r_X + repRect.penProp.rWidth / 2;
    	}
    	else
	    	return r_X;
    }

    protected double RealHight(RepObj repObj)
    {
    	if (repObj is RepRect)
    	{
    		RepRect repRect = (RepRect) repObj;
    		return repRect.rHeight + repRect.penProp.rWidth;
    	}
    	else if (repObj is RepString)
    	{
    		RepString repString = (RepString) repObj;
    		return repString.fontProp.rLineFeed;
    	}
    	else if (repObj is RepImage)
    	{
    		RepImage repImage = (RepImage) repObj;
    		if (Double.IsNaN(repImage.rHeight))
    		{
    			using (Image image = Image.FromStream(repImage.stream))
    			{
    				if (Double.IsNaN(repImage.rWidth))
    				{
    					repImage.rWidth = image.Width / image.HorizontalResolution * 72;
            			repImage.rHeight = image.Height / image.VerticalResolution * 72;
    				}
    				else
    					repImage.rHeight = image.Height * repImage.rWidth / image.Width;
    			}
    		}
    		return repImage.rHeight;
    	}
    	else
    		return repObj.rHeight;
    }
    
    protected double RealWidth(RepObj repObj)
    {
    	if (repObj is RepRect)
    	{
    		RepRect repRect = (RepRect) repObj;
    		return repRect.rWidth + repRect.penProp.rWidth;
    	}
    	else if (repObj is RepImage)
    	{
    		RepImage repImage = (RepImage) repObj;
    		if (Double.IsNaN(repImage.rWidth))
    		{
    			using (Image image = Image.FromStream(repImage.stream))
    			{
    				if (Double.IsNaN(repImage.rHeight))
    				{
    					repImage.rWidth = image.Width / image.HorizontalResolution * 72;
            			repImage.rHeight = image.Height / image.VerticalResolution * 72;
    				}
    				else
    					repImage.rWidth = image.Width * repImage.rHeight / image.Height;
    			}
    		}
    		return repImage.rWidth;
    	}
    	else
    		return repObj.rWidth;
    }
    
    protected void BreakByLetter(ref RepString repString, out RepString repStringLeftover, double rWidth)
    {
    	FontProp fp = repString.fontProp;
    	StringBuilder sb = new StringBuilder();
    	StringBuilder sbLeftover = new StringBuilder();
		double r_X = 0;

		foreach (char c in repString.sText)
		{
			r_X += fp.rWidth(Convert.ToString(c));
			if (r_X <= rWidth)
				sb.Append(c);
			else
				sbLeftover.Append(c);
		}
		if (sbLeftover.Length > 0)
    		repStringLeftover = new RepString(repString.fontProp, sbLeftover.ToString());
    	else
    		repStringLeftover = null;
    	if (sb.Length > 0)
    		repString = new RepString(repString.fontProp, sb.ToString());
    	else
    		repString = null;
    }
    
    protected void BreakByWord(ref RepString repString, out RepString repStringLeftover, double rWidth)
    {
    	FontProp fp = repString.fontProp;
    	StringBuilder sb = new StringBuilder();
    	StringBuilder sbLeftover = new StringBuilder();
    	double r_X = 0;
    	
    	foreach(string s in repString.sText.Split(null))
    	{
    		if (sb.Length > 0)
    			r_X += fp.rWidth(" ");
    		r_X += fp.rWidth(s);
    		if (r_X <= rWidth)
    		{
    			if (sb.Length > 0)
    				sb.Append(" ");
    			sb.Append(s);
    		}
    		else
    		{
    			if (sbLeftover.Length > 0)
    				sbLeftover.Append(" ");
    			sbLeftover.Append(s);
    		}
    	}
    	if (sbLeftover.Length > 0)
    		repStringLeftover = new RepString(repString.fontProp, sbLeftover.ToString());
    	else
    		repStringLeftover = null;
    	if (sb.Length > 0)
    		repString = new RepString(repString.fontProp, sb.ToString());
    	else
    		repString = null;
    }
    
    //----------------------------------------------------------------------------------------------------x
    /// <summary>Adds a report object to the current container at the current position.</summary>
    /// <param name="repObj">Report object to add to the container</param>
    public void Add(RepObj repObj) {
      if (status == Status.Init) {
        if (_container_Cur == null) {
          CreateNewContainer();
        }
      }

      if (repObj is RepString) {
        RepString repString = (RepString)repObj;
        FontProp fp = repString.fontProp;
        String sText = repString.sText;

        Int32 iLineStartIndex = 0;
        Int32 iIndex = 0;
        while (true) {
          if (rY_Cur > container_Cur.rHeight) {
            _container_Cur = null;
            CreateNewContainer();
          }
          Int32 iLineBreakIndex = 0;
          Double rPosX = rX_Cur;
          Double rLineBreakPos = 0;
          while (true) {
            if (iIndex >= sText.Length) {
              iLineBreakIndex = iIndex;
              rLineBreakPos = rPosX;
              break;
            }
            Char c = sText[iIndex];
            rPosX += fp.rWidth(Convert.ToString(c));
            if (rPosX >= container_Cur.rWidth) {
              if (iLineBreakIndex == 0) {
                if (iIndex == iLineStartIndex) {  // at least one character must be printed
                  iIndex++;
                }
                iLineBreakIndex = iIndex;
                break;
              }
              iIndex = iLineBreakIndex;
              break;
            }
            if (c == ' ') {
              iLineBreakIndex = iIndex + 1;
              rLineBreakPos = rPosX;
            }
            else if (c == '\n') {
              iLineBreakIndex = iIndex;
              iIndex++;
              break;
            }
            iIndex++;
          }

          if (iLineStartIndex == 0 && iIndex >= sText.Length) {  // add entire object
            container_Cur.Add(rX_Cur, rY_Cur, repObj);
            rX_Cur = rLineBreakPos;
            break;
          }
          String sLine = sText.Substring(iLineStartIndex, iLineBreakIndex - iLineStartIndex);
          container_Cur.Add(rX_Cur, rY_Cur, new RepString(fp, sLine));
          if (iIndex >= sText.Length) {
            rX_Cur = rLineBreakPos;
            break;
          }
          rX_Cur = 0;
          rY_Cur += fp.rLineFeed;
          iLineStartIndex = iIndex;
        }
      }
      else {
        Debug.Fail("Unknown object type");
        container_Cur.Add(rX_Cur, rY_Cur, repObj);
      }
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Adds a report object to the current container on a new line.</summary>
    /// <param name="repString">Report object to add to the container</param>
    public void AddNew(RepString repString) {
      NewLine(repString.fontProp.rLineFeed);
      Add(repString);
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Creates a new container.</summary>
//    public void NewContainer() {
//      OnNewContainer(new NewContainerEventArgs(this));
//    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Sets variable _container to the next container.</summary>
//    protected override void NextContainer() {
//      NewContainer();
//    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Makes a new line.</summary>
    /// <param name="rLineFeed">Line feed</param>
    public void NewLine(Double rLineFeed) {
      rX_Cur = 0;
      if (rY_Cur + rLineFeed > container_Cur.rHeight) {
        _container_Cur = null;
        CreateNewContainer();
      }
      rY_Cur += rLineFeed;
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Makes a new line (metric version).</summary>
    /// <param name="rLineFeedMM">Line feed in millimeters</param>
    public void NewLineMM(Double rLineFeedMM) {
      NewLine(RT.rPointFromMM(rLineFeedMM));
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Sets a new current container for the layout manager.</summary>
    /// <param name="container">New container</param>
    /// <remarks>The current position will be set to the upper left corner (0, 0).</remarks>
//    public void SetContainer(Container container) {
//      _container = container;
//      rX_Cur = 0;
//      rY_Cur = 0;
//    }

    //----------------------------------------------------------------------------------------------------x
    #region Container
    //----------------------------------------------------------------------------------------------------x

    /// <summary>Default height of the container (points, 1/72 inch)</summary>
    public Double rContainerHeight = Double.NaN;

    /// <summary>Default height of the table (mm)</summary>
    public Double rContainerHeightMM {
      get { return RT.rMMFromPoint(rContainerHeight); }
      set { rContainerHeight = RT.rPointFromMM(value); }
    }

    /// <summary>Default width of the container (points, 1/72 inch)</summary>
    public Double rContainerWidth = Double.NaN;

    /// <summary>Width of the table (mm)</summary>
    public Double rContainerWidthMM {
      get { return RT.rMMFromPoint(rContainerWidth); }
      set { rContainerWidth = RT.rPointFromMM(value); }
    }

    private Container _container_Cur;
    /// <summary>Current container</summary>
    public Container container_Cur {
      get { return _container_Cur; }
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>Provides data for the NewContainer event</summary>
    public class NewContainerEventArgs : EventArgs {
      /// <summary>Flow layout manager</summary>
      public readonly FlowLayoutManager flowLayoutManager;

      /// <summary>New container</summary>
      public readonly Container container;

      /// <summary>Creates a data object for the NewContainer event.</summary>
      /// <param name="flowLayoutManager">Handle of the flow layout manager</param>
      /// <param name="container">New container: this container must be added to a page or a container.</param>
      internal NewContainerEventArgs(FlowLayoutManager flowLayoutManager, Container container) {
        this.flowLayoutManager = flowLayoutManager;
        this.container = container;
      }

      //----------------------------------------------------------------------------------------------------
      #if Compatible_0_8
      //----------------------------------------------------------------------------------------------------

      [Obsolete("use 'flowLayoutManager'")]
      public FlowLayoutManager flm {
        get { return flowLayoutManager; }
      }
      #endif
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
        _container_Cur = new StaticContainer(rContainerWidth, rContainerHeight);
        NewContainerEventArgs ea = new NewContainerEventArgs(this, _container_Cur);
        OnNewContainer(ea);
      }
      rX_Cur = 0;
      rY_Cur = 0;
    }
    
    public void Reset()
    {
		_container_Cur = new StaticContainer(rContainerWidth, rContainerHeight);
		NewContainerEventArgs ea = new NewContainerEventArgs(this, _container_Cur);
		OnNewContainer(ea);
		rX_Cur = 0;
		rY_Cur = 0;  
    }

    //----------------------------------------------------------------------------------------------------x
    /// <summary>This method will create a new container that will be added to the parent container at the specified position.</summary>
    /// <param name="container_Parent">Parent container</param>
    /// <param name="rX">X-coordinate of the new container (points, 1/72 inch)</param>
    /// <param name="rY">Y-coordinate of the new container (points, 1/72 inch)</param>
    /// <exception cref="ReportException">The layout manager status is not 'Init'</exception>
    public Container container_Create(Container container_Parent, Double rX, Double rY) {
//      if (status != Status.Init && status != Status.Closed) {
//        throw new ReportException("The layout manager must be in initialization mode or it must be closed; cannot create a new container.");
//      }
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
  }
}
