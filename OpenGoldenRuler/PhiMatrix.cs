using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenGoldenRuler
{
    /// <summary>
    /// http://www.phimatrix.com/phi-concepts-applications/
    /// </summary>
    public class PhiMatrix:FrameworkElement
    {
        private readonly Pen RedPen = new Pen(Brushes.Red, 2.0);

        private const double GOLDEN_RATIO = 1.618;

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
                  typeof(PhiMatrix),
                  new FrameworkPropertyMetadata(0D, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region CurrentAngle

        public int MatrixMode
        {
            get
            {
                return (int)GetValue(MatrixModeProperty);
            }
            set
            {
                SetValue(MatrixModeProperty, value);
            }
        }

        /// <summary>
        /// Identifies the Length dependency property.
        /// </summary>
        public static readonly DependencyProperty MatrixModeProperty =
             DependencyProperty.Register(
                  "MatrixMode",
                  typeof(int),
                  typeof(PhiMatrix),
                  new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            double a = Length / GOLDEN_RATIO;
           
            GeneratePhiMatrix(new Rect(0, 0, Length, a), drawingContext, 11, MatrixMode);
        }

        private void GeneratePhiMatrix(Rect ParentRect, DrawingContext drawingContext, int maxLevel, int currentAngle = 0)
        {
            Pen BlackPen;

            if (maxLevel <= 0) return;

            int absCurrentAngle = currentAngle % 360;

            Point startPoint, drawPoint;
            double a = ParentRect.GetLongerLine(), a1 = a / GOLDEN_RATIO;
            Size newRectSize, newSquareSize;

            if (absCurrentAngle == 0)
            {
                startPoint = new Point(ParentRect.X + a1, ParentRect.Y);
                drawPoint = new Point(ParentRect.X, ParentRect.Y);

                newRectSize = new Size(a - a1, a1);
                newSquareSize = new Size(a1, a1);
            }
            else if (absCurrentAngle == 90)
            {
                startPoint = new Point(ParentRect.X, ParentRect.Y + a1);
                drawPoint = new Point(ParentRect.X, ParentRect.Y);

                newRectSize = new Size(a1, a - a1);
                newSquareSize = new Size(a1, a1);
            }
            else if (absCurrentAngle == 180)
            {
                startPoint = new Point(ParentRect.X, ParentRect.Y);
                drawPoint = new Point(ParentRect.X + a - a1, ParentRect.Y);

                newRectSize = new Size(a - a1, a1);
                newSquareSize = new Size(a1, a1);
            }
            else if (absCurrentAngle == 270)
            {
                startPoint = new Point(ParentRect.X, ParentRect.Y);
                drawPoint = new Point(ParentRect.X, ParentRect.Y + a - a1);

                newRectSize = new Size(a1, a - a1);
                newSquareSize = new Size(a1, a1);
            }
            else
            {
                throw new NotImplementedException();
            }

            BlackPen = new Pen(new SolidColorBrush(GoldenUtils.ColorStack[maxLevel % GoldenUtils.ColorStack.Count]), 1.5);

            drawingContext.DrawRectangle(Brushes.Transparent, BlackPen, new Rect(startPoint, newRectSize));
            drawingContext.DrawRectangle(Brushes.Transparent, BlackPen, new Rect(drawPoint, newSquareSize));

            GeneratePhiMatrix(new Rect(startPoint, newRectSize), drawingContext, --maxLevel, currentAngle);
        } 
    }

    public enum PhiMatrixDockMode
    {
        Left = 1,
        Top = 2,
        Right = 4,
        Bottom = 8,
    }
}
