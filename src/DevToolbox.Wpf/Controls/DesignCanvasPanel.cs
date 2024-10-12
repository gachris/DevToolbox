using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevToolbox.Wpf.Controls
{
    /// <summary>
    /// Represents a canvas panel that draws a customizable grid with horizontal and vertical lines.
    /// </summary>
    public partial class DesignCanvasPanel : Canvas
    {
        #region Fields/Consts

        /// <summary>
        /// Dependency property to control the visibility of the grid lines.
        /// </summary>
        public static readonly DependencyProperty ShowProperty = DependencyProperty.Register(
            nameof(Show), typeof(bool), typeof(DesignCanvasPanel), new PropertyMetadata(true, PropertyChangedCallback));

        /// <summary>
        /// Dependency property to control the scale factor for the grid lines.
        /// </summary>
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            nameof(Scale), typeof(double), typeof(DesignCanvasPanel), new PropertyMetadata(default(double), PropertyChangedCallback));

        /// <summary>
        /// Dependency property to set the number of horizontal grid lines.
        /// </summary>
        public static readonly DependencyProperty HorizontalLinesProperty = DependencyProperty.Register(
            nameof(HorizontalLines), typeof(int), typeof(DesignCanvasPanel), new PropertyMetadata(default(int), PropertyChangedCallback));

        /// <summary>
        /// Dependency property to set the number of vertical grid lines.
        /// </summary>
        public static readonly DependencyProperty VerticalLinesProperty = DependencyProperty.Register(
            nameof(VerticalLines), typeof(int), typeof(DesignCanvasPanel), new PropertyMetadata(default(int), PropertyChangedCallback));

        /// <summary>
        /// Dependency property to define the brush used for drawing the grid lines.
        /// </summary>
        public static readonly DependencyProperty StrokeBrushProperty = DependencyProperty.Register(
            nameof(StrokeBrush), typeof(Brush), typeof(DesignCanvasPanel), new PropertyMetadata(new SolidColorBrush(Colors.Black), PropertyChangedCallback));

        /// <summary>
        /// Dependency property to set the thickness of the grid lines.
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            nameof(StrokeThickness), typeof(double), typeof(DesignCanvasPanel), new PropertyMetadata(default(double), PropertyChangedCallback));

        /// <summary>
        /// Dependency property to define the dash style for the grid lines.
        /// </summary>
        public static readonly DependencyProperty StrokeDashStyleProperty = DependencyProperty.Register(
            nameof(StrokeDashStyle), typeof(DoubleCollection), typeof(DesignCanvasPanel), new PropertyMetadata(default, PropertyChangedCallback));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the grid lines are shown.
        /// </summary>
        public bool Show
        {
            get => (bool)GetValue(ShowProperty);
            set => SetValue(ShowProperty, value);
        }

        /// <summary>
        /// Gets or sets the scale factor for the grid lines.
        /// </summary>
        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        /// <summary>
        /// Gets or sets the number of horizontal grid lines.
        /// </summary>
        public int HorizontalLines
        {
            get => (int)GetValue(HorizontalLinesProperty);
            set => SetValue(HorizontalLinesProperty, value);
        }

        /// <summary>
        /// Gets or sets the number of vertical grid lines.
        /// </summary>
        public int VerticalLines
        {
            get => (int)GetValue(VerticalLinesProperty);
            set => SetValue(VerticalLinesProperty, value);
        }

        /// <summary>
        /// Gets or sets the brush used for drawing the grid lines.
        /// </summary>
        public Brush StrokeBrush
        {
            get => (Brush)GetValue(StrokeBrushProperty);
            set => SetValue(StrokeBrushProperty, value);
        }

        /// <summary>
        /// Gets or sets the thickness of the grid lines.
        /// </summary>
        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        /// <summary>
        /// Gets or sets the dash style for the grid lines.
        /// </summary>
        public DoubleCollection StrokeDashStyle
        {
            get => (DoubleCollection)GetValue(StrokeDashStyleProperty);
            set => SetValue(StrokeDashStyleProperty, value);
        }

        #endregion

        #region Methods Override

        /// <summary>
        /// Renders the grid lines on the canvas.
        /// </summary>
        /// <param name="dc">The drawing context in which to render.</param>
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (!Show)
            {
                return;
            }

            var drawingPen = new Pen(StrokeBrush, StrokeThickness / Scale);

            if (Math.Abs(ActualHeight) < 1 || Math.Abs(ActualWidth) < 1)
            {
                return;
            }

            for (var i = 1; i <= HorizontalLines; i++)
            {
                dc.DrawLine(drawingPen, new Point(0, (ActualHeight * i) / (HorizontalLines + 1)), new Point(ActualWidth, (ActualHeight * i) / (HorizontalLines + 1)));
            }
            
            for (var i = 1; i <= VerticalLines; i++)
            {
                dc.DrawLine(drawingPen, new Point((ActualWidth * i) / (VerticalLines + 1), 0), new Point((ActualWidth * i) / (VerticalLines + 1), ActualHeight));
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Callback method that is invoked when any of the dependency properties change.
        /// </summary>
        /// <param name="d">The dependency object where the property change occurred.</param>
        /// <param name="e">The event data containing information about the property change.</param>
        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as DesignCanvasPanel)?.InvalidateVisual();
        }

        #endregion
    }
}