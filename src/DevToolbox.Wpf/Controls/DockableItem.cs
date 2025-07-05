using System.Windows;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A tab item used within <see cref="DockableControl"/> that supports custom styling.
/// </summary>
public class DockableItem : TabItemEdit
{
    /// <summary>
    /// Initializes static members of the <see cref="DockableItem"/> class.
    /// Overrides the default style key to apply the <see cref="DockableItem"/> template.
    /// </summary>
    static DockableItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DockableItem),
            new FrameworkPropertyMetadata(typeof(DockableItem)));
    }
}
