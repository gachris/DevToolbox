namespace DevToolbox.Core.Windows;

/// <summary>
/// Specifies the semantic intent of a button provided by a plugin for dialog windows.
/// </summary>
public enum PluginButtonType
{
    /// <summary>
    /// Represents a "Yes" action, typically used to confirm or agree.
    /// </summary>
    Yes,

    /// <summary>
    /// Represents a "No" action, typically used to decline or disagree.
    /// </summary>
    No,

    /// <summary>
    /// Represents an "OK" action, typically used to accept or acknowledge.
    /// </summary>
    OK,

    /// <summary>
    /// Represents a "Cancel" action, typically used to dismiss without saving.
    /// </summary>
    Cancel
}