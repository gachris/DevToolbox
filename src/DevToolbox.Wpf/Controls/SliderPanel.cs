using System.Windows;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a panel that contains a slider control.
/// </summary>
public partial class SliderPanel : ContentControl
{
    #region Fields/Consts

    /// <summary>
    /// Identifies the <see cref="SliderControl"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SliderControlProperty =
        DependencyProperty.Register("SliderControl", typeof(SliderControl), typeof(SliderPanel), new FrameworkPropertyMetadata());

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the <see cref="SliderControl"/> associated with this panel.
    /// </summary>
    public SliderControl SliderControl
    {
        get => (SliderControl)GetValue(SliderControlProperty);
        set => SetValue(SliderControlProperty, value);
    }

    #endregion

    /// <summary>
    /// Initializes the static members of the <see cref="SliderPanel"/> class.
    /// </summary>
    static SliderPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SliderPanel), new FrameworkPropertyMetadata(typeof(SliderPanel)));
    }
}
