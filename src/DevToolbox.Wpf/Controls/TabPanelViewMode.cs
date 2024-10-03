namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Specifies the view modes for a tab panel.
/// </summary>
public enum TabPanelViewMode
{
    /// <summary>
    /// The tabs are displayed with a scrollbar when they exceed the available width.
    /// </summary>
    Scroll,

    /// <summary>
    /// The tabs are stretched to fill the available width.
    /// </summary>
    Stretch,

    /// <summary>
    /// The tabs are displayed in multiple lines if they exceed the available width.
    /// </summary>
    MultiLineTabHeaders
}
