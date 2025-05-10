using System.Windows.Input;

namespace DevToolbox.Wpf.Windows.ViewModels;

/// <summary>
/// ViewModel representing a button added by a plugin within a dialog window.
/// </summary>
public class PluginButtonViewModel
{
    /// <summary>
    /// Gets the content to display on the button (e.g., text or UI element).
    /// </summary>
    public object Content { get; }

    /// <summary>
    /// Gets a value indicating whether this button is the default action button.
    /// </summary>
    public bool IsDefault { get; }

    /// <summary>
    /// Gets a value indicating whether this button is the cancel action button.
    /// </summary>
    public bool IsCancel { get; }

    /// <summary>
    /// Gets the command to execute when the button is clicked.
    /// </summary>
    public ICommand Command { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginButtonViewModel"/> class.
    /// </summary>
    /// <param name="content">The content to display on the button.</param>
    /// <param name="command">The command to invoke when the button is clicked.</param>
    /// <param name="isDefault">
    /// <c>true</c> if this button should be treated as the default action (activated on Enter key); otherwise, <c>false</c>.
    /// </param>
    /// <param name="isCancel">The whether this button is the cancel action button.</param>
    public PluginButtonViewModel(object content, ICommand command, bool isDefault, bool isCancel)
    {
        Content = content;
        IsDefault = isDefault;
        IsCancel = isCancel;
        Command = command;
    }
}
