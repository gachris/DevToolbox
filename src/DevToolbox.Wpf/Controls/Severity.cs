namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents the severity level of a message displayed in a <see cref="MessageBar"/>.
/// </summary>
public enum Severity
{
    /// <summary>
    /// Informational message (default style).
    /// </summary>
    Info,

    /// <summary>
    /// Indicates a successful operation or state.
    /// </summary>
    Success,

    /// <summary>
    /// Warns the user about a potential issue.
    /// </summary>
    Warning,

    /// <summary>
    /// Represents an ongoing process or loading state.
    /// </summary>
    Progress,

    /// <summary>
    /// Indicates an error or failure condition.
    /// </summary>
    Error
}
