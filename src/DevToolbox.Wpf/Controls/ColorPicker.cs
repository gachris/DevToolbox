﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ColorSpace.Net;
using ColorSpace.Net.Componentes;
using DevToolbox.Wpf.Helpers;
using PointEx = System.Drawing.Point;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A control representing a color picker for selecting colors, supporting different color components and normal maps.
/// </summary>
[TemplatePart(Name = PART_ColorPlaneEllipse, Type = typeof(Ellipse))]
[TemplatePart(Name = PART_ColorCanvas, Type = typeof(ContentControl))]
[TemplatePart(Name = PART_ColorComponents, Type = typeof(ContentPresenter))]
[TemplatePart(Name = PART_NormalSlider, Type = typeof(Slider))]
[TemplatePart(Name = PART_AlphaSlider, Type = typeof(Slider))]
public class ColorPicker : Control
{
    private enum ColorChangeSource
    {
        MouseDown,
        Normal
    }

    #region Fields/Consts

    private const string PART_ColorPlaneEllipse = nameof(PART_ColorPlaneEllipse);
    private const string PART_ColorCanvas = nameof(PART_ColorCanvas);
    private const string PART_ColorComponents = nameof(PART_ColorComponents);
    private const string PART_NormalSlider = nameof(PART_NormalSlider);
    private const string PART_AlphaSlider = nameof(PART_AlphaSlider);

    private INormalComponent? _normalComponent;
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

    /// <summary>
    /// Event raised when the alpha (transparency) value changes.
    /// </summary>
    public event EventHandler? AlphaChanged;

    /// <summary>
    /// Event raised when the selected color changes.
    /// </summary>
    public event EventHandler? SelectedColorChanged;

    /// <summary>
    /// DependencyProperty for Alpha (transparency) value.
    /// </summary>
    public static readonly DependencyProperty AlphaProperty =
        DependencyProperty.Register(nameof(Alpha), typeof(byte), typeof(ColorPicker), new FrameworkPropertyMetadata((byte)255, OnAlphaChanged, AlphaCoerceValue));

    /// <summary>
    /// DependencyProperty for ColorPickerStyle, which defines the style and behavior of the color picker.
    /// </summary>
    public static readonly DependencyProperty ColorPickerStyleProperty =
        DependencyProperty.Register(nameof(ColorPickerStyle), typeof(ColorPickerStyle), typeof(ColorPicker), new FrameworkPropertyMetadata(default(ColorPickerStyle)));

    private static readonly DependencyPropertyKey MinNormalPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(MinNormal), typeof(int), typeof(ColorPicker), new FrameworkPropertyMetadata(default));

    private static readonly DependencyPropertyKey MaxNormalPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(MaxNormal), typeof(int), typeof(ColorPicker), new FrameworkPropertyMetadata(default));

    /// <summary>
    /// DependencyProperty for NormalComponentType, which specifies the type of color component being modified.
    /// </summary>
    public static readonly DependencyProperty NormalComponentTypeProperty =
        DependencyProperty.Register(nameof(NormalComponentType), typeof(NormalComponentType), typeof(ColorPicker), new FrameworkPropertyMetadata(default(NormalComponentType), OnNormalComponentTypeChanged));

    /// <summary>
    /// DependencyProperty for Normal value.
    /// </summary>
    public static readonly DependencyProperty NormalProperty =
        DependencyProperty.Register(nameof(Normal), typeof(int), typeof(ColorPicker), new FrameworkPropertyMetadata(0, OnNormalChanged, NormalCoerceValue));

    /// <summary>
    /// DependencyProperty for the SelectedColor.
    /// </summary>
    public static readonly DependencyProperty SelectedColorProperty =
        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorPicker), new FrameworkPropertyMetadata(Colors.Black, OnSelectedColorChanged));

    /// <summary>
    /// DependencyProperty for the SelectedColor.
    /// </summary>
    public static readonly DependencyProperty InitialColorProperty =
        DependencyProperty.Register(nameof(InitialColor), typeof(Color), typeof(ColorPicker), new FrameworkPropertyMetadata(Colors.Black));

    /// <summary>
    /// DependencyProperty for MinNormal. It represents the minimum value for the normal component.
    /// </summary>
    public static readonly DependencyProperty MinNormalProperty = MinNormalPropertyKey.DependencyProperty;

    /// <summary>
    /// DependencyProperty for MaxNormal. It represents the maximum value for the normal component.
    /// </summary>
    public static readonly DependencyProperty MaxNormalProperty = MaxNormalPropertyKey.DependencyProperty;

    #endregion

    #region Properties


    /// <summary>
    /// Gets or sets the Alpha (transparency) value.
    /// </summary>
    public byte Alpha
    {
        get => (byte)GetValue(AlphaProperty);
        set => SetValue(AlphaProperty, value);
    }

    /// <summary>
    /// Gets or sets the ColorPickerStyle which defines how the picker is presented and whether alpha is enabled.
    /// </summary>
    public ColorPickerStyle ColorPickerStyle
    {
        get => (ColorPickerStyle)GetValue(ColorPickerStyleProperty);
        set => SetValue(ColorPickerStyleProperty, value);
    }

    /// <summary>
    /// Gets a value indicating whether the alpha slider is enabled.
    /// </summary>
    private bool IsAlphaEnabled => ColorPickerStyle is ColorPickerStyle.StandardWithAlpha or ColorPickerStyle.FullWithAlpha;

    /// <summary>
    /// Gets the minimum normal value.
    /// </summary>
    public int MinNormal => (int)GetValue(MinNormalProperty);

    /// <summary>
    /// Gets the maximum normal value.
    /// </summary>
    public int MaxNormal => (int)GetValue(MaxNormalProperty);

    /// <summary>
    /// Gets or sets the type of normal component.
    /// </summary>
    public NormalComponentType NormalComponentType
    {
        get => (NormalComponentType)GetValue(NormalComponentTypeProperty);
        set => SetValue(NormalComponentTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets the normal value.
    /// </summary>
    public int Normal
    {
        get => (int)GetValue(NormalProperty);
        set => SetValue(NormalProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected color in the color picker.
    /// </summary>
    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the initial color of the color picker.
    /// </summary>
    public Color InitialColor
    {
        get => (Color)GetValue(InitialColorProperty);
        set => SetValue(InitialColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the current selection point in the color plane.
    /// </summary>
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

    /// <summary>
    /// Static constructor to override the default style for the ColorPicker control.
    /// </summary>
    public ColorPicker()
    {
    }

    #region Methods Override

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_colorPlaneEllipse is not null)
        {
            _colorPlaneEllipse.PreviewKeyDown -= ColorPlaneEllipse_KeyDown;
        }

        _colorPlaneEllipse = Template.FindName(PART_ColorPlaneEllipse, this) as Ellipse;

        if (_colorPlaneEllipse is not null)
        {
            _colorPlaneEllipse.Focusable = true;
            _colorPlaneEllipse.PreviewKeyDown += ColorPlaneEllipse_KeyDown;
        }

        if (_colorCanvas is not null)
        {
            _colorCanvas.MouseMove -= ColorCanvas_MouseMove;
            _colorCanvas.MouseDown -= ColorCanvas_MouseDown;
            _colorCanvas.MouseUp -= ColorCanvas_MouseUp;
        }

        _colorCanvas = Template.FindName(PART_ColorCanvas, this) as ContentControl;

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
        _colorComponentsWrapper = Template.FindName(PART_ColorComponents, this) as ContentPresenter;

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

        _normalSlider = Template.FindName(PART_NormalSlider, this) as Slider;
        _alphaSlider = Template.FindName(PART_AlphaSlider, this) as Slider;

        UpdateNormalComponent();
    }

    #endregion

    #region Methods

    private void UpdateColorCanvasBitmap(int normal)
    {
        if (_colorCanvas is null || _normalComponent is null)
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
        if (_normalSlider is null || _normalComponent is null)
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
        if (_alphaSlider is null || _normalComponent is null)
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
        if (_normalComponent is null)
            return;

        SelectionPoint = CoercePoint(point);

        var drawColor = _normalComponent.ColorAtPoint(SelectionPoint, Normal);
        SelectedColor = ColorPickerHelper.ConvertToMediaColor(drawColor, Alpha);

        if (!_normalComponent.IsNormalIndependantOfColor)
        {
            UpdateSliderNormalBitmap(drawColor);
        }
    }

    private void UpdateNormalComponent()
    {
        if (_colorComponents is null)
            return;

        var drawColor = ColorPickerHelper.ConvertToDrawingColor(SelectedColor);

        EnsureComponent(NormalComponentType);
        _normalComponent = Components[NormalComponentType];

        SetValue(MinNormalPropertyKey, _normalComponent.MinValue);
        SetValue(MaxNormalPropertyKey, _normalComponent.MaxValue);

        _colorComponents.UpdateFromColor(SelectedColor);

        Alpha = drawColor.A;
        SelectionPoint = _normalComponent.PointFromColor(drawColor);
        Normal = _colorComponents.Normal;

        UpdateColorCanvasBitmap(Normal);
        UpdateSliderNormalBitmap(drawColor);
        UpdateSliderAlphaBitmap(drawColor);
    }

    private void EnsureComponent(NormalComponentType normalComponentType)
    {
        if (!Components.ContainsKey(normalComponentType))
        {
            Components[normalComponentType] = NormalComponentFactory.CreateNormalComponent(normalComponentType);
        }
    }

    private void OnAlphaChanged(byte oldValue, byte newValue)
    {
        SelectedColor = Color.FromArgb(newValue, SelectedColor.R, SelectedColor.G, SelectedColor.B);
        AlphaChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnNormalChanged(int oldValue, int newValue)
    {
        if (_normalComponent is null || _colorComponents is  null)
            return;

        _colorChangeSource = ColorChangeSource.Normal;

        _colorComponents.Normal = newValue;
        SelectedColor = ColorPickerHelper.ConvertToMediaColor(_normalComponent.ColorAtPoint(SelectionPoint, newValue), Alpha);

        UpdateColorCanvasBitmap(newValue);

        if (!_normalComponent.IsNormalIndependantOfColor)
        {
            var drawColor = ColorPickerHelper.ConvertToDrawingColor(SelectedColor);
            UpdateSliderNormalBitmap(drawColor);
        }

        _colorChangeSource = null;
    }

    private void OnNormalComponentTypeChanged(NormalComponentType oldValue, NormalComponentType newValue)
    {
        UpdateNormalComponent();
    }

    private void OnSelectedColorChanged(Color oldValue, Color newValue)
    {
        if (_colorComponents is null || _normalComponent is null)
            return;

        var drawColor = ColorPickerHelper.ConvertToDrawingColor(newValue);

        Alpha = drawColor.A;
        UpdateSliderAlphaBitmap(drawColor);

        if (_colorChangeSource is ColorChangeSource.MouseDown)
        {
            _colorComponents.UpdateFromColorWithoutChangeNormalValue(newValue);
        }
        else if (_colorChangeSource is not ColorChangeSource.Normal)
        {
            _colorComponents.UpdateFromColor(newValue);

            SelectionPoint = _normalComponent.PointFromColor(drawColor);
            Normal = _colorComponents.Normal;
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

    private void ColorPlaneEllipse_KeyDown(object sender, KeyEventArgs e)
    {
        const int step = 1; // adjust sensitivity

        var newPoint = _selectionPoint;

        switch (e.Key)
        {
            case Key.Left:
                newPoint.X -= step;
                break;
            case Key.Right:
                newPoint.X += step;
                break;
            case Key.Up:
                newPoint.Y -= step;
                break;
            case Key.Down:
                newPoint.Y += step;
                break;
            default:
                return;
        }

        _colorChangeSource = ColorChangeSource.MouseDown;
        UpdateFromPoint(CoercePoint(newPoint));
        _colorChangeSource = null;

        e.Handled = true;
    }

    #endregion
}
