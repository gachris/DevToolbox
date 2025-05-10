using System;
using System.Threading.Tasks;
using DevToolbox.Core.Contracts;
using DevToolbox.Core.Media;
using DevToolbox.Wpf.Media;

namespace DevToolbox.Wpf.Services;

/// <summary>
/// Manages application UI settings such as theme, persisting them via local settings.
/// </summary>
public class AppUISettings : IAppUISettings
{
    #region Fields/Consts

    private const string SettingsKey = "AppBackgroundRequestedTheme";

    private readonly ILocalSettingsService _localSettingsService;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the currently applied <see cref="Theme"/>.
    /// </summary>
    public Theme Theme { get; private set; }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="AppUISettings"/> class.
    /// </summary>
    /// <param name="localSettingsService">
    /// The service used to read and write local application settings.
    /// </param>
    public AppUISettings(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    #region Methods

    /// <summary>
    /// Asynchronously initializes UI settings by loading persisted values and applying them.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when initialization is finished.</returns>
    public async Task InitializeAsync()
    {
        FontSizeManager.TextScaleEnabled = true;

        var themeName = await _localSettingsService.ReadSettingAsync<string>(SettingsKey);

        if (!Enum.TryParse(themeName, out Theme cacheTheme))
        {
            cacheTheme = Theme.Default;
        }

        Theme = cacheTheme;
        ThemeManager.RequestedTheme = ThemeToElementTheme(Theme);
    }

    /// <summary>
    /// Asynchronously sets and persists the application theme.
    /// </summary>
    /// <param name="theme">The <see cref="Theme"/> to apply.</param>
    /// <returns>A <see cref="Task"/> that completes when the theme has been set and saved.</returns>
    public async Task SetThemeAsync(Theme theme)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey, theme.ToString());

        Theme = theme;
        ThemeManager.RequestedTheme = ThemeToElementTheme(Theme);
    }

    private static ElementTheme ThemeToElementTheme(Theme theme)
    {
        return theme switch
        {
            Theme.Default => ElementTheme.WindowsDefault,
            Theme.Light => ElementTheme.Light,
            Theme.Dark => ElementTheme.Dark,
            Theme.None => ElementTheme.Default,
            _ => throw new Exception(),
        };
    }

    #endregion
}