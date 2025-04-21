using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a customizable editable text box control that supports various properties and commands for editing text.
/// </summary>
[TemplatePart(Name = "PART_TextBlockPart", Type = typeof(TextBlock))]
[TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
public class EditBox : Control
{
    #region Fields/Consts

    /// <summary>
    /// The underlying TextBlock used to display text.
    /// </summary>
    private TextBlock? _textBlock;

    /// <summary>
    /// The underlying TextBox used for text editing.
    /// </summary>
    private TextBox? _textBox;

    /// <summary>
    /// The last time the control was clicked.
    /// </summary>
    private DateTime _lastClicked;

    /// <summary>
    /// Indicates whether the edit operation should be canceled.
    /// </summary>
    private bool _cancelEdit;

    /// <summary>
    /// Dependency property for the caret brush in the TextBox.
    /// </summary>
    public static readonly DependencyProperty CaretBrushProperty = TextBox.CaretBrushProperty.AddOwner(typeof(EditBox));

    /// <summary>
    /// Dependency property for the text wrapping mode in the TextBox.
    /// </summary>
    public static readonly DependencyProperty TextWrappingProperty = TextBox.TextWrappingProperty.AddOwner(typeof(EditBox));

    /// <summary>
    /// Dependency property for the text trimming mode in the TextBlock.
    /// </summary>
    public static readonly DependencyProperty TextTrimmingProperty = TextBlock.TextTrimmingProperty.AddOwner(typeof(EditBox));

    /// <summary>
    /// Dependency property for the text content of the EditBox.
    /// </summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Text", typeof(string), typeof(EditBox), new FrameworkPropertyMetadata(string.Empty, OnTextChanged));

    /// <summary>
    /// Dependency property for the display text of the EditBox.
    /// </summary>
    public static readonly DependencyProperty DisplayTextProperty =
        DependencyProperty.Register("DisplayText", typeof(string), typeof(EditBox), new PropertyMetadata(string.Empty));

    /// <summary>
    /// Dependency property for the foreground brush of the display text.
    /// </summary>
    public static readonly DependencyProperty DisplayTextForegroundBrushProperty =
        DependencyProperty.Register("DisplayTextForegroundBrush", typeof(Brush), typeof(EditBox), new PropertyMetadata(Brushes.Black));

    /// <summary>
    /// Dependency property indicating whether the EditBox is read-only.
    /// </summary>
    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(EditBox), new FrameworkPropertyMetadata(false));

    /// <summary>
    /// Dependency property indicating whether the EditBox is currently in editing mode.
    /// </summary>
    public static readonly DependencyProperty IsEditingProperty =
        DependencyProperty.Register("IsEditing", typeof(bool), typeof(EditBox), new FrameworkPropertyMetadata(false, OnIsEditingChanged, IsEditingCoerceValue));

    /// <summary>
    /// Dependency property indicating whether editing is allowed on double-click.
    /// </summary>
    public static readonly DependencyProperty IsEditableOnDoubleClickProperty =
        DependencyProperty.Register("IsEditableOnDoubleClick", typeof(bool), typeof(EditBox), new PropertyMetadata(true));

    /// <summary>
    /// Dependency property for the minimum click duration to initiate editing.
    /// </summary>
    public static readonly DependencyProperty MinimumClickTimeProperty =
        DependencyProperty.Register("MinimumClickTime", typeof(double), typeof(EditBox), new UIPropertyMetadata(300d));

    /// <summary>
    /// Dependency property for the maximum click duration to initiate editing.
    /// </summary>
    public static readonly DependencyProperty MaximumClickTimeProperty =
        DependencyProperty.Register("MaximumClickTime", typeof(double), typeof(EditBox), new UIPropertyMetadata(700d));

    /// <summary>
    /// Dependency property for the command executed to begin editing.
    /// </summary>
    public static readonly DependencyProperty BeginEditCommandProperty =
        DependencyProperty.Register("BeginEditCommand", typeof(ICommand), typeof(EditBox), new UIPropertyMetadata(null));

    /// <summary>
    /// Dependency property for the parameter passed to the BeginEditCommand.
    /// </summary>
    public static readonly DependencyProperty BeginEditCommandParameterProperty =
        DependencyProperty.Register("BeginEditCommandParameter", typeof(object), typeof(EditBox), new PropertyMetadata(null));

    /// <summary>
    /// Dependency property for the command executed to cancel editing.
    /// </summary>
    public static readonly DependencyProperty CancelEditCommandProperty =
        DependencyProperty.Register("CancelEditCommand", typeof(ICommand), typeof(EditBox), new UIPropertyMetadata(null));

    /// <summary>
    /// Dependency property for the parameter passed to the CancelEditCommand.
    /// </summary>
    public static readonly DependencyProperty CancelEditCommandParameterProperty =
        DependencyProperty.Register("CancelEditCommandParameter", typeof(object), typeof(EditBox), new PropertyMetadata(null));

    /// <summary>
    /// Dependency property for the command executed to end editing.
    /// </summary>
    public static readonly DependencyProperty EndEditCommandProperty =
        DependencyProperty.Register("EndEditCommand", typeof(ICommand), typeof(EditBox), new UIPropertyMetadata(null));

    /// <summary>
    /// Dependency property for the parameter passed to the EndEditCommand.
    /// </summary>
    public static readonly DependencyProperty EndEditCommandParameterProperty =
        DependencyProperty.Register("EndEditCommandParameter", typeof(object), typeof(EditBox), new PropertyMetadata(null));

    /// <summary>
    /// Dependency property indicating whether editing should be canceled when the control loses focus.
    /// </summary>
    public static readonly DependencyProperty CancelOnLostFocusProperty =
        DependencyProperty.Register("CancelOnLostFocus", typeof(bool), typeof(EditBox), new PropertyMetadata(true));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the brush used for the caret in the TextBox.
    /// </summary>
    public Brush CaretBrush
    {
        get => (Brush)GetValue(CaretBrushProperty);
        set => SetValue(CaretBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the text content of the EditBox.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the text wrapping mode for the TextBox.
    /// </summary>
    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    /// <summary>
    /// Gets or sets the text trimming mode for the TextBlock.
    /// </summary>
    public TextTrimming TextTrimming
    {
        get => (TextTrimming)GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }

    /// <summary>
    /// Gets or sets the foreground brush for the display text.
    /// </summary>
    public Brush DisplayTextForegroundBrush
    {
        get => (Brush)GetValue(DisplayTextForegroundBrushProperty);
        set => SetValue(DisplayTextForegroundBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the EditBox is read-only.
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// Gets or sets the display text of the EditBox.
    /// </summary>
    public string DisplayText
    {
        get => (string)GetValue(DisplayTextProperty);
        set => SetValue(DisplayTextProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the EditBox is currently in editing mode.
    /// </summary>
    public bool IsEditing
    {
        get => (bool)GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether editing is allowed on double-click.
    /// </summary>
    public bool IsEditableOnDoubleClick
    {
        get => (bool)GetValue(IsEditableOnDoubleClickProperty);
        set => SetValue(IsEditableOnDoubleClickProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum click time required to initiate editing.
    /// </summary>
    public double MinimumClickTime
    {
        get => (double)GetValue(MinimumClickTimeProperty);
        set => SetValue(MinimumClickTimeProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum click time allowed to initiate editing.
    /// </summary>
    public double MaximumClickTime
    {
        get => (double)GetValue(MaximumClickTimeProperty);
        set => SetValue(MaximumClickTimeProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed to begin editing.
    /// </summary>
    public ICommand BeginEditCommand
    {
        get => (ICommand)GetValue(BeginEditCommandProperty);
        set => SetValue(BeginEditCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter for the BeginEditCommand.
    /// </summary>
    public object BeginEditCommandParameter
    {
        get => GetValue(BeginEditCommandParameterProperty);
        set => SetValue(BeginEditCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed to cancel editing.
    /// </summary>
    public ICommand CancelEditCommand
    {
        get => (ICommand)GetValue(CancelEditCommandProperty);
        set => SetValue(CancelEditCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter for the CancelEditCommand.
    /// </summary>
    public object CancelEditCommandParameter
    {
        get => GetValue(CancelEditCommandParameterProperty);
        set => SetValue(CancelEditCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed to end editing.
    /// </summary>
    public ICommand EndEditCommand
    {
        get => (ICommand)GetValue(EndEditCommandProperty);
        set => SetValue(EndEditCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter for the EndEditCommand.
    /// </summary>
    public object EndEditCommandParameter
    {
        get => GetValue(EndEditCommandParameterProperty);
        set => SetValue(EndEditCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether editing should be canceled when the EditBox loses focus.
    /// </summary>
    public bool CancelOnLostFocus
    {
        get => (bool)GetValue(CancelOnLostFocusProperty);
        set => SetValue(CancelOnLostFocusProperty, value);
    }

    #endregion

    /// <summary>
    /// Static constructor to override the default style for the EditBox control.
    /// </summary>
    static EditBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(EditBox), new FrameworkPropertyMetadata(typeof(EditBox)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EditBox"/> class.
    /// </summary>
    public EditBox()
    {
        // Subscribe to the MouseDown event to handle clicks on the control.
        MouseDown += OnMouseDown;
    }

    #region Methods Override

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _textBlock = GetTemplateChild("PART_TextBlock") as TextBlock;

        if (_textBlock is not null)
        {
            var binding = new Binding(nameof(DisplayText))
            {
                Source = this,
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            _textBlock.SetBinding(TextBlock.TextProperty, binding);

            _textBlock.Visibility = Visibility.Visible;
        }

        if (_textBox is not null)
        {
            _textBox.LayoutUpdated -= OnTextBoxLayoutUpdated;
            _textBox.KeyDown -= OnTextBoxKeyDown;
            _textBox.LostKeyboardFocus -= OnTextBoxLostKeyboardFocus;
            _textBox.LostFocus -= OnTextBoxLostFocus;
        }

        _textBox = GetTemplateChild("PART_TextBox") as TextBox;

        if (_textBox is not null)
        {
            _textBox.LayoutUpdated += OnTextBoxLayoutUpdated;
            _textBox.KeyDown += OnTextBoxKeyDown;
            _textBox.LostKeyboardFocus += OnTextBoxLostKeyboardFocus;
            _textBox.LostFocus += OnTextBoxLostFocus;

            _textBox.IsEnabled = false;
            _textBox.Visibility = Visibility.Collapsed;

            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(_textBox, OnMouseDownOutsideElement);
        }
    }

    #endregion

    #region Methods

    private void BeginEdit()
    {
        IsEditing = true;
    }

    private void CancelEdit()
    {
        _cancelEdit = true;
        IsEditing = false;
    }

    private void EndEdit()
    {
        IsEditing = false;
    }

    private object IsEditingCoerceValue(bool value)
    {
        return !IsReadOnly && value && ((BeginEditCommand?.CanExecute(BeginEditCommandParameter)) ?? true);
    }

    private void OnTextChanged(string oldValue, string newValue)
    {
        CoerceValue(TextProperty);
    }

    private void OnIsEditingChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            if (_textBlock is not null)
            {
                _textBlock.Visibility = Visibility.Collapsed;
            }

            if (_textBox is not null)
            {
                _textBox.Text = Text;
                _textBox.Visibility = Visibility.Visible;
                _textBox.IsEnabled = true;

                _textBox.CaptureMouse();
            }

            BeginEditCommand?.Execute(BeginEditCommandParameter);
        }
        else
        {
            var text = _textBox?.Text ?? string.Empty;

            if (_textBox is not null)
            {
                _textBox.Visibility = Visibility.Collapsed;
                _textBox.Text = null;
                _textBox.IsEnabled = false;
            }

            if (_textBlock is not null)
            {
                _textBlock.Focus();
                _textBlock.Visibility = Visibility.Visible;
            }

            if (_cancelEdit || Validation.GetHasError(this))
            {
                _cancelEdit = false;

                CancelEditCommand?.Execute(CancelEditCommandParameter);

                return;
            }

            Text = text;
            EndEditCommand?.Execute(EndEditCommandParameter);
        }
    }

    private static object IsEditingCoerceValue(DependencyObject d, object value)
    {
        var editBox = (EditBox)d;
        return editBox.IsEditingCoerceValue((bool)value);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var editBox = (EditBox)d;
        editBox.OnTextChanged((string)e.OldValue, (string)e.NewValue);
    }

    private static void OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var editBox = (EditBox)d;
        editBox.OnIsEditingChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    #endregion

    #region Events Subscriptions

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsEditableOnDoubleClick || e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        var timeBetweenClicks = (DateTime.Now - _lastClicked).TotalMilliseconds;

        _lastClicked = DateTime.Now;

        if (timeBetweenClicks > MinimumClickTime && timeBetweenClicks < MaximumClickTime)
        {
            BeginEdit();
        }

        e.Handled = false;
    }

    private void OnMouseDownOutsideElement(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;

        if (CancelOnLostFocus)
        {
            CancelEdit();

            return;
        }

        EndEdit();
    }

    private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
    {
        if (CancelOnLostFocus)
        {
            CancelEdit();

            return;
        }

        EndEdit();
    }

    private void OnTextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (CancelOnLostFocus)
        {
            CancelEdit();

            return;
        }

        EndEdit();
    }

    private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                EndEdit();
                e.Handled = true;
                return;
            case Key.Escape:
                CancelEdit();
                e.Handled = true;
                return;
        }
    }

    private void OnTextBoxLayoutUpdated(object? sender, EventArgs e)
    {
        if (_textBox is not null && _textBox.IsEnabled && !_textBox.IsFocused)
        {
            _textBox.Focus();
            _textBox.SelectAll();
        }
    }

    #endregion
}
