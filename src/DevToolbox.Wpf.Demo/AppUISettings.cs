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
    private readonly Application _application;
    private readonly ILocalSettingsService _localSettingsService;

    #endregion

    #region Properties

    public Theme AppTheme { get; private set; }

    #endregion

    public AppUISettings(Application application, ILocalSettingsService localSettingsService)
    {
        _application = application;
        _localSettingsService = localSettingsService;

        _uISettings = new UISettings();
        _uISettings.TextScaleFactorChanged += UISettings_TextScaleFactorChanged;
        _uISettings.ColorValuesChanged += UISettings_ColorValuesChanged;

        LoadTheme();
        UpdateFontSizes();
    }

    private async void LoadTheme()
    {
        var themeName = await _localSettingsService.ReadSettingAsync<string>(SettingsKey);

        if (!Enum.TryParse(themeName, out Theme cacheTheme))
        {
            cacheTheme = Theme.Default;
        }

        AppTheme = cacheTheme;
        UpdateTheme();
    }

    #region Methods

    public async void SetAppTheme(Theme appTheme)
    {
        AppTheme = appTheme;

        await _localSettingsService.SaveSettingAsync(SettingsKey, appTheme.ToString());

        UpdateTheme();
    }

    private void UpdateFontSizes()
    {
        for (int i = 9; i < 43; i++)
        {
            var currentFontSizeKey = string.Format(FontSizeKey, i);

            if (!_application.Resources.Contains(currentFontSizeKey))
            {
                continue;
            }

            var fontSize = (double)i;
            fontSize *= _uISettings.TextScaleFactor;

            // Remove the old value
            _application.Resources.Remove(currentFontSizeKey);

            // Add the updated value
            _application.Resources.Add(currentFontSizeKey, fontSize);
        }
    }

    private void UpdateTheme()
    {
        if (SystemParameters.HighContrast)
        {

        }
        else
        {
            _application.ApplyTheme(AppTheme);
        }
    }

    #endregion

    #region Events Subscriptions

    private void UISettings_TextScaleFactorChanged(UISettings sender, object args)
    {
        UpdateFontSizes();
    }

    private void UISettings_ColorValuesChanged(UISettings sender, object args)
    {
        UpdateTheme();
    }

    #endregion
}