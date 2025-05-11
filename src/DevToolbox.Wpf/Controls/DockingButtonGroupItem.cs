using System.Windows;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Controls;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class DockingButtonGroupItem : ItemsControl
{
    #region Properties

    public Dock Dock { get; internal set; }

    #endregion

    #region Methods Override

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new DockingButton();
    }

    #endregion
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member