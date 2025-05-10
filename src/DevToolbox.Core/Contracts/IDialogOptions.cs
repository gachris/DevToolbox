using System.Collections.Generic;
using DevToolbox.Core.Windows;

namespace DevToolbox.Core.Contracts;

/// <summary>
/// Encapsulates options for configuring the appearance and behavior of a dialog window.
/// </summary>
public interface IDialogOptions
{
    #region Properties

    /// <summary>
    /// Gets or sets the minimum width of the dialog window.
    /// </summary>
    double MinWidth { get; set; }

    /// <summary>
    /// Gets or sets the minimum height of the dialog window.
    /// </summary>
    double MinHeight { get; set; }

    /// <summary>
    /// Gets or sets the initial width of the dialog window.
    /// </summary>
    double Width { get; set; }

    /// <summary>
    /// Gets or sets the initial height of the dialog window.
    /// </summary>
    double Height { get; set; }

    /// <summary>
    /// Gets or sets the maximum width of the dialog window.
    /// </summary>
    double MaxWidth { get; set; }

    /// <summary>
    /// Gets or sets the maximum height of the dialog window.
    /// </summary>
    double MaxHeight { get; set; }

    /// <summary>
    /// Gets or sets the title text displayed in the dialog window's title bar.
    /// </summary>
    string? WindowTitle { get; set; }

    /// <summary>
    /// Gets or sets the title text shown in the waiting animation, if any.
    /// </summary>
    string? AnimationTitle { get; set; }

    /// <summary>
    /// Gets or sets the message text shown in the waiting animation, if any.
    /// </summary>
    string? AnimationMessage { get; set; }

    /// <summary>
    /// Gets or sets any plugin-provided buttons to display in the dialog footer.
    /// </summary>
    List<PluginButton> PluginButtons { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the dialog window's title bar is visible.
    /// </summary>
    bool IsTitleBarVisible { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the dialog window's icon is visible.
    /// </summary>
    bool ShowIcon { get; set; }

    #endregion
}
