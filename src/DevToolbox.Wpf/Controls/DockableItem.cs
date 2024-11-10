using System.Windows;

namespace DevToolbox.Wpf.Controls;

public class DockableItem : TabItemEdit
{
    static DockableItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DockableItem), new FrameworkPropertyMetadata(typeof(DockableItem)));
    }
}
