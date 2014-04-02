using System.Globalization;
using System.Windows;
using System.Windows.Media;
using HLGranite.WPF;

namespace OpenGoldenRuler
{
    public class PinLine : FrameworkElement
    {
        private readonly Pen BlackPen = new Pen(Brushes.Black, 1.5);

        #region Properties

        #region Length

        public double Length
        {
            get
            {
                return (double)GetValue(LengthProperty);
            }
            set
            {
                SetValue(LengthProperty, value);
            }
        }

        /// <summary>
        /// Identifies the Length dependency property.
        /// </summary>
        public static readonly DependencyProperty LengthProperty =
             DependencyProperty.Register(
                  "Length",
                  typeof(double),
                  typeof(PinLine),
                  new FrameworkPropertyMetadata(0D, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region Color

        public Color Color
        {
            get
            {
                return (Color)GetValue(ColorProperty);
            }
            set
            {
                SetValue(ColorProperty, value);
            }
        }

        /// <summary>
        /// Identifies the Length dependency property.
        /// </summary>
        public static readonly DependencyProperty ColorProperty =
             DependencyProperty.Register(
                  "Color",
                  typeof(Color),
                  typeof(PinLine),
                  new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region CurrentAngle

        public int CurrentAngle
        {
            get
            {
                return (int)GetValue(CurrentAngleProperty);
            }
            set
            {
                SetValue(CurrentAngleProperty, value);
            }
        }

        /// <summary>
        /// Identifies the Length dependency property.
        /// </summary>
        public static readonly DependencyProperty CurrentAngleProperty =
             DependencyProperty.Register(
                  "CurrentAngle",
                  typeof(int),
                  typeof(PinLine),
                  new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region Text
        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        /// <summary>
        /// Identifies the Length dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
             DependencyProperty.Register(
                  "Text",
                  typeof(string),
                  typeof(PinLine),
                  new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #endregion

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            BlackPen.Brush = new SolidColorBrush(Color);

            if(CurrentAngle == 0) drawingContext.DrawLine(BlackPen, new Point(0,0), new Point(0, Length) );

            if (CurrentAngle == 90) drawingContext.DrawLine(BlackPen, new Point(0, 0), new Point(Length, 0));

            if (!string.IsNullOrEmpty(Text))
            {
                FormattedText ft = new FormattedText(Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), DipHelper.PtToDip(8), BlackPen.Brush);
                drawingContext.DrawText(ft, new Point(-15,-15));
            }
        }
    }
}
