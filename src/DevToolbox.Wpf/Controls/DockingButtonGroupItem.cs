using System.Windows;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Controls;

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