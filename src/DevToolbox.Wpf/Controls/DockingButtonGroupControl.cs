using System.Windows;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a group of <see cref="DockingButtonGroupItem"/>s arranged either horizontally or vertically,
/// typically used to present a set of docking commands or options in a unified UI element.
/// </summary>
public class DockingButtonGroupControl : ItemsControl
{
    #region Fields/Consts

    /// <summary>
    /// Identifies the <see cref="Orientation"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(DockingButtonGroupControl),
            new FrameworkPropertyMetadata(Orientation.Horizontal));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the orientation in which the buttons are laid out.
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    #endregion

    static DockingButtonGroupControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DockingButtonGroupControl),
            new FrameworkPropertyMetadata(typeof(DockingButtonGroupControl)));
    }

    #region Methods Override

    /// <summary>
    /// Creates or identifies the element that is used to display the given item.
    /// </summary>
    /// <returns>A new <see cref="DockingButtonGroupItem"/> to contain the item.</returns>
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new DockingButtonGroupItem();
    }

    #endregion
}
