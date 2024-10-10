using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ColorSpace.Net;
using ColorSpace.Net.Componentes;
using PointEx = System.Drawing.Point;

namespace DevToolbox.Wpf.Controls;

[TemplatePart(Name = "PART_ColorPlaneControl", Type = typeof(ContentControl))]
[TemplatePart(Name = "PART_NormalSlider", Type = typeof(Slider))]
[TemplatePart(Name = "PART_AlphaSlider", Type = typeof(Slider))]
public partial class ColorPicker : Control
{
    private enum ColorChangeSource
    {
        MouseDown,
        Normal,
        SelectionChanging
    }

    #region Fields/Consts

    private INormalComponent _normalComponent = default!;
    private ColorChangeSource? _colorChangeSource;
    private int _lastNormal = -1;
    private string _lastComponentName = string.Empty;
    private ContentControl? _colorCanvas;
    private Slider? _normalSlider;
    private Slider? _alphaSlider;
    private Ellipse? _colorPlaneEllipse;
    private ContentPresenter? _colorComponentsWrapper;

    private ColorComponents? _colorComponents;
    private PointEx _selectionPoint;
    private readonly Dictionary<NormalComponentType, INormalComponent> Components = [];

    public event EventHandler? AlphaChanged;
    public event EventHandler? SelectedColorChanged;

    public static readonly DependencyProperty AlphaProperty =
        DependencyProperty.Register(nameof(Alpha), typeof(byte), typeof(ColorPicker), new FrameworkPropertyMetadata((byte)255, OnAlphaChanged, AlphaCoerceValue));

    public static readonly DependencyProperty ColorPickerStyleProperty =
        DependencyProperty.Register(nameof(ColorPickerStyle), typeof(ColorPickerStyle), typeof(ColorPicker), new FrameworkPropertyMetadata(default(ColorPickerStyle)));

    private static readonly DependencyPropertyKey MinNormalPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(MinNormal), typeof(int), typeof(ColorPicker), new FrameworkPropertyMetadata(default));

    private static readonly DependencyPropertyKey MaxNormalPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(MaxNormal), typeof(int), typeof(ColorPicker), new FrameworkPropertyMetadata(default));

    public static readonly DependencyProperty NormalComponentTypeProperty =
        DependencyProperty.Register(nameof(NormalComponentType), typeof(NormalComponentType), typeof(ColorPicker), new FrameworkPropertyMetadata(default(NormalComponentType), OnNormalComponentTypeChanged));

    public static readonly DependencyProperty NormalProperty =
        DependencyProperty.Register(nameof(Normal), typeof(int), typeof(ColorPicker), new FrameworkPropertyMetadata(0, OnNormalChanged, NormalCoerceValue));

    public static readonly DependencyProperty SelectedColorProperty =
        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorPicker), new FrameworkPropertyMetadata(Colors.Black, OnSelectedColorChanged));

    public static readonly DependencyProperty MinNormalProperty = MinNormalPropertyKey.DependencyProperty;
    public static readonly DependencyProperty MaxNormalProperty = MaxNormalPropertyKey.DependencyProperty;

    #endregion

    #region Properties

    public byte Alpha
    {
        get => (byte)GetValue(AlphaProperty);
        set => SetValue(AlphaProperty, value);
    }

    public ColorPickerStyle ColorPickerStyle
    {
        get => (ColorPickerStyle)GetValue(ColorPickerStyleProperty);
        set => SetValue(ColorPickerStyleProperty, value);
    }

    private bool IsAlphaEnabled => ColorPickerStyle is ColorPickerStyle.StandardWithAlpha or ColorPickerStyle.FullWithAlpha;

    public Color InitialColor { get; set; }

    public int MinNormal => (int)GetValue(MinNormalProperty);

    public int MaxNormal => (int)GetValue(MaxNormalProperty);

    public NormalComponentType NormalComponentType
    {
        get => (NormalComponentType)GetValue(NormalComponentTypeProperty);
        set => SetValue(NormalComponentTypeProperty, value);
    }

    public int Normal
    {
        get => (int)GetValue(NormalProperty);
        set => SetValue(NormalProperty, value);
    }

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    private PointEx SelectionPoint
    {
        get => _selectionPoint;
        set
        {
            _selectionPoint = value;
            UpdateEllipsePosition();
        }
    }

    #endregion

    static ColorPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
    }

    public ColorPicker()
    {
    }

    #region Methods Override

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _colorPlaneEllipse = Template.FindName("PART_ColorPlaneEllipse", this) as Ellipse;

        if (_colorCanvas is not null)
        {
            _colorCanvas.MouseMove -= ColorCanvas_MouseMove;
            _colorCanvas.MouseDown -= ColorCanvas_MouseDown;
            _colorCanvas.MouseUp -= ColorCanvas_MouseUp;
        }

        _colorCanvas = Template.FindName("PART_ColorCanvas", this) as ContentControl;

        if (_colorCanvas is not null)
        {
            _colorCanvas.MouseMove += ColorCanvas_MouseMove;
            _colorCanvas.MouseDown += ColorCanvas_MouseDown;
            _colorCanvas.MouseUp += ColorCanvas_MouseUp;
        }

        if (_colorComponents is not null)
        {
            _colorComponents.ColorChanged -= ColorComponents_ColorChanged;
        }

        _colorComponents = null;
        _colorComponentsWrapper = Template.FindName("PART_ColorComponents", this) as ContentPresenter;

        if (_colorComponentsWrapper is not null)
        {
            _colorComponents = new ColorComponents();
            _colorComponents.ColorChanged += ColorComponents_ColorChanged;

            _colorComponentsWrapper.Content = _colorComponents;

            SetBinding(NormalComponentTypeProperty, new Binding(nameof(ColorComponents.NormalComponentType))
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Source = _colorComponents
            });
        }

        _normalSlider = Template.FindName("PART_NormalSlider", this) as Slider;
        _alphaSlider = Template.FindName("PART_AlphaSlider", this) as Slider;

        OnNormalComponentTypeChanged(NormalComponentType, NormalComponentType);
    }

    #endregion

    #region Methods

    private void UpdateColorCanvasBitmap(int normal)
    {
        if (_colorCanvas is null)
            return;

        if (_lastNormal != normal || _lastComponentName != _normalComponent.Name)
        {
            var width = 256;
            var height = 256;
            var bytesPerPixel = 3;
            var stride = width * bytesPerPixel;
            var pixels = _normalComponent.GenerateNormalMapFromValue(normal, width, height, stride);

            _colorCanvas.Background = new ImageBrush
            {
                ImageSource = ColorPickerHelper.CreateWriteableBitmap(pixels, width, height, stride, PixelFormats.Bgr24)
            };

            _lastNormal = normal;
            _lastComponentName = _normalComponent.Name;
        }
    }

    private void UpdateSliderNormalBitmap(System.Drawing.Color color)
    {
        if (_normalSlider is null)
            return;

        var width = 18;
        var height = 256;
        var bytesPerPixel = 3;
        var stride = width * bytesPerPixel;
        var pixels = _normalComponent.GenerateNormalMapFromColor(color, width, height, stride);

        _normalSlider.Background = new ImageBrush
        {
            ImageSource = ColorPickerHelper.CreateWriteableBitmap(pixels, width, height, stride, PixelFormats.Bgr24)
        };
    }

    private void UpdateSliderAlphaBitmap(System.Drawing.Color color)
    {
        if (_alphaSlider is null)
            return;

        var width = 18;
        var height = 256;
        var bytesPerPixel = 4;
        var stride = width * bytesPerPixel;
        var pixels = _normalComponent.GenerateNormalMapWithAlphaChannel(color, width, height, stride);

        _alphaSlider.Background = new ImageBrush
        {
            ImageSource = ColorPickerHelper.CreateWriteableBitmap(pixels, width, height, stride, PixelFormats.Bgra32)
        };
    }

    private void UpdateEllipsePosition()
    {
        if (_colorPlaneEllipse is null)
            return;

        var translateTransform = new TranslateTransform();
        _colorPlaneEllipse.RenderTransform = translateTransform;

        var width = 256;
        var height = 256;

        translateTransform.X = SelectionPoint.X - width / 2.0;
        translateTransform.Y = SelectionPoint.Y - height / 2.0;
    }

    private void UpdateFromPoint(PointEx point)
    {
        SelectionPoint = CoercePoint(point);

        var drawColor = _normalComponent.ColorAtPoint(SelectionPoint, Normal);
        SelectedColor = ColorPickerHelper.ConvertToMediaColor(drawColor, Alpha);

        if (!_normalComponent.IsNormalIndependantOfColor)
        {
            UpdateSliderNormalBitmap(drawColor);
        }
    }

    private void OnAlphaChanged(byte oldValue, byte newValue)
    {
        SelectedColor = Color.FromArgb(newValue, SelectedColor.R, SelectedColor.G, SelectedColor.B);
        AlphaChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnNormalChanged(int oldValue, int newValue)
    {
        _colorChangeSource = ColorChangeSource.Normal;

        _colorComponents?.SetNormal(newValue);
        SelectedColor = ColorPickerHelper.ConvertToMediaColor(_normalComponent.ColorAtPoint(SelectionPoint, newValue), Alpha);

        UpdateColorCanvasBitmap(newValue);

        _colorChangeSource = null;
    }

    private void OnNormalComponentTypeChanged(NormalComponentType oldValue, NormalComponentType newValue)
    {
        var drawColor = ColorPickerHelper.ConvertToDrawingColor(SelectedColor);

        EnsureComponent(newValue);
        _normalComponent = Components[newValue];

        SetValue(MinNormalPropertyKey, _normalComponent.MinValue);
        SetValue(MaxNormalPropertyKey, _normalComponent.MaxValue);

        SelectionPoint = _normalComponent.PointFromColor(drawColor);

        Normal = _colorComponents?.GetNormal() ?? -1;

        UpdateColorCanvasBitmap(Normal);
        UpdateSliderNormalBitmap(drawColor);
        UpdateSliderAlphaBitmap(drawColor);

        void EnsureComponent(NormalComponentType normalComponentType)
        {
            if (!Components.ContainsKey(normalComponentType))
            {
                Components[normalComponentType] = NormalComponentFactory.CreateNormalComponent(normalComponentType);
            }
        }
    }

    private void OnSelectedColorChanged(Color oldValue, Color newValue)
    {
        var drawColor = ColorPickerHelper.ConvertToDrawingColor(newValue);

        Alpha = drawColor.A;
        UpdateSliderAlphaBitmap(drawColor);

        if (_colorChangeSource is not ColorChangeSource.Normal)
        {
            _colorComponents?.UpdateFromColor(newValue);
        }

        if (_colorChangeSource is null)
        {
            SelectionPoint = _normalComponent.PointFromColor(drawColor);
            Normal = _colorComponents?.GetNormal() ?? -1;
            UpdateColorCanvasBitmap(Normal);

            if (!_normalComponent.IsNormalIndependantOfColor)
            {
                UpdateSliderNormalBitmap(drawColor);
            }
        }

        SelectedColorChanged?.Invoke(this, EventArgs.Empty);
    }

    private object AlphaCoerceValue(object baseValue)
    {
        return IsAlphaEnabled ? Math.Min(Math.Max((byte)baseValue, (byte)0), (byte)255) : (byte)255;
    }

    private object NormalCoerceValue(object baseValue)
    {
        return Math.Min(Math.Max((int)baseValue, MinNormal), MaxNormal);
    }

    private static void OnAlphaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var colorPicker = (ColorPicker)d;
        colorPicker.OnAlphaChanged((byte)e.OldValue, (byte)e.NewValue);
    }

    private static void OnNormalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var colorPicker = (ColorPicker)d;
        colorPicker.OnNormalChanged((int)e.OldValue, (int)e.NewValue);
    }

    private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var colorPicker = (ColorPicker)d;
        colorPicker.OnSelectedColorChanged((Color)e.OldValue, (Color)e.NewValue);
    }

    private static void OnNormalComponentTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var colorPicker = (ColorPicker)d;
        colorPicker.OnNormalComponentTypeChanged((NormalComponentType)e.OldValue, (NormalComponentType)e.NewValue);
    }

    private static object AlphaCoerceValue(DependencyObject d, object baseValue)
    {
        var colorPicker = (ColorPicker)d;
        return colorPicker.AlphaCoerceValue(baseValue);
    }

    private static object NormalCoerceValue(DependencyObject d, object baseValue)
    {
        var colorPicker = (ColorPicker)d;
        return colorPicker.NormalCoerceValue(baseValue);
    }

    private static PointEx CoercePoint(PointEx source, PointEx maximum = default, PointEx minimum = default)
    {
        maximum = maximum == default ? new PointEx(255, 255) : maximum;
        minimum = minimum == default ? new PointEx(0, 0) : minimum;

        var x = source.X;
        var y = source.Y;

        x = x > maximum.X ? maximum.X : x < minimum.X ? minimum.X : x;
        y = y > maximum.Y ? maximum.Y : y < minimum.Y ? minimum.Y : y;

        return new PointEx(x, y);
    }

    #endregion

    #region Events Subscriptions

    private void ColorCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_colorCanvas is null)
            return;

        _colorChangeSource = ColorChangeSource.MouseDown;

        _colorCanvas.CaptureMouse();

        var point = e.GetPosition(_colorCanvas);
        UpdateFromPoint(ColorPickerHelper.ConvertToDrawingPoint(point));

        _colorChangeSource = null;
    }

    private void ColorCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_colorCanvas is null || Mouse.LeftButton is not MouseButtonState.Pressed)
            return;

        _colorChangeSource = ColorChangeSource.MouseDown;

        var point = e.GetPosition(_colorCanvas);
        UpdateFromPoint(ColorPickerHelper.ConvertToDrawingPoint(point));

        _colorChangeSource = null;
    }

    private void ColorCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_colorCanvas is null || Mouse.LeftButton is not MouseButtonState.Released)
            return;

        _colorCanvas.ReleaseMouseCapture();
    }

    private void ColorComponents_ColorChanged(object? sender, EventArgs e)
    {
        if (_colorComponents is null || _colorChangeSource is not null)
            return;

        SelectedColor = _colorComponents.Color;
    }

    #endregion
}
