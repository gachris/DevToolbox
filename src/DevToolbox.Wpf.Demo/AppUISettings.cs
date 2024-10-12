using System.Windows;
using DevToolbox.Wpf.Theming;
using Windows.UI.ViewManagement;

namespace DevToolbox.Wpf.Demo;

public class AppUISettings
{
    #region Fields/Consts

    private static readonly string FontSizeKey = "FontSize{0}";
    private const string SettingsKey = "AppBackgroundRequestedTheme";

    private readonly UISettings _uISettings;
    private readonly ILocalSettingsService _localSettingsService;

    #endregion

    #region Properties

    public ElementTheme AppTheme { get; private set; }

    #endregion

    public AppUISettings(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;

        _uISettings = new UISettings();
        _uISettings.TextScaleFactorChanged += UISettings_TextScaleFactorChanged;

        Initialize();
    }

    private async void Initialize()
    {
        UpdateFontSizes();

        var themeName = await _localSettingsService.ReadSettingAsync<string>(SettingsKey);

        if (!Enum.TryParse(themeName, out ElementTheme cacheTheme))
        {
            cacheTheme = ElementTheme.Default;
        }

        AppTheme = cacheTheme;
        ThemeManager.RequestedTheme  = AppTheme;
    }

    #region Methods

    public async void SetAppTheme(ElementTheme appTheme)
    {
        await _localSettingsService.SaveSettingAsync(SettingsKey, appTheme.ToString());

        AppTheme = appTheme;
        ThemeManager.RequestedTheme = AppTheme;
    }

    private void UpdateFontSizes()
    {
        for (var i = 9; i < 43; i++)
        {
            var currentFontSizeKey = string.Format(FontSizeKey, i);

            if (!Application.Current.Resources.Contains(currentFontSizeKey))
            {
                continue;
            }

            var fontSize = (double)i;
            fontSize *= _uISettings.TextScaleFactor;

            // Remove the old value
            Application.Current.Resources.Remove(currentFontSizeKey);

            // Add the updated value
            Application.Current.Resources.Add(currentFontSizeKey, fontSize);
        }
    }

    #endregion

    #region Events Subscriptions

    private void UISettings_TextScaleFactorChanged(UISettings sender, object args)
    {
        UpdateFontSizes();
    }

    #endregion
}