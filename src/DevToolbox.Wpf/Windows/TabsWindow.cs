using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using DevToolbox.Wpf.Controls;
using DevToolbox.Wpf.Windows.Effects;

namespace DevToolbox.Wpf.Windows;

/// <summary>
/// Represents a window that contains a tab control with additional customization options.
/// </summary>
[ContentProperty(nameof(Items))]
public class TabsWindow : WindowEx
{
    private TabControlEdit? _tabControl;
    /// <summary>
    /// Identifies the <see cref="ItemsSource"/> dependency property.
    /// This property binds a collection of items to the TabControl.
    /// </summary>
    public static readonly DependencyProperty ItemsSourceProperty =
        TabControlEdit.ItemsSourceProperty.AddOwner(typeof(TabsWindow));

    /// <summary>
    /// Identifies the <see cref="ItemContainerStyle"/> dependency property.
    /// This property specifies the style for item containers within the TabControl.
    /// </summary>
    public static readonly DependencyProperty ItemContainerStyleProperty =
        TabControlEdit.ItemContainerStyleProperty.AddOwner(typeof(TabsWindow));

    /// <summary>
    /// Identifies the <see cref="ItemContainerStyleSelector"/> dependency property.
    /// This property allows for a custom style selector to determine the style of item containers in the TabControl.
    /// </summary>
    public static readonly DependencyProperty ItemContainerStyleSelectorProperty =
        TabControlEdit.ItemContainerStyleSelectorProperty.AddOwner(typeof(TabsWindow));

    /// <summary>
    /// Identifies the <see cref="ItemTemplate"/> dependency property.
    /// This property defines the data template used to display items in the TabControl.
    /// </summary>
    public static readonly DependencyProperty ItemTemplateProperty =
        TabControlEdit.ItemTemplateProperty.AddOwner(typeof(TabsWindow));

    /// <summary>
    /// Identifies the <see cref="ItemTemplateSelector"/> dependency property.
    /// This property allows for a custom data template selector to customize the display of items in the TabControl.
    /// </summary>
    public static readonly DependencyProperty ItemTemplateSelectorProperty =
        TabControlEdit.ItemTemplateSelectorProperty.AddOwner(typeof(TabsWindow));

    /// <summary>
    /// Identifies the <see cref="ShowAddTabButton"/> dependency property.
    /// This property indicates whether the button for adding a new tab should be visible.
    /// </summary>
    public static readonly DependencyProperty ShowAddTabButtonProperty =
        TabControlEdit.ShowAddTabButtonProperty.AddOwner(typeof(TabsWindow));

    /// <summary>
    /// Identifies the <see cref="SwapTabsButtonShowMode"/> dependency property.
    /// This property controls the visibility mode of the button used for swapping tabs.
    /// </summary>
    public static readonly DependencyProperty SwapTabsButtonShowModeProperty =
        TabControlEdit.SwapTabsButtonShowModeProperty.AddOwner(typeof(TabsWindow));

    /// <summary>
    /// Identifies the <see cref="TabPanelViewMode"/> dependency property.
    /// This property sets the view mode for the tab panel within the TabControl.
    /// </summary>
    public static readonly DependencyProperty TabPanelViewModeProperty =
        TabControlEdit.TabPanelViewModeProperty.AddOwner(typeof(TabsWindow));

    /// <summary>
    /// Identifies the <see cref="CloseButtonShowMode"/> dependency property.
    /// This property determines the visibility mode of the close button for each tab.
    /// </summary>
    public static readonly DependencyProperty CloseButtonShowModeProperty =
        TabControlEdit.CloseButtonShowModeProperty.AddOwner(typeof(TabsWindow));

    /// <summary>
    /// Identifies the <see cref="TabPanelHorizontalAlignment"/> dependency property.
    /// This property specifies the horizontal alignment of the tab panel within the TabControl.
    /// </summary>
    public static readonly DependencyProperty TabPanelHorizontalAlignmentProperty =
        TabControlEdit.TabPanelHorizontalAlignmentProperty.AddOwner(typeof(TabsWindow));

    /// <summary>
    /// Identifies the <see cref="TabPanelVerticalAlignment"/> dependency property.
    /// This property specifies the vertical alignment of the tab panel within the TabControl.
    /// </summary>
    public static readonly DependencyProperty TabPanelVerticalAlignmentProperty =
        TabControlEdit.TabPanelVerticalAlignmentProperty.AddOwner(typeof(TabsWindow));

    /// <summary>
    /// Identifies the <see cref="CloseTabControlButtonStyle"/> dependency property.
    /// This property specifies the style for the button used to close the TabControl.
    /// </summary>
    public static readonly DependencyProperty CloseTabControlButtonStyleProperty =
        TabControlEdit.CloseTabControlButtonStyleProperty.AddOwner(typeof(TabsWindow));

    /// <summary>
    /// Identifies the <see cref="SwapTabsButtonStyle"/> dependency property.
    /// This property specifies the style for the button used to swap tabs within the TabControl.
    /// </summary>
    public static readonly DependencyProperty SwapTabsButtonStyleProperty =
        TabControlEdit.SwapTabsButtonStyleProperty.AddOwner(typeof(TabsWindow));

    /// <summary>
    /// Identifies the <see cref="AddTabButtonStyle"/> dependency property.
    /// This property specifies the style for the button that adds new tabs to the TabControl.
    /// </summary>
    public static readonly DependencyProperty AddTabButtonStyleProperty =
        TabControlEdit.AddTabButtonStyleProperty.AddOwner(typeof(TabsWindow));

    /// <summary>
    /// Identifies the <see cref="TabLeftScrollButtonStyle"/> dependency property.
    /// This property specifies the style for the button that scrolls left through the tabs.
    /// </summary>
    public static readonly DependencyProperty TabLeftScrollButtonStyleProperty =
        TabControlEdit.TabLeftScrollButtonStyleProperty.AddOwner(typeof(TabsWindow));

    /// <summary>
    /// Identifies the <see cref="TabBottomScrollButtonStyle"/> dependency property.
    /// This property specifies the style for the button that scrolls down through the tabs.
    /// </summary>
    public static readonly DependencyProperty TabBottomScrollButtonStyleProperty =
        TabControlEdit.TabBottomScrollButtonStyleProperty.AddOwner(typeof(TabsWindow));

    /// <summary>
    /// Identifies the <see cref="TabTopScrollButtonStyle"/> dependency property.
    /// This property specifies the style for the button that scrolls up through the tabs.
    /// </summary>
    public static readonly DependencyProperty TabTopScrollButtonStyleProperty =
        TabControlEdit.TabTopScrollButtonStyleProperty.AddOwner(typeof(TabsWindow));

    /// <summary>
    /// Identifies the <see cref="TabRightScrollButtonStyle"/> dependency property.
    /// This property specifies the style for the button that scrolls right through the tabs.
    /// </summary>
    public static readonly DependencyProperty TabRightScrollButtonStyleProperty =
        TabControlEdit.TabRightScrollButtonStyleProperty.AddOwner(typeof(TabsWindow));

    #region Properties

    /// <summary>
    /// Gets the <see cref="TabControlEdit"/> instance within the window, creating it if necessary.
    /// </summary>
    internal TabControlEdit? TabControl
    {
        get
        {
            if (_tabControl == null)
            {
                UpdateDefaultStyle();
                ApplyTemplate();
            }
            return _tabControl;
        }
    }

    /// <summary>
    /// Gets or sets the style for the item containers.
    /// </summary>
    public Style ItemContainerStyle
    {
        get => (Style)GetValue(ItemContainerStyleProperty);
        set => SetValue(ItemContainerStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the style selector for item containers.
    /// </summary>
    public Style ItemContainerStyleSelector
    {
        get => (Style)GetValue(ItemContainerStyleSelectorProperty);
        set => SetValue(ItemContainerStyleSelectorProperty, value);
    }

    /// <summary>
    /// Gets or sets the data template for items in the tab control.
    /// </summary>
    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the data template selector for items in the tab control.
    /// </summary>
    public DataTemplate ItemTemplateSelector
    {
        get => (DataTemplate)GetValue(ItemTemplateSelectorProperty);
        set => SetValue(ItemTemplateSelectorProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the add tab button is shown.
    /// </summary>
    public bool ShowAddTabButton
    {
        get => (bool)GetValue(ShowAddTabButtonProperty);
        set => SetValue(ShowAddTabButtonProperty, value);
    }

    /// <summary>
    /// Gets or sets the visibility mode for the close button on tabs.
    /// </summary>
    public CloseButtonShowMode CloseButtonShowMode
    {
        get => (CloseButtonShowMode)GetValue(CloseButtonShowModeProperty);
        set => SetValue(CloseButtonShowModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal alignment of the tab panel.
    /// </summary>
    public HorizontalAlignment TabPanelHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(TabPanelHorizontalAlignmentProperty);
        set => SetValue(TabPanelHorizontalAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the view mode of the tab panel.
    /// </summary>
    public TabPanelViewMode TabPanelViewMode
    {
        get => (TabPanelViewMode)GetValue(TabPanelViewModeProperty);
        set => SetValue(TabPanelViewModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the visibility mode for the swap tabs button.
    /// </summary>
    public SwapTabsButtonShowMode SwapTabsButtonShowMode
    {
        get => (SwapTabsButtonShowMode)GetValue(SwapTabsButtonShowModeProperty);
        set => SetValue(SwapTabsButtonShowModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical alignment of the tab panel.
    /// </summary>
    public VerticalAlignment TabPanelVerticalAlignment
    {
        get => (VerticalAlignment)GetValue(TabPanelVerticalAlignmentProperty);
        set => SetValue(TabPanelVerticalAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the style for the add tab button.
    /// </summary>
    public Style AddTabButtonStyle
    {
        get => (Style)GetValue(AddTabButtonStyleProperty);
        set => SetValue(AddTabButtonStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the style for the swap tabs button.
    /// </summary>
    public Style SwapTabsButtonStyle
    {
        get => (Style)GetValue(SwapTabsButtonStyleProperty);
        set => SetValue(SwapTabsButtonStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the style for the close tab control button.
    /// </summary>
    public Style CloseTabControlButtonStyle
    {
        get => (Style)GetValue(CloseTabControlButtonStyleProperty);
        set => SetValue(CloseTabControlButtonStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the Tab Left Button style.
    /// </summary>
    [Description("Gets or sets the tab left button style")]
    [Category("ScrollButton")]
    public Style TabLeftScrollButtonStyle
    {
        get => (Style)GetValue(TabLeftScrollButtonStyleProperty);
        set => SetValue(TabLeftScrollButtonStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the Tab Right Button style.
    /// </summary>
    [Description("Gets or sets the tab right button style")]
    [Category("ScrollButton")]
    public Style TabRightScrollButtonStyle
    {
        get => (Style)GetValue(TabRightScrollButtonStyleProperty);
        set => SetValue(TabRightScrollButtonStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the Tab Top Button style.
    /// </summary>
    [Description("Gets or sets the tab top button style")]
    [Category("ScrollButton")]
    public Style TabTopScrollButtonStyle
    {
        get => (Style)GetValue(TabTopScrollButtonStyleProperty);
        set => SetValue(TabTopScrollButtonStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the Tab Bottom Button style.
    /// </summary>
    [Description("Gets or sets the tab bottom button style")]
    [Category("ScrollButton")]
    public Style TabBottomScrollButtonStyle
    {
        get => (Style)GetValue(TabBottomScrollButtonStyleProperty);
        set => SetValue(TabBottomScrollButtonStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the items source for the tab control.
    /// </summary>
    [Bindable(true)]
    [Category("Content")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets the collection of items in the tab control.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    [Bindable(true)]
    public ItemCollection Items => TabControl?.Items!;

    #endregion

    #region Constructor

    /// <summary>
    /// Static constructor that overrides the default style key for the <see cref="TabsWindow"/>.
    /// </summary>
    static TabsWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TabsWindow), new FrameworkPropertyMetadata(typeof(TabsWindow)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TabsWindow"/> class.
    /// </summary>
    public TabsWindow()
    {
        Chrome.CaptionHeight = 44;

        WindowBehavior.SetWindowEffect(this, new Tabbed());
    }

    #endregion

    #region Methods

    /// <summary>
    /// Called when the control's template is applied. Initializes the tab control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _tabControl = Template.FindName("PART_TabControl", this) as TabControlEdit;
    }

    /// <summary>
    /// Gets the content of the currently selected tab.
    /// </summary>
    public new object Content => TabControl?.SelectedItem!;

    #endregion
}
