using System.Windows;
using System.Windows.Media;
using DevToolbox.Wpf.Helpers;
using Microsoft.Win32;

namespace DevToolbox.Wpf.Media;

/// <summary>
/// Provides immersive UI colors and brushes, and exposes them as WPF resource keys like SystemColors.
/// </summary>
public static class AccentColors
{
    #region Properties

    /// <summary>
    /// The background color used in UI elements.
    /// </summary>
    public static Color Background { get; private set; } = default!;

    /// <summary>
    /// The foreground color used for text or primary content.
    /// </summary>
    public static Color Foreground { get; private set; } = default!;

    /// <summary>
    /// The darkest shade of the accent color.
    /// </summary>
    public static Color AccentDark3 { get; private set; } = default!;

    /// <summary>
    /// A darker shade of the accent color.
    /// </summary>
    public static Color AccentDark2 { get; private set; } = default!;

    /// <summary>
    /// A slightly darkened shade of the accent color.
    /// </summary>
    public static Color AccentDark1 { get; private set; } = default!;

    /// <summary>
    /// The base accent color used for highlighting and emphasis.
    /// </summary>
    public static Color Accent { get; private set; } = default!;

    /// <summary>
    /// A slightly lightened shade of the accent color.
    /// </summary>
    public static Color AccentLight1 { get; private set; } = default!;

    /// <summary>
    /// A lighter shade of the accent color.
    /// </summary>
    public static Color AccentLight2 { get; private set; } = default!;

    /// <summary>
    /// The lightest shade of the accent color.
    /// </summary>
    public static Color AccentLight3 { get; private set; } = default!;

    /// <summary>
    /// A complementary color used to contrast with the accent color.
    /// </summary>
    public static Color Complement { get; private set; } = default!;

    /// <summary>
    /// Brush for the background color.
    /// </summary>
    public static Brush BackgroundBrush { get; private set; } = default!;

    /// <summary>
    /// Brush for the foreground color.
    /// </summary>
    public static Brush ForegroundBrush { get; private set; } = default!;

    /// <summary>
    /// Brush for the darkest accent shade.
    /// </summary>
    public static Brush AccentDark3Brush { get; private set; } = default!;

    /// <summary>
    /// Brush for the darker accent shade.
    /// </summary>
    public static Brush AccentDark2Brush { get; private set; } = default!;

    /// <summary>
    /// Brush for the slightly darkened accent shade.
    /// </summary>
    public static Brush AccentDark1Brush { get; private set; } = default!;

    /// <summary>
    /// Brush for the base accent color.
    /// </summary>
    public static Brush AccentBrush { get; private set; } = default!;

    /// <summary>
    /// Brush for the slightly lightened accent shade.
    /// </summary>
    public static Brush AccentLight1Brush { get; private set; } = default!;

    /// <summary>
    /// Brush for the lighter accent shade.
    /// </summary>
    public static Brush AccentLight2Brush { get; private set; } = default!;

    /// <summary>
    /// Brush for the lightest accent shade.
    /// </summary>
    public static Brush AccentLight3Brush { get; private set; } = default!;

    /// <summary>
    /// Brush for the complementary accent color.
    /// </summary>
    public static Brush ComplementBrush { get; private set; } = default!;

    #endregion

    #region ResourceKeys

    /// <summary>
    /// Resource key for the Background color.
    /// </summary>
    public static ComponentResourceKey BackgroundColorKey { get; } =
        new ComponentResourceKey(typeof(AccentColors), nameof(Background));

    /// <summary>
    /// Resource key for the Background brush.
    /// </summary>
    public static ComponentResourceKey BackgroundBrushKey { get; } =
        new ComponentResourceKey(typeof(AccentColors), nameof(BackgroundBrush));

    /// <summary>
    /// Resource key for the Foreground color.
    /// </summary>
    public static ComponentResourceKey ForegroundColorKey { get; } =
        new ComponentResourceKey(typeof(AccentColors), nameof(Foreground));

    /// <summary>
    /// Resource key for the Foreground brush.
    /// </summary>
    public static ComponentResourceKey ForegroundBrushKey { get; } =
        new ComponentResourceKey(typeof(AccentColors), nameof(ForegroundBrush));

    /// <summary>
    /// Resource key for the darkest accent shade color.
    /// </summary>
    public static ComponentResourceKey AccentDark3ColorKey { get; } =
        new ComponentResourceKey(typeof(AccentColors), nameof(AccentDark3));

    /// <summary>
    /// Resource key for the darkest accent shade brush.
    /// </summary>
    public static ComponentResourceKey AccentDark3BrushKey { get; } =
        new ComponentResourceKey(typeof(AccentColors), nameof(AccentDark3Brush));

    /// <summary>
    /// Resource key for the darker accent shade color.
    /// </summary>
    public static ComponentResourceKey AccentDark2ColorKey { get; } =
        new ComponentResourceKey(typeof(AccentColors), nameof(AccentDark2));

    /// <summary>
    /// Resource key for the darker accent shade brush.
    /// </summary>
    public static ComponentResourceKey AccentDark2BrushKey { get; } =
        new ComponentResourceKey(typeof(AccentColors), nameof(AccentDark2Brush));

    /// <summary>
    /// Resource key for the slightly darkened accent shade color.
    /// </summary>
    public static ComponentResourceKey AccentDark1ColorKey { get; } =
        new ComponentResourceKey(typeof(AccentColors), nameof(AccentDark1));

    /// <summary>
    /// Resource key for the slightly darkened accent shade brush.
    /// </summary>
    public static ComponentResourceKey AccentDark1BrushKey { get; } =
        new ComponentResourceKey(typeof(AccentColors), nameof(AccentDark1Brush));

    /// <summary>
    /// Resource key for the base accent color.
    /// </summary>
    public static ComponentResourceKey AccentColorKey { get; } =
        new ComponentResourceKey(typeof(AccentColors), nameof(Accent));

    /// <summary>
    /// Resource key for the base accent brush.
    /// </summary>
    public static ComponentResourceKey AccentBrushKey { get; } =
        new ComponentResourceKey(typeof(AccentColors), nameof(AccentBrush));

    /// <summary>
    /// Resource key for the slightly lightened accent shade color.
    /// </summary>
    public static ComponentResourceKey AccentLight1ColorKey { get; } =
        new ComponentResourceKey(typeof(AccentColors), nameof(AccentLight1));

    /// <summary>
    /// Resource key for the slightly lightened accent shade brush.
    /// </summary>
    public static ComponentResourceKey AccentLight1BrushKey { get; } =
        new ComponentResourceKey(typeof(AccentColors), nameof(AccentLight1Brush));

    /// <summary>
    /// Resource key for the lighter accent shade color.
    /// </summary>
    public static ComponentResourceKey AccentLight2ColorKey { get; } =
        new ComponentResourceKey(typeof(AccentColors), nameof(AccentLight2));

    /// <summary>
    /// Resource key for the lighter accent shade brush.
    /// </summary>
    public static ComponentResourceKey AccentLight2BrushKey { get; } =
        new ComponentResourceKey(typeof(AccentColors), nameof(AccentLight2Brush));

    /// <summary>
    /// Resource key for the lightest accent shade color.
    /// </summary>
    public static ComponentResourceKey AccentLight3ColorKey { get; } =
        new ComponentResourceKey(typeof(AccentColors), nameof(AccentLight3));

    /// <summary>
    /// Resource key for the lightest accent shade brush.
    /// </summary>
    public static ComponentResourceKey AccentLight3BrushKey { get; } =
        new ComponentResourceKey(typeof(AccentColors), nameof(AccentLight3Brush));

    /// <summary>
    /// Resource key for the complementary accent color.
    /// </summary>
    public static ComponentResourceKey ComplementColorKey { get; } =
        new ComponentResourceKey(typeof(AccentColors), nameof(Complement));

    /// <summary>
    /// Resource key for the complementary accent brush.
    /// </summary>
    public static ComponentResourceKey ComplementBrushKey { get; } =
        new ComponentResourceKey(typeof(AccentColors), nameof(ComplementBrush));

    #endregion

    static AccentColors()
    {
        Update();
        SystemEvents.UserPreferenceChanged += (_, _) => Update();
    }

    private static void Update()
    {
        // Fetch latest values
        Background = AccentColorHelper.GetColor(UIColorType.Background);
        Foreground = AccentColorHelper.GetColor(UIColorType.Foreground);
        AccentDark3 = AccentColorHelper.GetColor(UIColorType.AccentDark3);
        AccentDark2 = AccentColorHelper.GetColor(UIColorType.AccentDark2);
        AccentDark1 = AccentColorHelper.GetColor(UIColorType.AccentDark1);
        Accent = AccentColorHelper.GetColor(UIColorType.Accent);
        AccentLight1 = AccentColorHelper.GetColor(UIColorType.AccentLight1);
        AccentLight2 = AccentColorHelper.GetColor(UIColorType.AccentLight2);
        AccentLight3 = AccentColorHelper.GetColor(UIColorType.AccentLight3);
        Complement = AccentColorHelper.GetColor(UIColorType.Complement);

        // Create brushes
        BackgroundBrush = AccentColorHelper.GetBrush(UIColorType.Background);
        ForegroundBrush = AccentColorHelper.GetBrush(UIColorType.Foreground);
        AccentDark3Brush = AccentColorHelper.GetBrush(UIColorType.AccentDark3);
        AccentDark2Brush = AccentColorHelper.GetBrush(UIColorType.AccentDark2);
        AccentDark1Brush = AccentColorHelper.GetBrush(UIColorType.AccentDark1);
        AccentBrush = AccentColorHelper.GetBrush(UIColorType.Accent);
        AccentLight1Brush = AccentColorHelper.GetBrush(UIColorType.AccentLight1);
        AccentLight2Brush = AccentColorHelper.GetBrush(UIColorType.AccentLight2);
        AccentLight3Brush = AccentColorHelper.GetBrush(UIColorType.AccentLight3);
        ComplementBrush = AccentColorHelper.GetBrush(UIColorType.Complement);

        // Register in application resources for XAML lookup
        var resources = Application.Current?.Resources;
        if (resources != null)
        {
            resources[BackgroundColorKey] = Background;
            resources[BackgroundBrushKey] = BackgroundBrush;
            resources[ForegroundColorKey] = Foreground;
            resources[ForegroundBrushKey] = ForegroundBrush;
            resources[AccentDark3ColorKey] = AccentDark3;
            resources[AccentDark3BrushKey] = AccentDark3Brush;
            resources[AccentDark2ColorKey] = AccentDark2;
            resources[AccentDark2BrushKey] = AccentDark2Brush;
            resources[AccentDark1ColorKey] = AccentDark1;
            resources[AccentDark1BrushKey] = AccentDark1Brush;
            resources[AccentColorKey] = Accent;
            resources[AccentBrushKey] = AccentBrush;
            resources[AccentLight1ColorKey] = AccentLight1;
            resources[AccentLight1BrushKey] = AccentLight1Brush;
            resources[AccentLight2ColorKey] = AccentLight2;
            resources[AccentLight2BrushKey] = AccentLight2Brush;
            resources[AccentLight3ColorKey] = AccentLight3;
            resources[AccentLight3BrushKey] = AccentLight3Brush;
            resources[ComplementColorKey] = Complement;
            resources[ComplementBrushKey] = ComplementBrush;
        }
    }
}
