using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HLGranite.WPF;

namespace OpenGoldenRuler
{
    /// <summary>
    /// This component is used to measure
    /// </summary>
    public class GapMeasurer:FrameworkElement
    {
        #region Properties

        #region Pins

        public List<PinLine> Pins
        {
            get
            {
                return (List<PinLine>)GetValue(PinsProperty);
            }
            set
            {
                SetValue(PinsProperty, null);
                SetValue(PinsProperty, value);
            }
        }

        /// <summary>
        /// Identifies the Length dependency property.
        /// </summary>
        public static readonly DependencyProperty PinsProperty =
             DependencyProperty.Register(
                  "Pins",
                  typeof(List<PinLine>),
                  typeof(GapMeasurer),
                  new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #endregion

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (Pins != null && Pins.Count > 1)
            {
                bool isHorizontal = Pins[0].CurrentAngle == 0;

                double left1, left2, gap;

                List<PinLine> orderedPins = Pins.SortPins(isHorizontal);

                double offset = 0;

                for (int i = 0; i < orderedPins.Count-1; i++)
                {
                    if( i+1 >= orderedPins.Count ) break;

                    left1 = isHorizontal? Canvas.GetLeft(orderedPins[i]): Canvas.GetTop(orderedPins[i]);
                    left2 = isHorizontal ? Canvas.GetLeft(orderedPins[i + 1]) : Canvas.GetTop(orderedPins[i+1]);

                    Pen BlackPen = new Pen(new SolidColorBrush(orderedPins[i].Color), 1.5);

                    gap = left2 - left1;

                    FormattedText ft = new FormattedText(gap.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), DipHelper.PtToDip(8), BlackPen.Brush);

                    if (isHorizontal)
                    {
                        drawingContext.DrawLine(BlackPen, new Point(left1 - offset, 20), new Point(left2 - offset, 20));

                        if (left2 - left1 - ft.Width > 0) drawingContext.DrawText(ft, new Point((left2 - left1 - ft.Width) / 2 + left1 - offset, 0));
                        else drawingContext.DrawText(ft, new Point(left1 - offset, 0));
                    }
                    else
                    {
                        drawingContext.DrawLine(BlackPen, new Point(20, left1 - offset), new Point(20, left2 - offset));

                        if (left2 - left1 - ft.Width > 0) drawingContext.DrawText(ft, new Point(20,(left2 - left1 - ft.Width) / 2 + left1 - offset));
                        else drawingContext.DrawText(ft, new Point(20, left1 - offset));
                    }
                }

            }
        }
    }
}
