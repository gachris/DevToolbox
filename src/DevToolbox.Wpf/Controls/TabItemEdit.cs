using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a customizable tab item with editing capabilities, including an icon and a style for a close button.
/// </summary>
public partial class TabItemEdit : TabItem
{
    #region Fields/Consts

    /// <summary>
    /// Dependency property for the icon displayed on the tab item.
    /// </summary>
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(ImageSource), typeof(TabItemEdit), new PropertyMetadata(null));

    /// <summary>
    /// Dependency property for the style of the close tab button.
    /// </summary>
    public static readonly DependencyProperty CloseTabButtonStyleProperty =
        DependencyProperty.Register(nameof(CloseTabButtonStyle), typeof(Style), typeof(TabItemEdit), new PropertyMetadata(null));

    /// <summary>
    /// Dependency property for the corner radius of the tab item.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty =
        Border.CornerRadiusProperty.AddOwner(typeof(TabItemEdit));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the style of the close tab button.
    /// </summary>
    public Style CloseTabButtonStyle
    {
        get => (Style)GetValue(CloseTabButtonStyleProperty);
        set => SetValue(CloseTabButtonStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon displayed on the tab item.
    /// </summary>
    public ImageSource Icon
    {
        get => (ImageSource)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the corner radius of the tab item.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion

    /// <summary>
    /// Static constructor to override the default style for the TabItemEdit control.
    /// </summary>
    static TabItemEdit()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TabItemEdit), new FrameworkPropertyMetadata(typeof(TabItemEdit)));
    }
}
