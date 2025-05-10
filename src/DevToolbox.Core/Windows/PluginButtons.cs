using System;

namespace DevToolbox.Core.Windows;

/// <summary>
/// Specifies which buttons to include in a dialog, using flag combinations.
/// </summary>
[Flags]
public enum PluginButtons
{
    /// <summary>
    /// No buttons are displayed.
    /// </summary>
    None = 0,

    /// <summary>
    /// Displays a "Yes" button.
    /// </summary>
    Yes = 1 << 0,

    /// <summary>
    /// Displays an "OK" button.
    /// </summary>
    OK = 1 << 1,

    /// <summary>
    /// Displays a "No" button.
    /// </summary>
    No = 1 << 2,

    /// <summary>
    /// Displays a "Cancel" button.
    /// </summary>
    Cancel = 1 << 3
}
