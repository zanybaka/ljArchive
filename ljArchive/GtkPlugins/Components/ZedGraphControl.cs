using System;
using System.IO;
using Gtk;
using Gdk;
using Glade;
using ZedGraph;

namespace EF.ljArchive.Plugins.Gtk.Components
{
	class ZedGraphControl : DrawingArea
	{
		public ZedGraphControl ()
		{
			System.Drawing.Rectangle rect = new System.Drawing.Rectangle( 0, 0, 50, 50);
			graphPane = new GraphPane( rect, "Title", "X-Axis", "Y-Axis" );
			graphPane.AxisChange();
			m = new System.Drawing.Drawing2D.Matrix();
		}

		public GraphPane GraphPane
		{
			get { return graphPane; }
			set
			{
				graphPane = value;
				Invalidate();
			}
		}
		
		public bool RotateClockwise
		{
			get { return rotateClockwise; }
			set
			{
				rotateClockwise = value;
				Invalidate();
			}
		}
		
		protected override bool OnExposeEvent (Gdk.EventExpose args)
		{
			// slow - no buffering at all
			// and potentially heavy operations using "proxy" graphics
			System.Drawing.Graphics g = global::Gtk.DotNet.Graphics.FromDrawable(args.Window);
			if (rotateClockwise)
			{
				m.Reset();
				m.Translate(this.Width - 1, 0);
				m.Rotate(90);
				g.Transform = m;
			}
			graphPane.Draw(g);
			return true;
		}
		
		protected override void OnSizeAllocated (Gdk.Rectangle rect)
		{
			width = rect.Width;
			height = rect.Height;
			if (rotateClockwise)
				this.graphPane.PaneRect = new System.Drawing.RectangleF( 0, 0, rect.Height - 1, rect.Width - 1);
			else
				this.graphPane.PaneRect = new System.Drawing.RectangleF( 0, 0, rect.Width - 1, rect.Height - 1);
			base.OnSizeAllocated(rect);
		}
		
		public void Invalidate()
		{
			if (this.GdkWindow != null)
				this.GdkWindow.InvalidateRegion(this.GdkWindow.ClipRegion, false);
		}
		
		public int Width
		{
			get { return width; }
		}
		
		public int Height
		{
			get { return height; }
		}
		
		bool rotateClockwise = false;
		private int width;
		private int height;
		private GraphPane graphPane;
		private System.Drawing.Drawing2D.Matrix m;
	}
}