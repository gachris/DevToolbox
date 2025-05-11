using System.Windows;

namespace DevToolbox.Wpf.Controls;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class DockableItem : TabItemEdit
{
    static DockableItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DockableItem), new FrameworkPropertyMetadata(typeof(DockableItem)));
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
