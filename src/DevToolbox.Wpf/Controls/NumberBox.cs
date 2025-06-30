using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DevToolbox.Wpf.Data;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a custom control that allows users to enter and edit numeric values.
/// The control includes support for stepping through numbers using buttons or keys,
/// and can restrict values within defined limits (minimum and maximum).
/// </summary>
public class NumberBox : Control
{
    #region Fields/Consts

    private static readonly Regex _exprAllowedChars = new(@"^[0-9\.\,\+\-\*/\(\)\s]+$");

    private bool _valueUpdating;
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
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(NumberBox), new FrameworkPropertyMetadata(default(CornerRadius)));

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
        DependencyProperty.Register(nameof(AreButtonsVisible), typeof(bool), typeof(NumberBox), new FrameworkPropertyMetadata(true));

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

    #endregion

    #region Properties

    /// <summary>
    /// Routed command that increments the current <see cref="NumberBox.Value"/> by the <see cref="SmallChange"/> amount.
    /// </summary>
    public static RoutedCommand Increase { get; } = new(nameof(Increase), typeof(NumberBox));

    /// <summary>
    /// Routed command that decrements the current <see cref="NumberBox.Value"/> by the <see cref="SmallChange"/> amount.
    /// </summary>
    public static RoutedCommand Reduce { get; } = new(nameof(Reduce), typeof(NumberBox));

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

    #endregion

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
        NumberFormatter ??= GetRegionalSettingsAwareDecimalFormatter();

        // CommandBindings for Increase/Reduce
        CommandBindings.Add(new CommandBinding(
            Increase,
            (s, e) =>
            {
                StepValue(SmallChange);
                _ = Focus();
            },
            (s, e) => e.CanExecute = true));

        CommandBindings.Add(new CommandBinding(
            Reduce,
            (s, e) =>
            {
                StepValue(-SmallChange);
                _ = Focus();
            },
            (s, e) => e.CanExecute = true));
    }

    #region Methods Overrides

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        if (_textBox != null)
        {
            _textBox.PreviewTextInput -= OnPreviewTextInput;
            DataObject.RemovePastingHandler(_textBox, OnTextBoxPasting);
            _textBox.PreviewKeyDown -= OnPreviewKeyDown;
        }

        _textBox = Template.FindName("PART_TextBox", this) as TextBox;

        if (_textBox != null)
        {
            _textBox.PreviewTextInput += OnPreviewTextInput;
            DataObject.AddPastingHandler(_textBox, OnTextBoxPasting);
            _textBox.PreviewKeyDown += OnPreviewKeyDown;
            InputMethod.SetIsInputMethodEnabled(_textBox, false);
        }
    }

    /// <inheritdoc />
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);

        ValidateInput();
    }

    /// <inheritdoc />
    protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
    {
        base.OnTemplateChanged(oldTemplate, newTemplate);

        if (string.IsNullOrEmpty(_textBox?.Text) && Value != null)
        {
            UpdateValueToText();
        }
        else
        {
            UpdateTextToValue();
        }
    }

    /// <inheritdoc />
    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

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

    #endregion

    #region Methods

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

    private void StepValue(double? change)
    {
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
        if (ValidationMode == NumberBoxValidationMode.Disabled)
            return;

        var text = _textBox?.Text.Trim();
        if (string.IsNullOrEmpty(text))
        {
            SetCurrentValue(ValueProperty, null);
            return;
        }

        var numberParser = NumberFormatter as INumberParser;
        var value = numberParser?.ParseDouble(text);

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

    private static ValidateNumberFormatter GetRegionalSettingsAwareDecimalFormatter()
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

    #endregion

    #region Events Subscriptions

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (ValidationMode == NumberBoxValidationMode.Disabled || _textBox == null)
            return;

        if (AcceptsExpression)
        {
            var text = _textBox.Text;
            var start = _textBox.SelectionStart;
            var length = _textBox.SelectionLength;
            var preview = text.Remove(start, length).Insert(start, e.Text);

            if (!_exprAllowedChars.IsMatch(preview))
                e.Handled = true;

            return;
        }

        var sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        var newText = _textBox.Text
            .Remove(_textBox.SelectionStart, _textBox.SelectionLength)
            .Insert(_textBox.SelectionStart, e.Text);

        var parser = NumberFormatter as INumberParser;
        if (parser?.ParseDouble(newText) == null)
        {
            if (newText != sep && !(newText.EndsWith(sep) && Regex.IsMatch(newText.Substring(0, newText.Length - sep.Length), @"^\d+$")))
            {
                e.Handled = true;
            }
        }
    }

    private void OnTextBoxPasting(object sender, DataObjectPastingEventArgs e)
    {
        if (ValidationMode == NumberBoxValidationMode.Disabled || sender is not TextBox tb)
            return;

        if (AcceptsExpression && e.DataObject.GetDataPresent(DataFormats.Text))
        {
            var paste = (string?)e.DataObject.GetData(DataFormats.Text) ?? "";
            var text = tb.Text;
            var start = tb.SelectionStart;
            var length = tb.SelectionLength;
            var preview = text.Remove(start, length).Insert(start, paste);

            if (!_exprAllowedChars.IsMatch(preview))
                e.CancelCommand();

            return;
        }

        if (e.DataObject.GetDataPresent(DataFormats.Text))
        {
            var paste = (string?)e.DataObject.GetData(DataFormats.Text) ?? "";
            var text = tb.Text;
            var start = tb.SelectionStart;
            var len = tb.SelectionLength;
            var preview = text.Remove(start, len).Insert(start, paste);

            var parser = NumberFormatter as INumberParser;
            if (parser?.ParseDouble(preview) == null)
                e.CancelCommand();
        }
        else
        {
            e.CancelCommand();
        }
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
            e.Handled = true;
    }

    #endregion
}