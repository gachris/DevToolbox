using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A message bar control that supports different severity levels, closability, and user actions.
/// </summary>
public class MessageBar : ContentControl
{
    static MessageBar()
    {
        // Associates this control with its default style
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MessageBar),
            new FrameworkPropertyMetadata(typeof(MessageBar)));
    }

    /// <summary>
    /// Identifies the <see cref="CornerRadius"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(MessageBar),
            new PropertyMetadata(new CornerRadius(4)));

    /// <summary>
    /// Gets or sets the corner radius of the message bar border.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="Severity"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SeverityProperty =
        DependencyProperty.Register(
            nameof(Severity),
            typeof(Severity),
            typeof(MessageBar),
            new PropertyMetadata(Severity.Info));

    /// <summary>
    /// Gets or sets the severity level of the message (e.g., Info, Warning, Error).
    /// </summary>
    public Severity Severity
    {
        get => (Severity)GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="IsClosable"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsClosableProperty =
        DependencyProperty.Register(
            nameof(IsClosable),
            typeof(bool),
            typeof(MessageBar),
            new PropertyMetadata(true));

    /// <summary>
    /// Gets or sets a value indicating whether the message bar can be closed by the user.
    /// </summary>
    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="CloseCommand"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CloseCommandProperty =
        DependencyProperty.Register(
            nameof(CloseCommand),
            typeof(ICommand),
            typeof(MessageBar),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the command that is invoked when the close button is clicked.
    /// </summary>
    public ICommand CloseCommand
    {
        get => (ICommand)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    /// <summary>
    /// Gets the collection of actions associated with the message bar.
    /// This can be bound to an <see cref="ItemsControl"/> in the control template to display buttons or other interactive elements.
    /// </summary>
    public ObservableCollection<object> Actions { get; } = [];
}
