using System.Windows;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a container for a group of <see cref="DockingButton"/> instances,
/// providing a common <see cref="Dock"/> direction for all buttons it contains.
/// </summary>
public class DockingButtonGroupItem : ItemsControl
{
    #region Properties

    /// <summary>
    /// Gets or sets the docking direction associated with this group item.
    /// Buttons within this group will inherit this <see cref="Dock"/> value.
    /// </summary>
    public Dock Dock { get; internal set; }

    #endregion

    #region Methods Override

    /// <summary>
    /// Creates or identifies the element that is used to display the given item.
    /// </summary>
    /// <returns>
    /// A new <see cref="DockingButton"/> to contain the item.
    /// </returns>
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new DockingButton();
    }

    #endregion
}
