using System.Windows.Input;

namespace DevToolbox.Wpf.Windows.Commands;

/// <summary>
/// Defines the standard routed commands used by dialog windows, such as OK, Cancel, Yes, and No.
/// </summary>
public static class DialogWindowCommands
{
    #region Routed Commands

    /// <summary>
    /// Gets the <see cref="RoutedCommand"/> that signals a dialog window should cancel/close without applying changes.
    /// </summary>
    public static RoutedCommand Cancel { get; } = new(nameof(Cancel), typeof(DialogWindow));

    /// <summary>
    /// Gets the <see cref="RoutedCommand"/> that signals a dialog window should accept/apply changes and close.
    /// </summary>
    public static RoutedCommand OK { get; } = new(nameof(OK), typeof(DialogWindow));

    /// <summary>
    /// Gets the <see cref="RoutedCommand"/> that signals a positive response (Yes) in a dialog window.
    /// </summary>
    public static RoutedCommand Yes { get; } = new(nameof(Yes), typeof(DialogWindowCommands));

    /// <summary>
    /// Gets the <see cref="RoutedCommand"/> that signals a negative response (No) in a dialog window.
    /// </summary>
    public static RoutedCommand No { get; } = new(nameof(No), typeof(DialogWindowCommands));

    #endregion
}