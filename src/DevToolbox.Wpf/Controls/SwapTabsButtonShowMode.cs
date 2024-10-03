namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Specifies the visibility modes for the swap tabs button.
/// </summary>
public enum SwapTabsButtonShowMode
{
    /// <summary>
    /// The swap tabs button is always visible.
    /// </summary>
    Visible = 0,

    /// <summary>
    /// The swap tabs button is hidden and not displayed.
    /// </summary>
    Hidden = 1,

    /// <summary>
    /// The swap tabs button is visible only when there are existing hidden tabs.
    /// </summary>
    WhenHiddenTabsExisting = 2
}
