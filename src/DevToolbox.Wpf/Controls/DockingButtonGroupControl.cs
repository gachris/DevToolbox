using System.Windows;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Controls;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class DockingButtonGroupControl : ItemsControl
{
    #region Fields/Consts

    public static readonly DependencyProperty OrientationProperty = 
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(DockingButtonGroupControl), new FrameworkPropertyMetadata(default(Orientation)));

    #endregion

    #region Properties

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    #endregion

    static DockingButtonGroupControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DockingButtonGroupControl), new FrameworkPropertyMetadata(typeof(DockingButtonGroupControl)));
    }

    #region Methods Override

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new DockingButtonGroupItem();
    }

    #endregion
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
