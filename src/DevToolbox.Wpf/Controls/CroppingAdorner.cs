using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Point = System.Drawing.Point;

namespace DevToolbox.Wpf.Controls;

internal partial class CroppingAdorner : Adorner
{
    #region CropThumb

    private class CropThumb : Thumb
    {
        #region Overrides

        protected override Visual? GetVisualChild(int index) => null;

        protected override void OnRender(DrawingContext drawingContext) => drawingContext.DrawRoundedRectangle(Brushes.White, new Pen(Brushes.Black, 1), new Rect(new Size(_cpxThumbWidth, _cpxThumbWidth)), 1, 1);

        #endregion

        #region Methods

        public void SetPosition(double x, double y)
        {
            Canvas.SetTop(this, y - _cpxThumbWidth / 2);
            Canvas.SetLeft(this, x - _cpxThumbWidth / 2);
        }

        #endregion
    }

    #endregion

    #region PuncturedRect

    private class PuncturedRect : Shape
    {
        #region Fields/Consts

        public static readonly DependencyProperty RectInteriorProperty = DependencyProperty.Register("RectInterior", typeof(Rect), typeof(FrameworkElement),
             new FrameworkPropertyMetadata(
                 new Rect(0, 0, 0, 0),
                 FrameworkPropertyMetadataOptions.AffectsRender,
                 null,
                 (d, baseValue) => ((PuncturedRect)d).OnRectInteriorCoerce(baseValue),
                 false
             ),
             null
         );

        public static readonly DependencyProperty RectExteriorProperty =
            DependencyProperty.Register(
                "RectExterior",
                typeof(Rect),
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    new Rect(0, 0, double.MaxValue, double.MaxValue),
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure |
                    FrameworkPropertyMetadataOptions.AffectsParentArrange |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    null,
                    null,
                    false
                ),
                null
            );

        #endregion

        #region Properties

        public Rect RectInterior
        {
            get => (Rect)GetValue(RectInteriorProperty);
            set => SetValue(RectInteriorProperty, value);
        }

        public Rect RectExterior
        {
            get => (Rect)GetValue(RectExteriorProperty);
            set => SetValue(RectExteriorProperty, value);
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                var pthgExt = new PathGeometry();
                var pthfExt = new PathFigure { StartPoint = RectExterior.TopLeft };
                pthfExt.Segments.Add(new LineSegment(RectExterior.TopRight, false));
                pthfExt.Segments.Add(new LineSegment(RectExterior.BottomRight, false));
                pthfExt.Segments.Add(new LineSegment(RectExterior.BottomLeft, false));
                pthfExt.Segments.Add(new LineSegment(RectExterior.TopLeft, false));
                pthgExt.Figures.Add(pthfExt);

                var rectIntSect = Rect.Intersect(RectExterior, RectInterior);
                var pthgInt = new PathGeometry();
                var pthfInt = new PathFigure { StartPoint = rectIntSect.TopLeft };
                pthfInt.Segments.Add(new LineSegment(rectIntSect.TopRight, false));
                pthfInt.Segments.Add(new LineSegment(rectIntSect.BottomRight, false));
                pthfInt.Segments.Add(new LineSegment(rectIntSect.BottomLeft, false));
                pthfInt.Segments.Add(new LineSegment(rectIntSect.TopLeft, false));
                pthgInt.Figures.Add(pthfInt);

                var cmbg = new CombinedGeometry(GeometryCombineMode.Exclude, pthgExt, pthgInt);
                return cmbg;
            }
        }

        #endregion

        #region Constructor

        public PuncturedRect() : this(new Rect(0, 0, double.MaxValue, double.MaxValue), new Rect())
        {
        }

        public PuncturedRect(Rect rectExterior, Rect rectInterior)
        {
            RectInterior = rectInterior;
            RectExterior = rectExterior;
        }

        #endregion

        #region Methods

        private object OnRectInteriorCoerce(object value)
        {
            var rcExterior = RectExterior;
            var rcProposed = (Rect)value;
            var left = Math.Max(rcProposed.Left, rcExterior.Left);
            var top = Math.Max(rcProposed.Top, rcExterior.Top);
            var width = Math.Min(rcProposed.Right, rcExterior.Right) - left;
            var height = Math.Min(rcProposed.Bottom, rcExterior.Bottom) - top;
            rcProposed = new Rect(left, top, width, height);
            return rcProposed;
        }

        #endregion
    }

    #endregion

    #region Fields/Consts

    /// <summary>
    /// Width of the thumbs.
    /// </summary>
    private const int _cpxThumbWidth = 6;

    /// <summary>
    /// PuncturedRect to hold the "Cropping" portion of the adorner.
    /// </summary>
    private readonly PuncturedRect _puncturedRect;

    /// <summary>
    /// Canvas to hold the thumbs so they can be moved in response to the user.
    /// </summary>
    private readonly Canvas _canvasThumbs;

    /// <summary>
    /// TopLeft crop thumb.
    /// </summary>
    private readonly CropThumb _cropThumbTopLeft;

    /// <summary>
    /// TopRight crop thumb.
    /// </summary>
    private readonly CropThumb _cropThumbTopRight;

    /// <summary>
    /// BottomLeft crop thumb.
    /// </summary>
    private readonly CropThumb _cropThumbBottomLeft;

    /// <summary>
    /// BottomRight crop thumb.
    /// </summary>
    private readonly CropThumb _cropThumbBottomRight;

    /// <summary>
    /// Top crop thumb.
    /// </summary>
    private readonly CropThumb _cropThumbTop;

    /// <summary>
    /// Left crop thumb.
    /// </summary>
    private readonly CropThumb _cropThumbLeft;

    /// <summary>
    /// Bottom crop thumb.
    /// </summary>
    private readonly CropThumb _cropThumbBottom;

    /// <summary>
    /// Right crop thumb.
    /// </summary>
    private readonly CropThumb _cropThumbRight;

    /// <summary>
    /// To store and manage the adorner's visual children.
    /// </summary>
    private readonly VisualCollection _visualCollection;

    /// <summary>
    /// DPI X for screen.
    /// </summary>
    private static readonly double _dpiX;

    /// <summary>
    /// DPI Y for screen.
    /// </summary>
    private static readonly double _dpiY;

    public static readonly DependencyProperty FillProperty = Shape.FillProperty.AddOwner(typeof(CroppingAdorner));
    
    public static readonly DependencyProperty ShowCornersProperty = 
        DependencyProperty.Register(nameof(ShowCorners), typeof(bool), typeof(CroppingAdorner), new FrameworkPropertyMetadata(default(bool), (d, e) => ((CroppingAdorner)d).OnShowCornersChanged(e)));
   
    public static readonly RoutedEvent CropChangedEvent = 
        EventManager.RegisterRoutedEvent(nameof(CropChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CroppingAdorner));

    #endregion

    #region Properties

    protected override int VisualChildrenCount => _visualCollection.Count;

    public Rect ClippingRectangle
    {
        get => _puncturedRect.RectInterior;
        set
        {
            _puncturedRect.RectInterior = value;
            SetThumbsPosition(value);
        }
    }

    public Brush Fill
    {
        get => (Brush)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public bool ShowCorners
    {
        get => (bool)GetValue(ShowCornersProperty);
        set => SetValue(ShowCornersProperty, value);
    }

    #endregion

    #region Routed Events

    public event RoutedEventHandler CropChanged
    {
        add => AddHandler(CropChangedEvent, value);
        remove => RemoveHandler(CropChangedEvent, value);
    }

    #endregion

    static CroppingAdorner()
    {
        var color = Colors.Red;
        var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);

        _dpiX = graphics.DpiX;
        _dpiY = graphics.DpiY;
        color.A = 80;

        FillProperty.OverrideMetadata(typeof(CroppingAdorner), new PropertyMetadata(new SolidColorBrush(color), (d, e) => ((CroppingAdorner)d).OnFillChanged(e)));
    }

    public CroppingAdorner(UIElement adornedElement, Rect rcInit) : base(adornedElement)
    {
        _visualCollection = new(this);
        _puncturedRect = new()
        {
            IsHitTestVisible = false,
            RectInterior = rcInit,
            Fill = Fill
        };
        _visualCollection.Add(_puncturedRect);
        _canvasThumbs = new()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        _visualCollection.Add(_canvasThumbs);
        BuildCorner(out _cropThumbTop, Cursors.SizeNS);
        BuildCorner(out _cropThumbBottom, Cursors.SizeNS);
        BuildCorner(out _cropThumbLeft, Cursors.SizeWE);
        BuildCorner(out _cropThumbRight, Cursors.SizeWE);
        BuildCorner(out _cropThumbTopLeft, Cursors.SizeNWSE);
        BuildCorner(out _cropThumbTopRight, Cursors.SizeNESW);
        BuildCorner(out _cropThumbBottomLeft, Cursors.SizeNESW);
        BuildCorner(out _cropThumbBottomRight, Cursors.SizeNWSE);

        _cropThumbBottomLeft.DragDelta += new DragDeltaEventHandler(HandleBottomLeft);
        _cropThumbBottomRight.DragDelta += new DragDeltaEventHandler(HandleBottomRight);
        _cropThumbTopLeft.DragDelta += new DragDeltaEventHandler(HandleTopLeft);
        _cropThumbTopRight.DragDelta += new DragDeltaEventHandler(HandleTopRight);
        _cropThumbTop.DragDelta += new DragDeltaEventHandler(HandleTop);
        _cropThumbBottom.DragDelta += new DragDeltaEventHandler(HandleBottom);
        _cropThumbRight.DragDelta += new DragDeltaEventHandler(HandleRight);
        _cropThumbLeft.DragDelta += new DragDeltaEventHandler(HandleLeft);

        if (adornedElement is FrameworkElement frameworkElement)
            frameworkElement.SizeChanged += new SizeChangedEventHandler(AdornedElementSizeChanged);
    }

    #region Overrides

    protected override Visual GetVisualChild(int index) => _visualCollection[index];

    /// <summary>
    /// Arrange the Adorners.
    /// </summary>
    /// <param name="finalSize"></param>
    /// <returns></returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var rcExterior = new Rect(0, 0, AdornedElement.RenderSize.Width, AdornedElement.RenderSize.Height);
        _puncturedRect.RectExterior = rcExterior;
        var rcInterior = _puncturedRect.RectInterior;
        _puncturedRect.Arrange(rcExterior);

        SetThumbsPosition(rcInterior);
        _canvasThumbs.Arrange(rcExterior);
        return finalSize;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Generic handler for Cropping.
    /// </summary>
    /// <param name="drcL"></param>
    /// <param name="drcT"></param>
    /// <param name="drcW"></param>
    /// <param name="drcH"></param>
    /// <param name="dx"></param>
    /// <param name="dy"></param>
    private void HandleThumb(double drcL, double drcT, double drcW, double drcH, double dx, double dy)
    {
        var rcInterior = _puncturedRect.RectInterior;

        if (rcInterior.Width + drcW * dx < 0)
            dx = -rcInterior.Width / drcW;

        if (rcInterior.Height + drcH * dy < 0)
            dy = -rcInterior.Height / drcH;

        rcInterior = new Rect(
            rcInterior.Left + drcL * dx,
            rcInterior.Top + drcT * dy,
            rcInterior.Width + drcW * dx,
            rcInterior.Height + drcH * dy);

        _puncturedRect.RectInterior = rcInterior;
        SetThumbsPosition(_puncturedRect.RectInterior);
        RaiseEvent(new RoutedEventArgs(CropChangedEvent, this));
    }

    /// <summary>
    /// Handler for Cropping from the bottom-left.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void HandleBottomLeft(object sender, DragDeltaEventArgs args)
    {
        if (sender is CropThumb)
        {
            HandleThumb(
                1, 0, -1, 1,
                args.HorizontalChange,
                args.VerticalChange);
        }
    }

    /// <summary>
    /// Handler for Cropping from the bottom-right.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void HandleBottomRight(object sender, DragDeltaEventArgs args)
    {
        if (sender is CropThumb)
        {
            HandleThumb(
                0, 0, 1, 1,
                args.HorizontalChange,
                args.VerticalChange);
        }
    }

    /// <summary>
    /// Handler for Cropping from the top-right.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void HandleTopRight(object sender, DragDeltaEventArgs args)
    {
        if (sender is CropThumb)
        {
            HandleThumb(
                0, 1, 1, -1,
                args.HorizontalChange,
                args.VerticalChange);
        }
    }

    /// <summary>
    /// Handler for Cropping from the top-left.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void HandleTopLeft(object sender, DragDeltaEventArgs args)
    {
        if (sender is CropThumb)
        {
            HandleThumb(
                1, 1, -1, -1,
                args.HorizontalChange,
                args.VerticalChange);
        }
    }

    /// <summary>
    /// Handler for Cropping from the top.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void HandleTop(object sender, DragDeltaEventArgs args)
    {
        if (sender is CropThumb)
        {
            HandleThumb(
                0, 1, 0, -1,
                args.HorizontalChange,
                args.VerticalChange);
        }
    }

    /// <summary>
    /// Handler for Cropping from the left.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void HandleLeft(object sender, DragDeltaEventArgs args)
    {
        if (sender is CropThumb)
        {
            HandleThumb(
                1, 0, -1, 0,
                args.HorizontalChange,
                args.VerticalChange);
        }
    }

    /// <summary>
    /// Handler for Cropping from the right.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void HandleRight(object sender, DragDeltaEventArgs args)
    {
        if (sender is CropThumb)
        {
            HandleThumb(
                0, 0, 1, 0,
                args.HorizontalChange,
                args.VerticalChange);
        }
    }

    /// <summary>
    /// Handler for Cropping from the bottom.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void HandleBottom(object sender, DragDeltaEventArgs args)
    {
        if (sender is CropThumb)
        {
            HandleThumb(
                0, 0, 0, 1,
                args.HorizontalChange,
                args.VerticalChange);
        }
    }

    private void AdornedElementSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var fixupRequired = false;
        var element = (FrameworkElement)sender;
        var rectInterior = _puncturedRect.RectInterior;
        var interiorLeft = rectInterior.Left;
        var interiorTop = rectInterior.Top;
        var interiorWidth = rectInterior.Width;
        var interiorHeight = rectInterior.Height;

        if (rectInterior.Left > element.RenderSize.Width)
        {
            interiorLeft = element.RenderSize.Width;
            interiorWidth = 0;
            fixupRequired = true;
        }

        if (rectInterior.Top > element.RenderSize.Height)
        {
            interiorTop = element.RenderSize.Height;
            interiorHeight = 0;
            fixupRequired = true;
        }

        if (rectInterior.Right > element.RenderSize.Width)
        {
            interiorWidth = Math.Max(0, element.RenderSize.Width - interiorLeft);
            fixupRequired = true;
        }

        if (rectInterior.Bottom > element.RenderSize.Height)
        {
            interiorHeight = Math.Max(0, element.RenderSize.Height - interiorTop);
            fixupRequired = true;
        }

        if (fixupRequired)
            _puncturedRect.RectInterior = new Rect(interiorLeft, interiorTop, interiorWidth, interiorHeight);
    }

    private void SetThumbsPosition(Rect rc)
    {
        _cropThumbBottomRight.SetPosition(rc.Right, rc.Bottom);
        _cropThumbTopLeft.SetPosition(rc.Left, rc.Top);
        _cropThumbTopRight.SetPosition(rc.Right, rc.Top);
        _cropThumbBottomLeft.SetPosition(rc.Left, rc.Bottom);
        _cropThumbTop.SetPosition(rc.Left + rc.Width / 2, rc.Top);
        _cropThumbBottom.SetPosition(rc.Left + rc.Width / 2, rc.Bottom);
        _cropThumbLeft.SetPosition(rc.Left, rc.Top + rc.Height / 2);
        _cropThumbRight.SetPosition(rc.Right, rc.Top + rc.Height / 2);
    }

    public BitmapSource? GetAsBitmapSource()
    {
        var margin = GetAdornerMargin();
        var rectInterior = _puncturedRect.RectInterior;
        var pxFromSize = UnitsToPx(rectInterior.Width, rectInterior.Height);
        var pxFromPosition = UnitsToPx(rectInterior.Left + margin.Left, rectInterior.Top + margin.Top);
        var pxWhole = UnitsToPx(AdornedElement.RenderSize.Width + margin.Left, AdornedElement.RenderSize.Height + margin.Left);

        pxFromSize.X = Math.Max(Math.Min(pxWhole.X - pxFromPosition.X, pxFromSize.X), 0);
        pxFromSize.Y = Math.Max(Math.Min(pxWhole.Y - pxFromPosition.Y, pxFromSize.Y), 0);

        if (pxFromSize.X == 0 || pxFromSize.Y == 0)
            return null;

        var int32Rect = new Int32Rect(pxFromPosition.X, pxFromPosition.Y, pxFromSize.X, pxFromSize.Y);
        var renderTargetBitmap = new RenderTargetBitmap(pxWhole.X, pxWhole.Y, _dpiX, _dpiY, PixelFormats.Default);

        renderTargetBitmap.Render(AdornedElement);

        return new CroppedBitmap(renderTargetBitmap, int32Rect);
    }

    private void BuildCorner(out CropThumb cropThumb, Cursor crs)
    {
        cropThumb = new CropThumb()
        {
            Cursor = crs,
            Visibility = ShowCorners ? Visibility.Visible : Visibility.Collapsed,
        };

        _canvasThumbs.Children.Add(cropThumb);
    }

    private void OnShowCornersChanged(DependencyPropertyChangedEventArgs e)
    {
        var visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        _cropThumbTop.Visibility = visibility;
        _cropThumbBottom.Visibility = visibility;
        _cropThumbLeft.Visibility = visibility;
        _cropThumbRight.Visibility = visibility;
        _cropThumbTopLeft.Visibility = visibility;
        _cropThumbTopRight.Visibility = visibility;
        _cropThumbBottomLeft.Visibility = visibility;
        _cropThumbBottomRight.Visibility = visibility;
    }

    private void OnFillChanged(DependencyPropertyChangedEventArgs e) => _puncturedRect.Fill = (Brush)e.NewValue;

    private Thickness GetAdornerMargin()
    {
        var thickness = new Thickness(0);
        if (AdornedElement is FrameworkElement element)
            thickness = element.Margin;
        return thickness;
    }

    private static Point UnitsToPx(double x, double y) => new Point((int)(x * _dpiX / 96), (int)(y * _dpiY / 96));

    #endregion
}