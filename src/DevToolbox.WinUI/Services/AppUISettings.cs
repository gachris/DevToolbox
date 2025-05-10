using System;
using System.Threading.Tasks;
using DevToolbox.Core.Contracts;
using DevToolbox.Core.Media;
using DevToolbox.WinUI.Contracts;
using Microsoft.UI.Xaml;

namespace DevToolbox.WinUI.Services;

/// <summary>
/// Manages the application UI settings, including loading, saving, and applying the theme.
/// </summary>
public class AppUISettings : IAppUISettings
{
    #region Fields/Consts

    private const string SettingsKey = "AppBackgroundRequestedTheme";

    private readonly ILocalSettingsService _localSettingsService;
    private readonly IThemeSelectorService _themeSelectorService;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the current <see cref="Theme"/> of the application UI.
    /// </summary>
    public Theme Theme { get; private set; }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="AppUISettings"/> class with specified services.
    /// </summary>
    /// <param name="localSettingsService">
    /// The service used for reading and writing local settings.
    /// </param>
    /// <param name="themeSelectorService">
    /// The service used for applying the selected <see cref="Theme"/> to the UI.
    /// </param>
    public AppUISettings(
        ILocalSettingsService localSettingsService,
        IThemeSelectorService themeSelectorService)
    {
        _localSettingsService = localSettingsService;
        _themeSelectorService = themeSelectorService;
    }

    #region Methods

    /// <summary>
    /// Loads the saved theme from local settings and applies it to the application.
    /// </summary>
    /// <returns>
    /// A task that completes once initialization and theme application are finished.
    /// </returns>
    public async Task InitializeAsync()
    {
        var themeName = await _localSettingsService.ReadSettingAsync<string>(SettingsKey);

        if (!Enum.TryParse(themeName, out Theme cacheTheme))
        {
            cacheTheme = Theme.Default;
        }

        Theme = cacheTheme;
        await _themeSelectorService.SetThemeAsync(ThemeToElementTheme(Theme));
    }

    /// <summary>
    /// Saves and applies the specified <see cref="Theme"/> to the application.
    /// </summary>
    /// <param name="theme">
    /// The <see cref="Theme"/> to set and persist.
    /// </param>
    /// <returns>
    /// A task that completes once the theme has been saved and applied.
    /// </returns>
    public async Task SetThemeAsync(Theme theme)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey, theme.ToString());

        Theme = theme;

        await _themeSelectorService.SetThemeAsync(ThemeToElementTheme(Theme));
    }

    /// <summary>
    /// Maps the <see cref="Theme"/> enum to the corresponding <see cref="ElementTheme"/>.
    /// </summary>
    /// <param name="theme">
    /// The <see cref="Theme"/> value to map.
    /// </param>
    /// <returns>
    /// The corresponding <see cref="ElementTheme"/> value.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown if the <paramref name="theme"/> is invalid or unsupported.
    /// </exception>
    private static ElementTheme ThemeToElementTheme(Theme theme)
    {
        return theme switch
        {
            Theme.Default => ElementTheme.Default,
            Theme.Light => ElementTheme.Light,
            Theme.Dark => ElementTheme.Dark,
            _ => throw new Exception($"Unsupported theme: {theme}"),
        };
    }

    #endregion
}
