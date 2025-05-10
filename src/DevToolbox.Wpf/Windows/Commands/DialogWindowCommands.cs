using System.Windows.Input;

namespace DevToolbox.Wpf.Windows.Commands;

/// <summary>
/// Defines the standard routed commands used by dialog windows, such as OK and Cancel.
/// </summary>
public class DialogWindowCommands
{
    #region Fields/Consts

    /// <summary>
    /// Backing field for the <see cref="Cancel"/> routed command.
    /// </summary>
    private static readonly RoutedCommand _cancel = new(nameof(Cancel), typeof(DialogWindow));

    /// <summary>
    /// Backing field for the <see cref="OK"/> routed command.
    /// </summary>
    private static readonly RoutedCommand _ok = new(nameof(OK), typeof(DialogWindow));

    #endregion

    #region Properties

    /// <summary>
    /// Gets the routed command that signals a dialog window should cancel/close without applying changes.
    /// </summary>
    public static RoutedCommand Cancel => _cancel;

    /// <summary>
    /// Gets the routed command that signals a dialog window should accept/apply changes and close.
    /// </summary>
    public static RoutedCommand OK => _ok;

    public static readonly RoutedCommand Yes = new(nameof(Yes), typeof(DialogWindowCommands));

    public static readonly RoutedCommand No = new(nameof(No), typeof(DialogWindowCommands));

    #endregion
}