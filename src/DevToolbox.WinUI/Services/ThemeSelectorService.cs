using System;
using System.Threading.Tasks;
using DevToolbox.Core.Contracts;
using DevToolbox.WinUI.Contracts;
using DevToolbox.WinUI.Helpers;
using Microsoft.UI.Xaml;

namespace DevToolbox.WinUI.Services;

/// <summary>
/// Implements <see cref="IThemeSelectorService"/>, providing methods to initialize,
/// apply, and persist the application's UI theme.
/// </summary>
public class ThemeSelectorService : IThemeSelectorService
{
    #region Fields/Consts

    private const string SettingsKey = "AppBackgroundRequestedTheme";

    private readonly ILocalSettingsService _localSettingsService;
    private readonly IAppWindowService _appWindowService;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the current <see cref="ElementTheme"/> being applied.
    /// </summary>
    public ElementTheme Theme { get; set; } = ElementTheme.Default;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeSelectorService"/> class
    /// with required services for settings persistence and window access.
    /// </summary>
    /// <param name="localSettingsService">
    /// The service used to read and write theme settings.
    /// </param>
    /// <param name="appWindowService">
    /// The service providing access to the main application window for theme updates.
    /// </param>
    public ThemeSelectorService(ILocalSettingsService localSettingsService, IAppWindowService appWindowService)
    {
        _localSettingsService = localSettingsService;
        _appWindowService = appWindowService;
    }

    #region IThemeSelectorService Implementation

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        Theme = await LoadThemeFromSettingsAsync();
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task SetThemeAsync(ElementTheme theme)
    {
        Theme = theme;

        await SetRequestedThemeAsync();
        await SaveThemeInSettingsAsync(Theme);
    }

    /// <inheritdoc/>
    public async Task SetRequestedThemeAsync()
    {
        if (_appWindowService.MainWindow.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = Theme;

            TitleBarHelper.UpdateTitleBar(_appWindowService.MainWindow, Theme);
        }

        await Task.CompletedTask;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Loads the saved theme from local settings, defaulting to <see cref="ElementTheme.Default"/> if unavailable or invalid.
    /// </summary>
    /// <returns>
    /// A task that completes with the loaded <see cref="ElementTheme"/>.
    /// </returns>
    private async Task<ElementTheme> LoadThemeFromSettingsAsync()
    {
        var themeName = await _localSettingsService.ReadSettingAsync<string>(SettingsKey).ConfigureAwait(false);
        return Enum.TryParse(themeName, out ElementTheme loaded)
            ? loaded
            : ElementTheme.Default;
    }

    /// <summary>
    /// Persists the specified theme to local settings.
    /// </summary>
    /// <param name="theme">The <see cref="ElementTheme"/> to save.</param>
    /// <returns>A task that completes once saving is finished.</returns>
    private async Task SaveThemeInSettingsAsync(ElementTheme theme)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey, theme.ToString()).ConfigureAwait(false);
    }

    #endregion
}
