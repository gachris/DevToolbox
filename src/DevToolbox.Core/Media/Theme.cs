namespace DevToolbox.Core.Media;

/// <summary>
/// Represents the visual theme options available for the application.
/// </summary>
public enum Theme
{
    /// <summary>
    /// Uses the default theme as specified by the system or application settings.
    /// </summary>
    Default,

    /// <summary>
    /// Uses a light theme with brighter backgrounds and dark text.
    /// </summary>
    Light,

    /// <summary>
    /// Uses a dark theme with darker backgrounds and light text.
    /// </summary>
    Dark,

    /// <summary>
    /// Disables theming and applies no specific theme styling.
    /// </summary>
    None
}