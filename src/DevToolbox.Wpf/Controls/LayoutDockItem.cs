using System.Windows;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A tab item used within <see cref="LayoutDockItemsControl"/> that docking in the <see cref="LayoutManager"/>.
/// </summary>
public class LayoutDockItem : TabItemEdit
{
    static LayoutDockItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(LayoutDockItem),
            new FrameworkPropertyMetadata(typeof(LayoutDockItem)));
    }
}
