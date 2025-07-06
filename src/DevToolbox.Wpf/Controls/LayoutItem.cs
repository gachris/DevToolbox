using System.Windows;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A tab item representing a document in the <see cref="LayoutManager"/>'s document area.
/// Can optionally be converted into a dockable pane.
/// </summary>
public class LayoutItem : TabItemEdit
{
    static LayoutItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(LayoutItem),
            new FrameworkPropertyMetadata(typeof(LayoutItem)));
    }
}
