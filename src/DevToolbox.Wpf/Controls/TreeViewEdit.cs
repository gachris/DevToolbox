using System.Windows;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a customizable tree view that allows for editing functionality 
/// and supports custom toggle button styles.
/// </summary>
public partial class TreeViewEdit : TreeView
{
    #region Fields/Consts

    /// <summary>
    /// Identifies the <see cref="ToogleButtonStyle"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ToogleButtonStyleProperty =
        DependencyProperty.Register("ToogleButtonStyle", typeof(Style), typeof(TreeViewEdit));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the style for the toggle buttons in the tree view items.
    /// </summary>
    public Style ToogleButtonStyle
    {
        get => (Style)GetValue(ToogleButtonStyleProperty);
        set => SetValue(ToogleButtonStyleProperty, value);
    }

    #endregion

    /// <summary>
    /// Initializes the <see cref="TreeViewEdit"/> class.
    /// </summary>
    static TreeViewEdit()
        => DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeViewEdit), new FrameworkPropertyMetadata(typeof(TreeViewEdit)));

    #region Methods

    /// <summary>
    /// Gets the container for the item override.
    /// </summary>
    /// <returns>A new instance of <see cref="TreeViewItemEdit"/>.</returns>
    protected override DependencyObject GetContainerForItemOverride() => new TreeViewItemEdit();

    #endregion
}
