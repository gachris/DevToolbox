using System.Windows;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a group of <see cref="LayoutDockButtonGroupItem"/>s arranged either horizontally or vertically,
/// typically used to present a set of docking commands or options in a unified UI element.
/// </summary>
public class LayoutDockButtonGroupControl : ItemsControl
{
    #region Fields/Consts

    /// <summary>
    /// Identifies the <see cref="Orientation"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(LayoutDockButtonGroupControl),
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

    static LayoutDockButtonGroupControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(LayoutDockButtonGroupControl),
            new FrameworkPropertyMetadata(typeof(LayoutDockButtonGroupControl)));
    }

    #region Methods Override

    /// <summary>
    /// Creates or identifies the element that is used to display the given item.
    /// </summary>
    /// <returns>A new <see cref="LayoutDockButtonGroupItem"/> to contain the item.</returns>
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new LayoutDockButtonGroupItem();
    }

    #endregion
}
