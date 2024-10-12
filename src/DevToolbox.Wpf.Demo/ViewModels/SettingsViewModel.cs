using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevToolbox.Wpf.Theming;

namespace DevToolbox.Wpf.Demo.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    #region Fields/Conts

    private ElementTheme _theme;
    private readonly AppUISettings _appUISettings;

    #endregion
    
    #region Properties

    public string ApplicationVersion { get; }

    public ElementTheme Theme
    {
        get => _theme;
        private set => SetProperty(ref _theme, value, nameof(Theme));
    }

    #endregion

    public SettingsViewModel(AppUISettings appUISettings)
    {
        _appUISettings = appUISettings;

        ThemeManager.RequestedThemeChanged += ThemeManager_RequestedThemeChanged;

        ApplicationVersion = GetApplicationVersion();
    }

    #region Methods

    private static string GetApplicationVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version!;
        return $"Wpf Gallery - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    #endregion

    #region Relay Commands

    [RelayCommand]
    private void ChangeTheme(ElementTheme theme)
    {
        _appUISettings.SetAppTheme(theme);
    }

    #endregion

    #region Events Subscriptions

    private void ThemeManager_RequestedThemeChanged(object? sender, EventArgs e)
    {
        Theme = _appUISettings.AppTheme;
    }

    #endregion
}