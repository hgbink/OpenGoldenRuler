using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenGoldenRuler
{
    public static class GoldenUtils
    {
        /// <summary>
        /// Used to sort a list of pin lines based on its position
        /// </summary>
        /// <param name="Pins">a list of pins</param>
        /// <param name="isHorizontal">true is the current ruler is horizontal or current angle is 0</param>
        /// <returns></returns>
        public static List<PinLine> SortPins(this List<PinLine> Pins, bool isHorizontal)
        {
            return isHorizontal ? new List<PinLine>(Pins.Where(p => !string.IsNullOrWhiteSpace(p.Text)).OrderBy(Canvas.GetLeft)) : new List<PinLine>(Pins.OrderBy(Canvas.GetTop));
        } 

        /// <summary>
        /// Used to store a list of colors that will be used in this project
        /// </summary>
        public static List<Color> ColorStack = new List<Color>(){Colors.Black, Colors.Orange, Colors.Yellow, Colors.Green, Colors.Aqua, Colors.Blue, Colors.Purple}; 

        /// <summary>
        /// Draw an Arc of an ellipse or circle. Static extension method of DrawingContext.
        /// </summary>
        /// <param name="dc">DrawingContext</param>
        /// <param name="pen">Pen for outline. set to null for no outline.</param>
        /// <param name="brush">Brush for fill. set to null for no fill.</param>
        /// <param name="rect">Box to hold the whole ellipse described by the arc</param>
        /// <param name="startDegrees">Start angle of the arc degrees within the ellipse. 0 degrees is a line to the right.</param>
        /// <param name="sweepDegrees">Sweep angle, -ve = Counterclockwise, +ve = Clockwise</param>
        public static void DrawArc(this DrawingContext dc, Pen pen, Brush brush, Rect rect, double startDegrees, double sweepDegrees)
        {
            GeometryDrawing arc = CreateArcDrawing(rect, startDegrees, sweepDegrees);
            dc.DrawGeometry(brush, pen, arc.Geometry);
        }

        /// <summary>
        /// Draw an quarter cicle of an ellipse or circle. Static extension method of DrawingContext.
        /// </summary>
        /// <param name="dc">DrawingContext</param>
        /// <param name="pen">Pen for outline. set to null for no outline.</param>
        /// <param name="brush">Brush for fill. set to null for no fill.</param>
        /// <param name="rect">Box to hold the whole ellipse described by the arc</param>
        /// <param name="startDegrees">Start angle of the arc degrees within the ellipse. 0 degrees is a line to the right.</param>
        /// <param name="sweepDegrees">Sweep angle, -ve = Counterclockwise, +ve = Clockwise</param>
        public static void DrawQuarterCicle(this DrawingContext dc, Pen pen, Brush brush, Rect rect, double startDegrees,double sweepDegrees)
        {
            double absDegrees = startDegrees%360;

            Rect newRect = rect;

            if (absDegrees >= 180 && absDegrees < 270)
            {
                newRect = new Rect(new Point(rect.X, rect.Y), new Size(rect.Size.Width*2, rect.Height*2));
            }
            else if (absDegrees >= 270 && absDegrees < 360)
            {
                newRect = new Rect(new Point(rect.X - rect.Size.Width, rect.Y), new Size(rect.Size.Width * 2, rect.Height * 2));
            }
            else if (absDegrees >= 0 && absDegrees < 90)
            {
                newRect = new Rect(new Point(rect.X - rect.Size.Width, rect.Y - rect.Size.Height), new Size(rect.Size.Width * 2, rect.Height * 2));
            }
            else if (absDegrees >= 90 && absDegrees < 180)
            {
                newRect = new Rect(new Point(rect.X, rect.Y - rect.Size.Width), new Size(rect.Size.Width * 2, rect.Height * 2));
            }

            dc.DrawArc(pen, brush, newRect, startDegrees, sweepDegrees);
        }

        /// <summary>
        /// Create an Arc geometry drawing of an ellipse or circle
        /// </summary>
        /// <param name="rect">Box to hold the whole ellipse described by the arc</param>
        /// <param name="startDegrees">Start angle of the arc degrees within the ellipse. 0 degrees is a line to the right.</param>
        /// <param name="sweepDegrees">Sweep angle, -ve = Counterclockwise, +ve = Clockwise</param>
        /// <returns>GeometryDrawing object</returns>
        private static GeometryDrawing CreateArcDrawing(Rect rect, double startDegrees, double sweepDegrees)
        {
            // degrees to radians conversion
            double startRadians = startDegrees * Math.PI / 180.0;
            double sweepRadians = sweepDegrees * Math.PI / 180.0;

            // x and y radius
            double dx = rect.Width / 2;
            double dy = rect.Height / 2;

            // determine the start point 
            double xs = rect.X + dx + (Math.Cos(startRadians) * dx);
            double ys = rect.Y + dy + (Math.Sin(startRadians) * dy);

            // determine the end point 
            double xe = rect.X + dx + (Math.Cos(startRadians + sweepRadians) * dx);
            double ye = rect.Y + dy + (Math.Sin(startRadians + sweepRadians) * dy);

            // draw the arc into a stream geometry
            StreamGeometry streamGeom = new StreamGeometry();

            using (StreamGeometryContext ctx = streamGeom.Open())
            {
                bool isLargeArc = Math.Abs(sweepDegrees) > 180;
                SweepDirection sweepDirection = sweepDegrees < 0 ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;

                ctx.BeginFigure(new Point(xs, ys), false, false);
                ctx.ArcTo(new Point(xe, ye), new Size(dx, dy), 0, isLargeArc, sweepDirection, true, false);
            }

            // create the drawing
            GeometryDrawing drawing = new GeometryDrawing();
            drawing.Geometry = streamGeom;
            return drawing;
        }

        /// <summary>
        /// Used to get the shorter one between the width and height of a rectangle
        /// </summary>
        public static double GetShorterLine(this Rect rect)
        {
            return (rect.Size.Height > rect.Size.Width) ? rect.Size.Width : rect.Size.Height;
        }

        /// <summary>
        /// Used to get the longer one between the width and height of a rectangle
        /// </summary>
        public static double GetLongerLine(this Rect rect)
        {
            return (rect.Size.Height > rect.Size.Width)? rect.Size.Height: rect.Size.Width;
        }
    }

    public class MenuItemExtensions : DependencyObject
    {
        public static Dictionary<MenuItem, String> ElementToGroupNames = new Dictionary<MenuItem, String>();

        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.RegisterAttached("GroupName",
                                         typeof(String),
                                         typeof(MenuItemExtensions),
                                         new PropertyMetadata(String.Empty, OnGroupNameChanged));

        public static void SetGroupName(MenuItem element, String value)
        {
            element.SetValue(GroupNameProperty, value);
        }

        public static String GetGroupName(MenuItem element)
        {
            return element.GetValue(GroupNameProperty).ToString();
        }

        private static void OnGroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //Add an entry to the group name collection
            var menuItem = d as MenuItem;

            if (menuItem != null)
            {
                String newGroupName = e.NewValue.ToString();
                String oldGroupName = e.OldValue.ToString();
                if (String.IsNullOrEmpty(newGroupName))
                {
                    //Removing the toggle button from grouping
                    RemoveCheckboxFromGrouping(menuItem);
                }
                else
                {
                    //Switching to a new group
                    if (newGroupName != oldGroupName)
                    {
                        if (!String.IsNullOrEmpty(oldGroupName))
                        {
                            //Remove the old group mapping
                            RemoveCheckboxFromGrouping(menuItem);
                        }
                        ElementToGroupNames.Add(menuItem, e.NewValue.ToString());
                        menuItem.Checked += MenuItemChecked;
                    }
                }
            }
        }

        private static void RemoveCheckboxFromGrouping(MenuItem checkBox)
        {
            ElementToGroupNames.Remove(checkBox);
            checkBox.Checked -= MenuItemChecked;
        }


        static void MenuItemChecked(object sender, RoutedEventArgs e)
        {
            var menuItem = e.OriginalSource as MenuItem;
            foreach (var item in ElementToGroupNames)
            {
                if (item.Key != menuItem && item.Value == GetGroupName(menuItem))
                {
                    item.Key.IsChecked = false;
                }
            }
        }
    }

    public enum RulerModes
    {
        GoldenRectangle,
        Pin,
        None
    }
}
