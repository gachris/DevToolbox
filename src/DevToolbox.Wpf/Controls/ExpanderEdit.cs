using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A custom Expander control that supports additional mouse interaction events for expanding or collapsing.
/// This control includes properties for specifying the style of the toggle button and handling different mouse event types.
/// </summary>
[TemplatePart(Name = "PART_HeaderSite", Type = typeof(ContentPresenter))]
public partial class ExpanderEdit : Expander
{
    #region Fields/Consts

    /// <summary>
    /// Stores the reference to the header content of the Expander.
    /// </summary>
    private ContentPresenter? _headerSite;

    /// <summary>
    /// DependencyProperty for defining the type of mouse events that trigger expand/collapse behavior.
    /// </summary>
    public static readonly DependencyProperty ExpanderMouseEventTypeProperty =
        DependencyProperty.Register("ExpanderMouseEventType", typeof(ExpanderMouseEventType), typeof(ExpanderEdit), new PropertyMetadata(default(ExpanderMouseEventType)));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the type of mouse event (click or double-click) that controls the expanding/collapsing of the Expander.
    /// </summary>
    public ExpanderMouseEventType ExpanderMouseEventType
    {
        get => (ExpanderMouseEventType)GetValue(ExpanderMouseEventTypeProperty);
        set => SetValue(ExpanderMouseEventTypeProperty, value);
    }

    #endregion

    /// <summary>
    /// Static constructor to override the default style key for ExpanderEdit.
    /// </summary>
    static ExpanderEdit() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpanderEdit), new FrameworkPropertyMetadata(typeof(ExpanderEdit)));

    #region Methods Override

    /// <summary>
    /// Overrides the OnApplyTemplate method to retrieve and attach event handlers to template parts.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _headerSite = Template.FindName("PART_HeaderSite", this) as ContentPresenter;
        _headerSite?.AddHandler(PreviewMouseDownEvent, new MouseButtonEventHandler(OnMouseDoubleClick), true);
    }

    /// <summary>
    /// Handles mouse events for expanding or collapsing the Expander based on the specified <see cref="ExpanderMouseEventType"/>.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Mouse event arguments, used to determine the click count.</param>
    private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        switch (ExpanderMouseEventType)
        {
            // Handle single click to toggle expand/collapse
            case ExpanderMouseEventType.Click when e.ClickCount == 1:
                SetCurrentValue(IsExpandedProperty, !IsExpanded);
                break;
            // Handle double-click to toggle expand/collapse
            case ExpanderMouseEventType.DoubleClick when e.ClickCount > 1:
                SetCurrentValue(IsExpandedProperty, !IsExpanded);
                break;
            default:
                break;
        }
    }

    #endregion
}