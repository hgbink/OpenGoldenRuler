using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;


namespace OpenGoldenRuler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        private bool _isMouseLeftButtonDown = false;

        private const int MIN_RULER_LENGTH = 550;

        private const int MIN_RULER_READING = 200;

        private const int MIN_RULER_HEIGHT = 80;

        private const int PIN_LENGTH = 2000;

        private const double GOLDEN_RATIO = 1.618;

        private double STARTPOINT_X = 0;
        private double STARTPOINT_Y = 0;

        private int _currentAngle = 0;

        private List<PinLine> _pins= new List<PinLine>();

        private RulerModes _currentMode = RulerModes.GoldenRectangle;

        private GapMeasurer _gapMeasurer = null;

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            RulerLength = MIN_RULER_LENGTH;

            XRuler.MouseMove += XRuler_MouseMove;
            XRuler.PreviewMouseDown += XRuler_MouseLeftButtonDown;
            XRuler.PreviewMouseUp += XRuler_MouseLeftButtonUp;

            Loaded += MainWindow_Loaded;
            this.KeyDown += MainWindow_KeyDown;
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl + D
            if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.D))
            {
                if (_currentMode == RulerModes.Pin)
                {
                    if (btnDropPin.Header.ToString().StartsWith("remove pin")) RemoveCurrentMouseOverPin();
                    else DropPin();
                }
            }
            // Ctrl + R
            else if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.C))
            {
                if (_currentMode == RulerModes.Pin)
                {
                    RemovePins();
                }
            }
            else if  (e.Key == Key.Escape)
            {
                Close();
            }
            else if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.R))
            {
                RotateRuler();
            }
            else if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.M))
            {
                if (_currentMode == RulerModes.Pin)
                {
                    _currentMode = RulerModes.None;
                }
                else if (_currentMode == RulerModes.None)
                {
                    _currentMode = RulerModes.GoldenRectangle;
                }
                else
                {
                    _currentMode = RulerModes.Pin;
                }
                UpdateContextMenuButtons();
            }
        }

        #region Properties
        /// <summary>
        /// Get the current pin line that will move as user mouse over the ruler
        /// or add a pin (this added pin will be the new current pin and the old pin will stay in canvas) 
        /// </summary>
        public PinLine CurrentPin
        {
            get
            {
                if (_pins.Count == 0)
                {
                    _pins.Add(new PinLine(){CurrentAngle = this._currentAngle, IsHitTestVisible = false, Length = PIN_LENGTH, Color = GoldenUtils.ColorStack[(_pins.Count+1)%GoldenUtils.ColorStack.Count]});
                    RootCanvas.Children.Add(_pins[_pins.Count - 1]);
                }

                return _pins[_pins.Count - 1];
            }
            set
            {
                _pins.Add(value);
                RootCanvas.Children.Add(value);

                if(_pins.Count < 3) return;

                if (_gapMeasurer == null)
                {
                    _gapMeasurer = new GapMeasurer();
                    RootCanvas.Children.Add(_gapMeasurer);
                }

                _gapMeasurer.Pins = _pins;

                bool isHorizontal = _currentAngle == 0;

                if (isHorizontal) Canvas.SetTop(_gapMeasurer, 100);
                else Canvas.SetLeft(_gapMeasurer, 100);
            }
        }
        
        /// <summary>
        /// Used to set and get the ruler length
        /// </summary>
        public double RulerLength
        {
            get { return XRuler.Length; }
            set
            {
                double length = value > MIN_RULER_LENGTH ? value : (XRuler.Length == MIN_RULER_LENGTH) ? MIN_RULER_LENGTH -1 : MIN_RULER_LENGTH;

                XRuler.Length = length;
            }
        }

        /// <summary>
        /// Used to set and get the ruler height
        /// </summary>
        public double RulerHeight
        {
            get { return XRuler.Height; }
            set
            {
                double height = value > MIN_RULER_HEIGHT ? value : MIN_RULER_HEIGHT;

                XRuler.Height = height;
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Used to initialise the position of the ruler
        /// </summary>
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ResetWindowPosition();

            Canvas.SetLeft(XRuler, STARTPOINT_X);
            Canvas.SetTop(XRuler, STARTPOINT_Y);

            Canvas.SetLeft(TxtReadBlock, STARTPOINT_X);
            Canvas.SetTop(TxtReadBlock, 0);

            Canvas.SetLeft(GoldenRectangle, STARTPOINT_X);
            Canvas.SetTop(GoldenRectangle, STARTPOINT_Y + XRuler.Height);

            UpdateContextMenuButtons();
        }
        
        /// <summary>
        /// Used to handle the context menu item click
        /// </summary>
        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (e == null || e.Source == null || !(e.Source is MenuItem) || !IsLoaded) return;

            MenuItem item = e.Source as MenuItem;

            if (item.Header.ToString().ToLower().StartsWith("horizontal") || item.Header.ToString().ToLower().StartsWith("vertical"))
            {
                RotateRuler();
            }
            else if (item.Header.ToString().ToLower().StartsWith("golden rectangle"))
            {
                _currentMode = RulerModes.GoldenRectangle;
                UpdateContextMenuButtons();
            }
            else if (item.Header.ToString().ToLower().StartsWith("pin"))
            {
                _currentMode = RulerModes.Pin;
                this.Width = this.Height = PIN_LENGTH;
                UpdateContextMenuButtons();
            }
            else if (item.Header.ToString().ToLower().StartsWith("drop pin"))
            {
                DropPin();
            }
            else if (item.Header.ToString().ToLower().StartsWith("remove pin"))
            {
                RemoveCurrentMouseOverPin();
            }
            else if (item.Header.ToString().ToLower().StartsWith("clear pins"))
            {
               RemovePins();
            }
            else if (item.Header.ToString().ToLower().StartsWith("standard"))
            {
                _currentMode = RulerModes.None;
                UpdateContextMenuButtons();
            }
            else if (item.Header.ToString().ToLower().StartsWith("0 degrees"))
            {
                this.GoldenRectangle.CurrentAngle = 0;
            }
            else if (item.Header.ToString().ToLower().StartsWith("90 degrees"))
            {
                this.GoldenRectangle.CurrentAngle = 90;
            }
            else if (item.Header.ToString().ToLower().StartsWith("180 degrees"))
            {
                this.GoldenRectangle.CurrentAngle = 180;
            }
            else if (item.Header.ToString().ToLower().StartsWith("270 degrees"))
            {
                this.GoldenRectangle.CurrentAngle = 270;
            }
            else if (item.Header.ToString().ToLower().StartsWith("close"))
            {
                this.Close();
            }
            else if (item.Header.ToString().ToLower().StartsWith("about open golden ruler"))
            {
                System.Diagnostics.Process.Start("http://hgbink.github.io/OpenGoldenRuler/");
            }
        }


        #region Drag & Drop
        void XRuler_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseLeftButtonDown = false;
        }

        void XRuler_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseLeftButtonDown = true;
        }


        void XRuler_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseLeftButtonDown)
            {
                try
                {
                    this.DragMove();
                }
                catch (Exception)
                {
                    _isMouseLeftButtonDown = false;
                }
            }
            else if (e != null)
            {
                ProcessCurrentReading(XRuler.CurrentReading);

                if (_currentAngle / 90 % 2 == this.GoldenRectangle.CurrentAngle / 90 % 2)
                {
                    this.GoldenRectangle.Length = XRuler.CurrentReading;
                }
                else
                {
                    this.GoldenRectangle.Length = XRuler.CurrentReading * GOLDEN_RATIO;
                }
            }
        }
        
        #endregion

        #endregion

        #region Private methods
        private void DropPin()
        {
            CurrentPin.Text = XRuler.CurrentReading.ToString();
            CurrentPin = new PinLine() { CurrentAngle = this._currentAngle, IsHitTestVisible = false, Length = PIN_LENGTH, Color = GoldenUtils.ColorStack[(_pins.Count + 1) % GoldenUtils.ColorStack.Count] };
        }

        private void RemoveCurrentMouseOverPin()
        {
            CurrentPin.Text = XRuler.CurrentReading.ToString();
            CurrentPin = new PinLine() { CurrentAngle = this._currentAngle, IsHitTestVisible = false, Length = PIN_LENGTH, Color = GoldenUtils.ColorStack[(_pins.Count + 1) % GoldenUtils.ColorStack.Count] };
        }

        /// <summary>
        /// Used to remove all pins from canvas
        /// </summary>
        public void RemovePins()
        {
            foreach (PinLine line in _pins)
            {
                RootCanvas.Children.Remove(line);
            }

            _pins.Clear();

            if (_gapMeasurer != null)
            {
                RootCanvas.Children.Remove(_gapMeasurer);

                _gapMeasurer = null;
            }
        }

        /// <summary>
        /// Used to update context menu base on the current mode
        /// </summary>
        private void UpdateContextMenuButtons ()
        {
            switch (_currentMode)
            {
                case RulerModes.GoldenRectangle:
                    this.GoldenRectangle.Visibility = Visibility.Visible;
                    this.btnRectRotate.Visibility = Visibility.Visible;
                    btnClearPins.Visibility = Visibility.Collapsed;
                    btnDropPin.Visibility = Visibility.Collapsed;
                    RemovePins();
                    break;
                case RulerModes.Pin:
                    this.GoldenRectangle.Visibility = Visibility.Collapsed;
                    this.btnRectRotate.Visibility = Visibility.Collapsed;
                    btnClearPins.Visibility = Visibility.Visible;
                    btnDropPin.Visibility = Visibility.Visible;
                    break;
                case RulerModes.None:
                    this.GoldenRectangle.Visibility = Visibility.Collapsed;
                    this.btnRectRotate.Visibility = Visibility.Collapsed;
                    btnClearPins.Visibility = Visibility.Collapsed;
                    btnDropPin.Visibility = Visibility.Collapsed;
                    RemovePins();
                    break;
            }
        }

        /// <summary>
        /// Used to clear the current reading of the ruler
        /// </summary>
        private void ClearCurrentReadingDisplay()
        {
            txtReading.Text = string.Empty;

        }

        /// <summary>
        /// Used to show the current reading of the ruler on screen
        /// </summary>
        /// <param name="reading">the current double type reading </param>
        private void SetCurrentReadingDisplay(double reading)
        {
            txtReading.Text = reading.ToString();
        }
      
        /// <summary>
        /// Used to process the reading of the ruler as user moving mouse on the ruler
        /// </summary>
        /// <param name="currentReading"></param>
        private void ProcessCurrentReading(double currentReading)
        {
            if (currentReading > MIN_RULER_READING)
            {
                RulerLength = currentReading / 0.7;
            }
            else
            {
                RulerLength = MIN_RULER_LENGTH;
            }

            if(_currentAngle == 0) this.Width = RulerLength + MIN_RULER_LENGTH;
            else this.Height = RulerLength + MIN_RULER_LENGTH;

            SetCurrentReadingDisplay(currentReading);

            if (_currentMode == RulerModes.Pin && _currentAngle == 0)
            {
                Canvas.SetTop(CurrentPin, XRuler.Height);
                Canvas.SetLeft(CurrentPin, STARTPOINT_X+currentReading);

                if (_pins.Any(p => !string.IsNullOrWhiteSpace(p.Text) && Canvas.GetLeft(CurrentPin) == Canvas.GetLeft(p)))
                {
                    btnDropPin.Header = "Remove pin (Ctrl+D)";
                }
                else
                {
                    btnDropPin.Header = "Drop pin (Ctrl+D)";
                }
            }
            else if (_currentMode == RulerModes.Pin && _currentAngle == 90)
            {
                Canvas.SetLeft(CurrentPin, XRuler.Height);
                Canvas.SetTop(CurrentPin, STARTPOINT_Y + currentReading);

                if (_pins.Any(p => !string.IsNullOrWhiteSpace(p.Text) && Canvas.GetTop(CurrentPin) == Canvas.GetTop(p)))
                {
                    btnDropPin.Header = "Remove pin (Ctrl+D)";
                }
                else
                {
                    btnDropPin.Header = "Drop pin (Ctrl+D)";
                }
            }

        }

        /// <summary>
        /// Used to rotate a generic framework element.
        /// </summary>
        /// <param name="control">A reference to the framework element to be rotated</param>
        /// <param name="x0">The x axis value of the initial position point</param>
        /// <param name="y0">The y axis value of the initial position point</param>
        /// <param name="x1">The x axis value of the after-rotating position point</param>
        /// <param name="y1">The y axis value of the after-rotating position point</param>
        /// <param name="startAngle">The initial angle of the control</param>
        /// <param name="endAngle">The angle after rotation</param>
        private void RotateControl(FrameworkElement control,double x0, double y0, double x1, double y1, int startAngle, int endAngle)
        {
            if(control == null) return;

            //rotate transform
            var tg = new TransformGroup();
            RotateTransform rt = new RotateTransform();
            TranslateTransform tt = new TranslateTransform(0, 0);

            tg.Children.Add(rt);
            tg.Children.Add(tt);

            control.RenderTransform = tg;

            DoubleAnimation anim1 = new DoubleAnimation(x0, x1, TimeSpan.FromSeconds(.5));
            DoubleAnimation anim2 = new DoubleAnimation(y0, y1, TimeSpan.FromSeconds(.5));
            DoubleAnimation anim3 = new DoubleAnimation(startAngle, endAngle, TimeSpan.FromSeconds(.5));

            rt.BeginAnimation(RotateTransform.AngleProperty, anim3);
            tt.BeginAnimation(TranslateTransform.XProperty, anim1);
            tt.BeginAnimation(TranslateTransform.YProperty, anim2);
        }

        /// <summary>
        /// Used to rotate the ruler together with golden rectangle and ruler readings
        /// </summary>
        private void RotateRuler()
        {
            if (_currentAngle == 0)
            {
                RotateControl(XRuler, STARTPOINT_X, STARTPOINT_Y, STARTPOINT_X + XRuler.Height, STARTPOINT_Y, _currentAngle, _currentAngle + 90);
                RotateControl(TxtReadBlock, STARTPOINT_X, STARTPOINT_Y, STARTPOINT_X + XRuler.Height, STARTPOINT_Y, _currentAngle, _currentAngle + 90);
                RotateControl(GoldenRectangle, STARTPOINT_X, STARTPOINT_Y + XRuler.Height, STARTPOINT_X + XRuler.Height, STARTPOINT_Y - XRuler.Height, _currentAngle, _currentAngle);

                this.GoldenRectangle.Length = 0;

                _currentAngle = 90;
            }
            else
            {
                RotateControl(XRuler, STARTPOINT_X, STARTPOINT_Y, STARTPOINT_X, STARTPOINT_Y, _currentAngle, _currentAngle - 90);
                RotateControl(TxtReadBlock, STARTPOINT_X, STARTPOINT_Y, STARTPOINT_X, STARTPOINT_Y, _currentAngle, _currentAngle - 90);
                RotateControl(GoldenRectangle, STARTPOINT_X + XRuler.Height, STARTPOINT_Y - XRuler.Height, STARTPOINT_X, STARTPOINT_Y, _currentAngle-90, _currentAngle-90);

                this.GoldenRectangle.Length = 0;

                _currentAngle = 0;
            }

            ProcessCurrentReading(0);

            ClearCurrentReadingDisplay();

            RemovePins();
        }

        /// <summary>
        /// Used to set up the start up location of the ruler
        /// </summary>
        private void ResetWindowPosition()
        {
            // Get absolute location on screen of upper left corner of button
            Canvas.SetLeft(XRuler, XRuler.Height);

            Point locationFromScreen = XRuler.PointToScreen(new Point(10, 10));

            // Transform screen point to WPF device independent point

            PresentationSource source = PresentationSource.FromVisual(this);

            System.Windows.Point targetPoints = source.CompositionTarget.TransformFromDevice.Transform(locationFromScreen);

            this.Left = targetPoints.X;
            this.Top = targetPoints.Y;
        }

       
        #endregion
    }
}
