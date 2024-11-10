using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using DevToolbox.Wpf.Controls;

namespace DevToolbox.Wpf.Windows;

/// <summary>
/// Represents an extended window that provides additional functionality
/// and customization options beyond the standard Window class in WPF.
/// </summary>
public class WindowEx : Window
{
    #region Fields/Consts

    /// <summary>
    /// Identifies the HeaderTemplate dependency property.
    /// </summary>
    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(WindowEx), new FrameworkPropertyMetadata(null));

    /// <summary>
    /// Identifies the Header dependency property.
    /// </summary>
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register("Header", typeof(object), typeof(WindowEx), new FrameworkPropertyMetadata(null));

    /// <summary>
    /// Identifies the ShowIcon dependency property.
    /// </summary>
    public static readonly DependencyProperty ShowIconProperty =
        DependencyProperty.Register("ShowIcon", typeof(bool), typeof(WindowEx), new FrameworkPropertyMetadata(true));

    /// <summary>
    /// Identifies the IconTemplate dependency property.
    /// </summary>
    public static readonly DependencyProperty IconTemplateProperty =
        DependencyProperty.Register("IconTemplate", typeof(DataTemplate), typeof(WindowEx), new FrameworkPropertyMetadata(null));

    /// <summary>
    /// Identifies the IsTitleBarVisible attached dependency property.
    /// </summary>
    public static readonly DependencyProperty IsTitleBarVisibleProperty =
        DependencyProperty.RegisterAttached("IsTitleBarVisible", typeof(bool), typeof(WindowEx), new FrameworkPropertyMetadata(true));

    /// <summary>
    /// Identifies the HeaderedContentControlStyle dependency property.
    /// </summary>
    public static readonly DependencyProperty HeaderedContentControlStyleProperty =
        DependencyProperty.Register("HeaderedContentControlStyle", typeof(Style), typeof(WindowEx), new FrameworkPropertyMetadata(null));

    /// <summary>
    /// Identifies the HitTestResult attached dependency property.
    /// </summary>
    public static readonly DependencyProperty HitTestResultProperty =
        DependencyProperty.RegisterAttached("HitTestResult", typeof(HitTestResult), typeof(WindowEx), new PropertyMetadata(default(HitTestResult)));

    #endregion

    #region Properties

    /// <summary>
    /// Gets the key for the Help button style resource.
    /// </summary>
    public static ComponentResourceKey HelpButtonStyleKey => new(typeof(FontGlyph), nameof(HelpButtonStyleKey));

    /// <summary>
    /// Gets the key for the Minimize button style resource.
    /// </summary>
    public static ComponentResourceKey MinimizeButtonStyleKey => new(typeof(FontGlyph), nameof(MinimizeButtonStyleKey));

    /// <summary>
    /// Gets the key for the Maximize button style resource.
    /// </summary>
    public static ComponentResourceKey MaximizeButtonStyleKey => new(typeof(FontGlyph), nameof(MaximizeButtonStyleKey));

    /// <summary>
    /// Gets the key for the Close button style resource.
    /// </summary>
    public static ComponentResourceKey CloseButtonStyleKey => new(typeof(FontGlyph), nameof(CloseButtonStyleKey));

    /// <summary>
    /// Gets or sets the template for the window header.
    /// </summary>
    public DataTemplate HeaderTemplate
    {
        get => (DataTemplate)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the content of the window header.
    /// </summary>
    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the window icon should be displayed.
    /// </summary>
    public bool ShowIcon
    {
        get => (bool)GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    /// <summary>
    /// Gets or sets the template for the window icon.
    /// </summary>
    public DataTemplate IconTemplate
    {
        get => (DataTemplate)GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the title bar is visible.
    /// </summary>
    public bool IsTitleBarVisible
    {
        get => (bool)GetValue(IsTitleBarVisibleProperty);
        set => SetValue(IsTitleBarVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets the style for the headered content control.
    /// </summary>
    public Style HeaderedContentControlStyle
    {
        get => (Style)GetValue(HeaderedContentControlStyleProperty);
        set => SetValue(HeaderedContentControlStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the WindowChrome associated with the window.
    /// </summary>
    public WindowChrome Chrome
    {
        get => WindowChrome.GetWindowChrome(this);
        set => WindowChrome.SetWindowChrome(this, value);
    }

    #endregion

    static WindowEx()
    {
        var handlerType = typeof(WindowEx);
        DefaultStyleKeyProperty.OverrideMetadata(handlerType, new FrameworkPropertyMetadata(handlerType));

        // Register command bindings for window commands
        CommandManager.RegisterClassCommandBinding(handlerType, new CommandBinding(SystemCommands.MinimizeWindowCommand,
            (sender, e) => ((WindowEx)sender).WindowState = WindowState.Minimized, (sender, e) => e.CanExecute = sender is WindowEx));
        CommandManager.RegisterClassCommandBinding(handlerType, new CommandBinding(SystemCommands.MaximizeWindowCommand,
            (sender, e) => ((WindowEx)sender).WindowState = WindowState.Maximized, (sender, e) => e.CanExecute = sender is WindowEx));
        CommandManager.RegisterClassCommandBinding(handlerType, new CommandBinding(SystemCommands.RestoreWindowCommand,
            (sender, e) => ((WindowEx)sender).WindowState = WindowState.Normal, (sender, e) => e.CanExecute = sender is WindowEx));
        CommandManager.RegisterClassCommandBinding(handlerType, new CommandBinding(SystemCommands.CloseWindowCommand,
            (sender, e) => ((WindowEx)sender).Close(), (sender, e) => e.CanExecute = sender is WindowEx));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowEx"/> class.
    /// </summary>
    public WindowEx()
    {
        if (DesignerProperties.GetIsInDesignMode(this))
            return;

        // Attach a handler for when the WindowChrome property changes
        var dpdWindowChrome = DependencyPropertyDescriptor.FromProperty(WindowChrome.WindowChromeProperty, typeof(WindowEx));
        dpdWindowChrome?.AddValueChanged(this, (sender, e) =>
        {
            // TODO: Refresh WindowExBehaviour
        });

        // Configure WindowChrome settings
        Chrome = new WindowChrome
        {
            UseAeroCaptionButtons = false,
            GlassFrameThickness = new Thickness(-1),
            CaptionHeight = 33
        };

        var windowExBehaviour = new WindowExBehaviour();
        WindowExBehaviour.SetWindowExBehaviour(this, windowExBehaviour);
    }

    #region Methods Overrides

    /// <summary>
    /// Invoked when the window's source is initialized.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        InvalidateMeasure();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Sets the hit test result for the specified dependency object.
    /// </summary>
    /// <param name="element">The target dependency object.</param>
    /// <param name="value">The hit test result to set.</param>
    public static void SetHitTestResult(DependencyObject element, HitTestResult value) => element.SetValue(HitTestResultProperty, value);

    /// <summary>
    /// Gets the hit test result for the specified dependency object.
    /// </summary>
    /// <param name="element">The target dependency object.</param>
    /// <returns>The current hit test result.</returns>
    public static HitTestResult GetHitTestResult(DependencyObject element) => (HitTestResult)element.GetValue(HitTestResultProperty);

    #endregion
}