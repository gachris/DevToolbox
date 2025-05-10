using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevToolbox.Core.Contracts;
using DevToolbox.Core.Media;
using DevToolbox.Wpf.Media;

namespace DevToolbox.Wpf.Demo.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    #region Fields/Conts

    private Theme _theme;
    private readonly IAppUISettings _appUISettings;

    #endregion

    #region Properties

    public string ApplicationVersion { get; }

    public Theme Theme
    {
        get => _theme;
        private set => SetProperty(ref _theme, value);
    }

    #endregion

    public SettingsViewModel(IAppUISettings appUISettings)
    {
        _appUISettings = appUISettings;
        _theme = appUISettings.Theme;

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
    private async Task ChangeTheme(Theme theme)
    {
        await _appUISettings.SetThemeAsync(theme);
    }

    #endregion

    #region Events Subscriptions

    private void ThemeManager_RequestedThemeChanged(object? sender, EventArgs e)
    {
        Theme = _appUISettings.Theme;
    }

    #endregion
}