namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Specifies the different modes for displaying the close button in a tab control.
/// </summary>
public enum CloseButtonShowMode
{
    /// <summary>
    /// Display the close button in all tabs.
    /// </summary>
    InAllTabs = 0,

    /// <summary>
    /// Display the close button only in the tab control (not in individual tabs).
    /// </summary>
    InTabControl = 1,

    /// <summary>
    /// Display the close button only in the active tab.
    /// </summary>
    InActiveTab = 2,

    /// <summary>
    /// Display the close button in the active tab and tabs where the mouse is hovering.
    /// </summary>
    InActiveAndMouseOverTabs = 3,

    /// <summary>
    /// Display the close button in both all tabs and the tab control.
    /// </summary>
    InAllTabsAndTabControl = 4,

    /// <summary>
    /// Display the close button in the active tab, tabs where the mouse is hovering, and the tab control.
    /// </summary>
    InActiveAndMouseOverTabsAndTabControl = 5,

    /// <summary>
    /// Do not display the close button anywhere.
    /// </summary>
    NoWhere = 6
}
