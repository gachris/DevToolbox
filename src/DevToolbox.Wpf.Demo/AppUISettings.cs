using DevToolbox.Wpf.Media;

namespace DevToolbox.Wpf.Demo;

public class AppUISettings
{
    #region Fields/Consts

    private const string SettingsKey = "AppBackgroundRequestedTheme";

    private readonly ILocalSettingsService _localSettingsService;

    #endregion

    #region Properties

    public ElementTheme AppTheme { get; private set; }

    #endregion

    public AppUISettings(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;

        InitializeTheme();

        FontSizeManager.TextScaleEnabled = true;
    }

    #region Methods

    private async void InitializeTheme()
    {
        var themeName = await _localSettingsService.ReadSettingAsync<string>(SettingsKey);

        if (!Enum.TryParse(themeName, out ElementTheme cacheTheme))
        {
            cacheTheme = ElementTheme.Default;
        }

        AppTheme = cacheTheme;
        ThemeManager.RequestedTheme = AppTheme;
    }

    public async void SetAppTheme(ElementTheme appTheme)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey, appTheme.ToString());

        AppTheme = appTheme;
        ThemeManager.RequestedTheme = AppTheme;
    }

    #endregion
}