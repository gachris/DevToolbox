using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using DevToolbox.Wpf.Extensions;
using DevToolbox.Wpf.Interop;
using DevToolbox.Wpf.Utils;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A control that provides an eye-dropper tool for capturing colors from the screen.
/// </summary>
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
[TemplatePart(Name = "PART_ZoomAndPanControl", Type = typeof(ZoomAndPanControl))]
public class EyeDropper : ContentControl
{
    #region Fields/Consts

    private const int ViewportZoom = 10;

    private Window? _virtualScreenFrame;
    private Popup? _popup;
    private Image? _image;
    private Canvas? _canvas;
    private ZoomAndPanControl? _zoomAndPanControl;
    private System.Drawing.Point _relativeCursorPosition;
    private System.Drawing.Point _cursorPosition;
    private System.Drawing.Bitmap? _lastBitmap;

    /// <summary>
    /// Raised when the capture state changes (Started, Continued, Finished).
    /// </summary>
    public event EventHandler<CaptureEventArgs>? CaptureChanged;

    /// <summary>
    /// Raised when the detected color changes during the color-picking process.
    /// </summary>
    public event EventHandler<ColorChangedEventArgs>? ColorChanged;

    /// <summary>
    /// Dependency property for the background of the popup.
    /// </summary>
    public static readonly DependencyProperty PopupBackgroundProperty =
        DependencyProperty.Register(nameof(PopupBackground), typeof(Brush), typeof(EyeDropper), new FrameworkPropertyMetadata(Brushes.White));

    /// <summary>
    /// Dependency property for the border brush of the popup.
    /// </summary>
    public static readonly DependencyProperty PopupBorderBrushProperty =
        DependencyProperty.Register(nameof(PopupBorderBrush), typeof(Brush), typeof(EyeDropper), new FrameworkPropertyMetadata(Brushes.Black));

    /// <summary>
    /// Dependency property for the foreground brush of the popup.
    /// </summary>
    public static readonly DependencyProperty PopupForegroundProperty =
        DependencyProperty.Register(nameof(PopupForeground), typeof(Brush), typeof(EyeDropper), new FrameworkPropertyMetadata(Brushes.Black));

    /// <summary>
    /// Dependency property that holds the current selected color as a Brush.
    /// </summary>
    private static readonly DependencyPropertyKey ColorPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(Color), typeof(Brush), typeof(EyeDropper), new FrameworkPropertyMetadata(default, OnColorChanged));

    /// <summary>
    /// Dependency property that holds the formatted color string based on the selected color format.
    /// </summary>
    private static readonly DependencyPropertyKey FormattedColorPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(FormattedColor), typeof(string), typeof(EyeDropper), new FrameworkPropertyMetadata(default));

    /// <summary>
    /// Dependency property that holds the string format for the color string.
    /// </summary>
    public static readonly DependencyProperty ColorFormatTemplateProperty =
        DependencyProperty.Register(nameof(ColorFormatTemplate), typeof(string), typeof(EyeDropper), new FrameworkPropertyMetadata(default(string)));
   
    /// <summary>
    /// Dependency property that holds the string format precision for the color string (min: 1, max: 11).
    /// </summary>
    public static readonly DependencyProperty ColorFormatPrecisionProperty =
        DependencyProperty.Register(
            nameof(ColorFormatPrecision),
            typeof(int),
            typeof(EyeDropper),
            new FrameworkPropertyMetadata(11, FrameworkPropertyMetadataOptions.None),
            ValidatePrecision);

    /// <summary>
    /// Dependency property that indicates whether the eye-dropper is actively capturing colors.
    /// </summary>
    private static readonly DependencyPropertyKey IsCapturingPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsCapturing), typeof(bool), typeof(EyeDropper), new FrameworkPropertyMetadata(false));

    /// <summary>
    /// Dependency property that indicates whether the selected color should automatically be copied to the clipboard.
    /// </summary>
    public static readonly DependencyProperty CopyColorToClipboardProperty =
        DependencyProperty.Register(nameof(CopyColorToClipboard), typeof(bool), typeof(EyeDropper), new FrameworkPropertyMetadata(true));

    /// <summary>
    /// Dependency property that defines the format in which the selected color should be displayed (e.g., Hex, RGB, HSV).
    /// </summary>
    public static readonly DependencyProperty ColorFormatProperty =
        DependencyProperty.Register(nameof(ColorFormat), typeof(ColorFormat), typeof(EyeDropper), new FrameworkPropertyMetadata(ColorFormat.HTML));

    /// <summary>
    /// The dependency property corresponding to the readonly Color property.
    /// </summary>
    public static readonly DependencyProperty ColorProperty = ColorPropertyKey.DependencyProperty;

    /// <summary>
    /// The dependency property corresponding to the readonly FormattedColor property.
    /// </summary>
    public static readonly DependencyProperty FormattedColorProperty = FormattedColorPropertyKey.DependencyProperty;

    /// <summary>
    /// The dependency property corresponding to the readonly IsCapturing property.
    /// </summary>
    public static readonly DependencyProperty IsCapturingProperty = IsCapturingPropertyKey.DependencyProperty;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the current position of the cursor in absolute screen coordinates.
    /// </summary>
    public System.Drawing.Point CursorPosition => _cursorPosition;

    /// <summary>
    /// Gets the position of the cursor relative to the captured screen region.
    /// </summary>
    public System.Drawing.Point RelativeCursorPosition => _relativeCursorPosition;

    /// <summary>
    /// Gets or sets the popup background of the EyeDropper.
    /// </summary>
    public Brush PopupBackground
    {
        get => (Brush)GetValue(PopupBackgroundProperty);
        set => SetValue(PopupBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the popup border brush of the EyeDropper.
    /// </summary>
    public Brush PopupBorderBrush
    {
        get => (Brush)GetValue(PopupBorderBrushProperty);
        set => SetValue(PopupBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the popup foreground of the EyeDropper.
    /// </summary>
    public Brush PopupForeground
    {
        get => (Brush)GetValue(PopupForegroundProperty);
        set => SetValue(PopupForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the color that is being captured by the EyeDropper.
    /// </summary>
    public Brush Color
    {
        get => (Brush)GetValue(ColorProperty);
        private set => SetValue(ColorPropertyKey, value);
    }

    /// <summary>
    /// Gets or sets the color formatted string (e.g., RGB, Hex, etc.) based on the selected color format.
    /// </summary>
    public string FormattedColor
    {
        get => (string)GetValue(FormattedColorProperty);
        private set => SetValue(FormattedColorPropertyKey, value);
    }

    /// <summary>
    /// Gets or sets the string format used to format the color string.
    /// </summary>
    public string ColorFormatTemplate
    {
        get => (string)GetValue(ColorFormatTemplateProperty);
        set => SetValue(ColorFormatTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the precision used to format the color string.
    /// </summary>
    public int ColorFormatPrecision
    {
        get => (int)GetValue(ColorFormatPrecisionProperty);
        set => SetValue(ColorFormatPrecisionProperty, value);
    }
    
    /// <summary>
    /// Gets a value indicating whether the EyeDropper is currently capturing.
    /// </summary>
    public bool IsCapturing
    {
        get => (bool)GetValue(IsCapturingProperty);
        private set => SetValue(IsCapturingPropertyKey, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the captured color should be copied to the clipboard automatically.
    /// </summary>
    public bool CopyColorToClipboard
    {
        get => (bool)GetValue(CopyColorToClipboardProperty);
        set => SetValue(CopyColorToClipboardProperty, value);
    }

    /// <summary>
    /// Gets or sets the color format used to display the color (e.g., RGB, Hex, HSL).
    /// </summary>
    public ColorFormat ColorFormat
    {
        get => (ColorFormat)GetValue(ColorFormatProperty);
        set => SetValue(ColorFormatProperty, value);
    }

    #endregion

    static EyeDropper()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(EyeDropper), new FrameworkPropertyMetadata(typeof(EyeDropper)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EyeDropper"/> class.
    /// </summary>
    public EyeDropper()
    {
    }

    #region Methods Override

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _image = new()
        {
            SnapsToDevicePixels = true
        };

        RenderOptions.SetBitmapScalingMode(_image, BitmapScalingMode.NearestNeighbor);
        RenderOptions.SetEdgeMode(_image, EdgeMode.Aliased);

        _canvas = new();
        _canvas.Children.Add(_image);

        if (Template.FindName("PART_Popup", this) is Popup popup)
        {
            _popup = popup;
            _popup.Placement = PlacementMode.Absolute;
        }

        if (Template.FindName("PART_ZoomAndPanControl", this) is ZoomAndPanControl zoomAndPanControl)
        {
            _zoomAndPanControl = zoomAndPanControl;
            _zoomAndPanControl.Content = _canvas;
            _zoomAndPanControl.ViewportZoom = ViewportZoom;
        }

        PreviewMouseDown += OnPreviewMouseDown;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Starts the color capture process.
    /// </summary>
    public void Capture()
    {
        if (IsCapturing)
        {
            return;
        }

        IsCapturing = true;

        CaptureScreen();

        _virtualScreenFrame = EyeDropperHelper.CreateVirtualScreenFrame();
        _virtualScreenFrame.Cursor = Cursor;
        _virtualScreenFrame.MouseMove += VirtualScreenFrame_MouseMove;
        _virtualScreenFrame.MouseUp += VirtualScreenFrame_MouseUp;
        _virtualScreenFrame.Show();

        CaptureChanged?.Invoke(this, new CaptureEventArgs(CaptureState.Started));

        CaptureNextColor();
    }

    /// <summary>
    /// Ends the color capture process.
    /// </summary>
    public void ReleaseCapture()
    {
        if (!IsCapturing)
        {
            return;
        }

        _lastBitmap?.Dispose();
        _lastBitmap = null;

        if (_virtualScreenFrame != null)
        {
            _virtualScreenFrame.MouseMove -= VirtualScreenFrame_MouseMove;
            _virtualScreenFrame.MouseUp -= VirtualScreenFrame_MouseUp;
            _virtualScreenFrame.Close();
            _virtualScreenFrame = null;
        }

        if (_popup != null)
        {
            _popup.IsOpen = false;
        }

        if (CopyColorToClipboard)
        {
            Clipboard.SetText(FormattedColor);
        }

        IsCapturing = false;

        CaptureChanged?.Invoke(this, new CaptureEventArgs(CaptureState.Finished));
    }

    private void CaptureScreen()
    {
        _lastBitmap = EyeDropperHelper.CaptureVirtualScreen();

        if (_lastBitmap is null || _image is null || _canvas is null)
        {
            if (_image != null)
            {
                _image.Source = null;
            }

            return;
        }

        _image.Source = _lastBitmap.ToImageSource();
        _image.Width = _lastBitmap.Width;
        _image.Height = _lastBitmap.Height;

        if (_zoomAndPanControl != null)
        {
            _canvas.Width = _lastBitmap.Width + _zoomAndPanControl.Width / ViewportZoom;
            _canvas.Height = _lastBitmap.Height + _zoomAndPanControl.Height / ViewportZoom;
        }

        var left = (_canvas.Width - _image.Width) / 2;
        Canvas.SetLeft(_image, left);

        var top = (_canvas.Height - _image.Height) / 2;
        Canvas.SetTop(_image, top);
    }

    private void CaptureNextColor()
    {
        _cursorPosition = NativeMethods.GetCursorPosition();
        _relativeCursorPosition = EyeDropperHelper.GetRelativeCoordinatesToVirtualScreen(_cursorPosition);

        if (_lastBitmap is not null)
        {
            var color = _lastBitmap.GetPixel(_relativeCursorPosition.X, _relativeCursorPosition.Y).ToMediaColor();
            Color = new SolidColorBrush(color);
        }

        UpdateView();

        CaptureChanged?.Invoke(this, new CaptureEventArgs(CaptureState.Continued));
    }

    private void UpdateView()
    {
        if (!IsCapturing || _popup is null || _zoomAndPanControl is null)
        {
            return;
        }

        if (!_popup.IsOpen)
        {
            _popup.IsOpen = true;
        }

        _popup.HorizontalOffset = CursorPosition.X + _zoomAndPanControl.Width / 2;
        _popup.VerticalOffset = CursorPosition.Y - _zoomAndPanControl.Height / 2;

        _zoomAndPanControl.ContentOffsetX = RelativeCursorPosition.X;
        _zoomAndPanControl.ContentOffsetY = RelativeCursorPosition.Y;
    }

    private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var eyeDropper = (EyeDropper)d;
        eyeDropper.OnColorChanged((Brush)e.OldValue, (Brush)e.NewValue);
    }

    /// <summary>
    /// Validation callback for ColorFormatPrecisionProperty to ensure the value is between 1 and 11.
    /// </summary>
    private static bool ValidatePrecision(object value)
    {
        if (value is int precision)
        {
            return precision is >= 1 and <= 11;
        }
        return false;
    }

    private void OnColorChanged(Brush oldValue, Brush newValue)
    {
        var brush = (SolidColorBrush)newValue;
        FormattedColor = EyeDropperHelper.GetFormattedColor(brush, ColorFormat, ColorFormatTemplate, ColorFormatPrecision);
        ColorChanged?.Invoke(this, new ColorChangedEventArgs(oldValue, newValue));
    }

    #endregion

    #region Events Subscriptions

    private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        Capture();
    }

    private void VirtualScreenFrame_MouseMove(object sender, MouseEventArgs e)
    {
        if (!IsCapturing)
        {
            return;
        }

        CaptureNextColor();
    }

    private void VirtualScreenFrame_MouseUp(object sender, MouseButtonEventArgs e)
    {
        ReleaseCapture();
    }

    #endregion
}
