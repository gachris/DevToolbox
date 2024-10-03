using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a custom control that allows users to enter and edit numeric values.
/// The control includes support for stepping through numbers using buttons or keys,
/// and can restrict values within defined limits (minimum and maximum).
/// </summary>
public class NumberBox : Control
{
    private bool _valueUpdating;

    private RepeatButton? _buttonIncrease;
    private RepeatButton? _buttonReduce;
    private TextBox? _textBox;

    /// <summary>
    /// Identifies the <see cref="Value"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double?),
        typeof(NumberBox),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnValueChanged,
            null,
            false,
            UpdateSourceTrigger.LostFocus
        )
    );

    /// <summary>
    /// Identifies the <see cref="MaxDecimalPlaces"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MaxDecimalPlacesProperty = DependencyProperty.Register(
        nameof(MaxDecimalPlaces),
        typeof(int),
        typeof(NumberBox),
        new PropertyMetadata(6)
    );

    /// <summary>
    /// Identifies the <see cref="CornerRadius"/> dependency property, 
    /// allowing customization of the corner radius of the control.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(NumberBox), new FrameworkPropertyMetadata(default(CornerRadius)));

    /// <summary>
    /// Gets or sets the radius for rounding the corners of the control.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="SmallChange"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SmallChangeProperty = DependencyProperty.Register(
        nameof(SmallChange),
        typeof(double),
        typeof(NumberBox),
        new PropertyMetadata(1.0d)
    );

    /// <summary>
    /// Identifies the <see cref="LargeChange"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty LargeChangeProperty = DependencyProperty.Register(
        nameof(LargeChange),
        typeof(double),
        typeof(NumberBox),
        new PropertyMetadata(10.0d)
    );

    /// <summary>
    /// Identifies the <see cref="Maximum"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum),
        typeof(double),
        typeof(NumberBox),
        new PropertyMetadata(double.MaxValue)
    );

    /// <summary>
    /// Identifies the <see cref="Minimum"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum),
        typeof(double),
        typeof(NumberBox),
        new PropertyMetadata(double.MinValue)
    );

    /// <summary>
    /// Identifies the <see cref="AcceptsExpression"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty AcceptsExpressionProperty = DependencyProperty.Register(
        nameof(AcceptsExpression),
        typeof(bool),
        typeof(NumberBox),
        new PropertyMetadata(true)
    );

    /// <summary>
    /// Identifies the <see cref="AreButtonsVisible"/> dependency property, controlling the visibility of the increment/decrement buttons.
    /// </summary>
    public static readonly DependencyProperty AreButtonsVisibleProperty =
        DependencyProperty.Register("AreButtonsVisible", typeof(bool), typeof(NumberBox), new FrameworkPropertyMetadata(true, OnAreButtonsVisibleChanged));

    /// <summary>
    /// Identifies the <see cref="ValidationMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ValidationModeProperty = DependencyProperty.Register(
        nameof(ValidationMode),
        typeof(NumberBoxValidationMode),
        typeof(NumberBox),
        new PropertyMetadata(NumberBoxValidationMode.InvalidInputOverwritten)
    );

    /// <summary>
    /// Identifies the <see cref="NumberFormatter"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty NumberFormatterProperty = DependencyProperty.Register(
        nameof(NumberFormatter),
        typeof(INumberFormatter),
        typeof(NumberBox),
        new PropertyMetadata(null, OnNumberFormatterChanged)
    );

    /// <summary>
    /// Identifies the <see cref="ValueChanged"/> routed event.
    /// </summary>
    public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(ValueChanged),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(NumberBox)
    );

    /// <summary>
    /// Gets or sets the numeric value of a <see cref="NumberBox"/>.
    /// </summary>
    public double? Value
    {
        get => (double?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of decimal places to be rounded when converting from Text to Value.
    /// </summary>
    public int MaxDecimalPlaces
    {
        get => (int)GetValue(MaxDecimalPlacesProperty);
        set => SetValue(MaxDecimalPlacesProperty, value);
    }

    /// <summary>
    /// Gets or sets the value that is added to or subtracted from <see cref="Value"/> when a small change is made, such as with an arrow key or scrolling.
    /// </summary>
    public double SmallChange
    {
        get => (double)GetValue(SmallChangeProperty);
        set => SetValue(SmallChangeProperty, value);
    }

    /// <summary>
    /// Gets or sets the value that is added to or subtracted from <see cref="Value"/> when a large change is made, such as with the PageUP and PageDown keys.
    /// </summary>
    public double LargeChange
    {
        get => (double)GetValue(LargeChangeProperty);
        set => SetValue(LargeChangeProperty, value);
    }

    /// <summary>
    /// Gets or sets the numerical maximum for <see cref="Value"/>.
    /// </summary>
    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>
    /// Gets or sets the numerical minimum for <see cref="Value"/>.
    /// </summary>
    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the control will accept and evaluate a basic formulaic expression entered as input.
    /// </summary>
    public bool AcceptsExpression
    {
        get => (bool)GetValue(AcceptsExpressionProperty);
        set => SetValue(AcceptsExpressionProperty, value);
    }

    /// <summary>
    /// Gets or sets the number formatter.
    /// </summary>
    public INumberFormatter? NumberFormatter
    {
        get => (INumberFormatter?)GetValue(NumberFormatterProperty);
        set => SetValue(NumberFormatterProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the increment and decrement buttons are visible.
    /// </summary>
    public bool AreButtonsVisible
    {
        get => (bool)GetValue(AreButtonsVisibleProperty);
        set => SetValue(AreButtonsVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets the input validation behavior to invoke when invalid input is entered.
    /// </summary>
    public NumberBoxValidationMode ValidationMode
    {
        get => (NumberBoxValidationMode)GetValue(ValidationModeProperty);
        set => SetValue(ValidationModeProperty, value);
    }

    /// <summary>
    /// Occurs after the user triggers evaluation of new input by pressing the Enter key, clicking a spin button, or by changing focus.
    /// </summary>
    public event RoutedEventHandler ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    static NumberBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberBox), new FrameworkPropertyMetadata(typeof(NumberBox)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberBox"/> class.
    /// </summary>
    public NumberBox()
        : base()
    {
        NumberFormatter ??= NumberBox.GetRegionalSettingsAwareDecimalFormatter();
        DataObject.AddPastingHandler(this, OnClipboardPaste);
    }

    /// <inheritdoc />
    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        // Handle key inputs for stepping through values or validating input.
        switch (e.Key)
        {
            case Key.PageUp:
                StepValue(LargeChange);
                break;
            case Key.PageDown:
                StepValue(-LargeChange);
                break;
            case Key.Up:
                StepValue(SmallChange);
                break;
            case Key.Down:
                StepValue(-SmallChange);
                break;
            case Key.Enter:
                ValidateInput();
                MoveCaretToTextEnd();
                break;
        }
    }
    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        if (_buttonIncrease != null)
            _buttonIncrease.Click -= OnButtonIncrease_Click;

        _buttonIncrease = Template.FindName("PART_ButtonIncrease", this) as RepeatButton;

        if (_buttonIncrease != null)
            _buttonIncrease.Click += OnButtonIncrease_Click;

        if (_buttonReduce != null)
            _buttonReduce.Click -= OnButtonReduce_Click;

        _buttonReduce = Template.FindName("PART_ButtonReduce", this) as RepeatButton;

        if (_buttonReduce != null)
            _buttonReduce.Click += OnButtonReduce_Click;

        _textBox = Template.FindName("PART_TextBox", this) as TextBox;

        UpdateButtonsVisibility();
    }

    private void OnButtonIncrease_Click(object sender, RoutedEventArgs e)
    {
        StepValue(SmallChange);
        _ = Focus();
    }

    private void OnButtonReduce_Click(object sender, RoutedEventArgs e)
    {
        StepValue(-SmallChange);
        _ = Focus();
    }

    /// <inheritdoc />
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);

        ValidateInput();
    }

    /// <inheritdoc />
    protected override void OnTemplateChanged(
        System.Windows.Controls.ControlTemplate oldTemplate,
        System.Windows.Controls.ControlTemplate newTemplate
    )
    {
        base.OnTemplateChanged(oldTemplate, newTemplate);

        // If Text has been set, but Value hasn't, update Value based on Text.
        if (string.IsNullOrEmpty(_textBox?.Text) && Value != null)
        {
            UpdateValueToText();
        }
        else
        {
            UpdateTextToValue();
        }
    }

    /// <summary>
    /// Is called when <see cref="Value"/> in this <see cref="NumberBox"/> changes.
    /// </summary>
    protected virtual void OnValueChanged(DependencyObject d, double? oldValue)
    {
        if (_valueUpdating)
        {
            return;
        }

        _valueUpdating = true;

        var newValue = Value;

        if (newValue > Maximum)
        {
            SetCurrentValue(ValueProperty, Maximum);
        }

        if (newValue < Minimum)
        {
            SetCurrentValue(ValueProperty, Minimum);
        }

        if (!Equals(newValue, oldValue))
        {
            RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
        }

        UpdateTextToValue();

        _valueUpdating = false;
    }

    /// <summary>
    /// Is called when something is pasted in this <see cref="NumberBox"/>.
    /// </summary>
    protected virtual void OnClipboardPaste(object sender, DataObjectPastingEventArgs e)
    {
        // TODO: Fix clipboard
        if (sender is not NumberBox)
        {
            return;
        }

        ValidateInput();
    }

    private void StepValue(double? change)
    {
        // Before adjusting the value, validate the contents of the textbox so we don't override it.
        ValidateInput();

        var newValue = Value ?? 0;

        if (change is not null)
        {
            newValue += change ?? 0d;
        }

        SetCurrentValue(ValueProperty, newValue);

        MoveCaretToTextEnd();
    }

    private void UpdateTextToValue()
    {
        var newText = string.Empty;

        if (Value is not null && NumberFormatter is not null)
        {
            newText = NumberFormatter.FormatDouble(Math.Round((double)Value, MaxDecimalPlaces));
        }

        _textBox?.SetCurrentValue(TextBox.TextProperty, newText);
    }

    private void UpdateValueToText()
    {
        ValidateInput();
    }

    private void ValidateInput()
    {
        var text = _textBox?.Text.Trim();

        if (string.IsNullOrEmpty(text))
        {
            SetCurrentValue(ValueProperty, null);

            return;
        }

        var numberParser = NumberFormatter as INumberParser;
        var value = numberParser!.ParseDouble(text);

        if (value is null || Equals(Value, value))
        {
            UpdateTextToValue();

            return;
        }

        if (value > Maximum)
        {
            value = Maximum;
        }

        if (value < Minimum)
        {
            value = Minimum;
        }

        SetCurrentValue(ValueProperty, value);

        UpdateTextToValue();
    }

    private void MoveCaretToTextEnd()
    {
        if (_textBox is null)
        {
            return;
        }

        _textBox.CaretIndex = _textBox.Text.Length;
    }

    private static INumberFormatter GetRegionalSettingsAwareDecimalFormatter()
    {
        return new ValidateNumberFormatter();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NumberBox numberBox)
        {
            return;
        }

        numberBox.OnValueChanged(d, (double?)e.OldValue);
    }

    private static void OnNumberFormatterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not INumberParser)
        {
            throw new InvalidOperationException(
                $"{nameof(NumberFormatter)} must implement {typeof(INumberParser)}"
            );
        }
    }

    private static void OnAreButtonsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var numberBox = (NumberBox)d;
        numberBox.UpdateButtonsVisibility();
    }

    private void UpdateButtonsVisibility()
    {
        if (_buttonIncrease != null)
            _buttonIncrease.Visibility = AreButtonsVisible ? Visibility.Visible : Visibility.Collapsed;

        if (_buttonReduce != null)
            _buttonReduce.Visibility = AreButtonsVisible ? Visibility.Visible : Visibility.Collapsed;
    }
}