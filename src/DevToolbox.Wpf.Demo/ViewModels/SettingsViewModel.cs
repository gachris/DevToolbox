using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevToolbox.Wpf.Theming;

namespace DevToolbox.Wpf.Demo.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private Theme _theme;
    private readonly AppUISettings _appUISettings;

    public string ApplicationVersion { get; }

    public Theme Theme
    {
        get => _theme;
        private set => SetProperty(ref _theme, value, nameof(Theme));
    }

    public SettingsViewModel(AppUISettings appUISettings)
    {
        _appUISettings = appUISettings;

        ThemeManager.ApplicationThemeChanged += ThemeManager_ApplicationThemeChanged;

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
    private void ChangeTheme(Theme theme)
    {
        _appUISettings.SetAppTheme(theme);
    }

    #endregion

    #region Events Subscriptions

    private void ThemeManager_ApplicationThemeChanged(object? sender, EventArgs e)
    {
        Theme = _appUISettings.AppTheme;
    }

    #endregion
}