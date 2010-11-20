﻿using System;
using System.Windows;
using System.Windows.Input;

namespace OxyPlot.Wpf
{
    public class ZoomAction : MouseAction
    {
        public Point DownPoint;
        private bool isZooming;
        private OxyPlot.Axis xaxis;
        private OxyPlot.Axis yaxis;

        private Rect zoomRectangle;

        public ZoomAction(PlotControl pc)
            : base(pc)
        {
        }

        public override void OnMouseDown(Point pt, MouseButton button, int clickCount, bool control, bool shift)
        {
            base.OnMouseDown(pt, button, clickCount, control, shift);

            // RMB+Control is the same
            if (button == MouseButton.Right && control)
                button = MouseButton.Middle;

            if (button != MouseButton.Middle)
                return;

            // Middle button double click resets the axis
            if (clickCount == 2)
            {
                if (xaxis != null) xaxis.Reset();
                if (yaxis != null) yaxis.Reset();
                pc.Refresh();
            }

            pc.GetAxesFromPoint(pt, out xaxis, out yaxis);
            DownPoint = pt;
            zoomRectangle = new Rect(pt, pt);
            pc.ShowZoomRectangle(zoomRectangle);
            pc.CaptureMouse();
            pc.Cursor = Cursors.SizeNWSE;
            isZooming = true;
        }

        public override void OnMouseMove(Point pt, bool control, bool shift)
        {
            if (!isZooming)
                return;
            // var currentPoint = pc.InverseTransform(pt, xaxis, yaxis);
            if (pc.Model==null)
                return;

            if (yaxis == null)
            {
                DownPoint.Y = pc.Model.AxisMargins.Top;
                pt.Y = pc.ActualHeight - pc.Model.AxisMargins.Bottom;
            }
            if (xaxis == null)
            {
                DownPoint.X = pc.Model.AxisMargins.Left;
                pt.X = pc.ActualWidth - pc.Model.AxisMargins.Right;
            }

            zoomRectangle = CreateRect(DownPoint, pt);
            pc.ShowZoomRectangle(zoomRectangle);
        }

        private static Rect CreateRect(Point p0, Point p1)
        {
            double x = Math.Min(p0.X, p1.X);
            double w = Math.Abs(p0.X - p1.X);
            double y = Math.Min(p0.Y, p1.Y);
            double h = Math.Abs(p0.Y - p1.Y);
            return new Rect(x, y, w, h);
        }

        public override void OnMouseUp()
        {
            if (!isZooming)
                return;

            pc.Cursor = Cursors.Arrow;
            pc.ReleaseMouseCapture();
            pc.HideZoomRectangle();

            if (zoomRectangle.Width > 10 && zoomRectangle.Height > 10)
            {
                DataPoint p0 = pc.InverseTransform(zoomRectangle.TopLeft, xaxis, yaxis);
                DataPoint p1 = pc.InverseTransform(zoomRectangle.BottomRight, xaxis, yaxis);

                if (xaxis != null)
                    xaxis.Zoom(p0.X, p1.X);
                if (yaxis != null)
                    yaxis.Zoom(p0.Y, p1.Y);
                pc.Refresh();
            }
            isZooming = false;
            base.OnMouseUp();
        }

        public override void OnMouseWheel(Point pt, double delta, bool control, bool shift)
        {
            OxyPlot.Axis xa, ya;
            pc.GetAxesFromPoint(pt, out xa, out ya);
            DataPoint current = pc.InverseTransform(pt, xa, ya);
            double f = 0.001;
            if (control) f *= 0.2;
            double s = 1 + delta*f;
            if (xa != null)
                xa.ScaleAt(s, current.X);
            if (ya != null)
                ya.ScaleAt(s, current.Y);
            pc.Refresh();
        }
    }
}