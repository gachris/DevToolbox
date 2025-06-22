using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;

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
    public static readonly DependencyProperty HeaderTemplateProperty = HeaderedContentControl.HeaderTemplateProperty.AddOwner(typeof(WindowEx));

    /// <summary>
    /// Identifies the Header dependency property.
    /// </summary>
    public static readonly DependencyProperty HeaderProperty = HeaderedContentControl.HeaderProperty.AddOwner(typeof(WindowEx));

    /// <summary>
    /// Identifies the ShowIcon dependency property.
    /// </summary>
    public static readonly DependencyProperty ShowIconProperty =
        DependencyProperty.Register(nameof(ShowIcon), typeof(bool), typeof(WindowEx), new PropertyMetadata(true));

    /// <summary>
    /// Identifies the ShowTitle dependency property.
    /// </summary>
    public static readonly DependencyProperty ShowTitleProperty =
        DependencyProperty.Register(nameof(ShowTitle), typeof(bool), typeof(WindowEx), new PropertyMetadata(true));

    /// <summary>
    /// Identifies the ShowBackButton dependency property.
    /// </summary>
    public static readonly DependencyProperty ShowBackButtonProperty =
        DependencyProperty.Register(nameof(ShowBackButton), typeof(bool), typeof(WindowEx), new PropertyMetadata(false));

    /// <summary>
    /// Identifies the TitleForeground dependency property.
    /// </summary>
    public static readonly DependencyProperty TitleTextBlockStyleProperty =
        DependencyProperty.Register(nameof(TitleTextBlockStyle), typeof(Style), typeof(WindowEx), new FrameworkPropertyMetadata(default));

    /// <summary>
    /// Identifies the ImageStyle dependency property.
    /// </summary>
    public static readonly DependencyProperty ImageStyleProperty =
        DependencyProperty.Register(nameof(ImageStyle), typeof(Style), typeof(WindowEx), new PropertyMetadata(default));

    /// <summary>
    /// Identifies the HeaderedContentControlStyle dependency property.
    /// </summary>
    public static readonly DependencyProperty HeaderedContentControlStyleProperty =
        DependencyProperty.Register(nameof(HeaderedContentControlStyle), typeof(Style), typeof(WindowEx), new FrameworkPropertyMetadata(default));

    /// <summary>
    /// Identifies the BackButtonCommand dependency property.
    /// </summary>
    public static readonly DependencyProperty BackButtonCommandProperty =
        DependencyProperty.Register(nameof(BackButtonCommand), typeof(ICommand), typeof(WindowEx), new PropertyMetadata(null));

    /// <summary>
    /// Identifies the BackButtonCommandParameter dependency property.
    /// </summary>
    public static readonly DependencyProperty BackButtonCommandParameterProperty =
        DependencyProperty.Register(nameof(BackButtonCommandParameter), typeof(object), typeof(WindowEx), new PropertyMetadata(null));

    /// <summary>
    /// Identifies the IsTitleBarVisible attached dependency property.
    /// </summary>
    public static readonly DependencyProperty IsTitleBarVisibleProperty =
        DependencyProperty.RegisterAttached(nameof(IsTitleBarVisible), typeof(bool), typeof(WindowEx), new PropertyMetadata(true));

    /// <summary>
    /// Identifies the HitTestResult attached dependency property.
    /// </summary>
    public static readonly DependencyProperty HitTestResultProperty =
        DependencyProperty.RegisterAttached(nameof(HitTestResult), typeof(HitTestResult), typeof(WindowEx), new PropertyMetadata(default(HitTestResult)));

    #endregion

    #region Properties

    /// <summary>
    /// Gets the key for the Chrome Window style resource.
    /// </summary>
    public static ComponentResourceKey ChromeStyleKey => new(typeof(WindowEx), nameof(ChromeStyleKey));

    /// <summary>
    /// Gets the key for the Back button style resource.
    /// </summary>
    public static ComponentResourceKey BackButtonStyleKey => new(typeof(WindowEx), nameof(BackButtonStyleKey));

    /// <summary>
    /// Gets the key for the Help button style resource.
    /// </summary>
    public static ComponentResourceKey HelpButtonStyleKey => new(typeof(WindowEx), nameof(HelpButtonStyleKey));

    /// <summary>
    /// Gets the key for the Minimize button style resource.
    /// </summary>
    public static ComponentResourceKey MinimizeButtonStyleKey => new(typeof(WindowEx), nameof(MinimizeButtonStyleKey));

    /// <summary>
    /// Gets the key for the Maximize button style resource.
    /// </summary>
    public static ComponentResourceKey MaximizeButtonStyleKey => new(typeof(WindowEx), nameof(MaximizeButtonStyleKey));

    /// <summary>
    /// Gets the key for the Close button style resource.
    /// </summary>
    public static ComponentResourceKey CloseButtonStyleKey => new(typeof(WindowEx), nameof(CloseButtonStyleKey));

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
    /// Gets or sets a value indicating whether the window title should be displayed.
    /// </summary>
    public bool ShowTitle
    {
        get => (bool)GetValue(ShowTitleProperty);
        set => SetValue(ShowTitleProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the window back button should be displayed.
    /// </summary>
    public bool ShowBackButton
    {
        get => (bool)GetValue(ShowBackButtonProperty);
        set => SetValue(ShowBackButtonProperty, value);
    }

    /// <summary>
    /// Gets or sets the window title style.
    /// </summary>
    public Style TitleTextBlockStyle
    {
        get => (Style)GetValue(TitleTextBlockStyleProperty);
        set => SetValue(TitleTextBlockStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the style for the window icon.
    /// </summary>
    public Style ImageStyle
    {
        get => (Style)GetValue(ImageStyleProperty);
        set => SetValue(ImageStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to be executed when the back button is clicked.
    /// </summary>
    public ICommand BackButtonCommand
    {
        get => (ICommand)GetValue(BackButtonCommandProperty);
        set => SetValue(BackButtonCommandProperty, value);
    }
    /// <summary>
    /// Gets or sets the parameter to be passed to the back button command.
    /// </summary>
    public object BackButtonCommandParameter
    {
        get => GetValue(BackButtonCommandParameterProperty);
        set => SetValue(BackButtonCommandParameterProperty, value);
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
        DefaultStyleKeyProperty.OverrideMetadata(handlerType, new FrameworkPropertyMetadata(ChromeStyleKey));

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

        Chrome = new WindowChrome
        {
            UseAeroCaptionButtons = false,
            GlassFrameThickness = new Thickness(-1),
            CaptionHeight = 48,
            NonClientFrameEdges = NonClientFrameEdges.None,
        };

        var windowExBehavior = new WindowExBehavior();
        WindowExBehavior.SetWindowExBehavior(this, windowExBehavior);

        SetResourceReference(StyleProperty, ChromeStyleKey);
    }

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