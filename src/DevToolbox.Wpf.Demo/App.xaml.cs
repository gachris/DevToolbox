using System.Windows;
using CommonServiceLocator;
using DevToolbox.Core.Contracts;
using DevToolbox.Wpf.Media;

namespace DevToolbox.Wpf.Demo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application, IApplication
{
    #region Fields/Consts

    private readonly SingletonApplicationManager _singletonApplicationManager;

    /// <summary>
    /// The event mutex name.
    /// </summary>
    private const string UniqueEventName = "88D8660D-ACB3-4570-ADBC-50F8AFA1352C";

    /// <summary>
    /// The unique mutex name.
    /// </summary>
    private const string UniqueMutexName = "DevToolbox.Wpf.Demo";

    #endregion

    public App()
    {
        _singletonApplicationManager = new SingletonApplicationManager(UniqueEventName, UniqueMutexName);
    }

    #region Methods Overrides

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ThemeManager.OverrideCoreTheming = true;
        UpdateThemeMode();

        ThemeManager.ApplicationThemeChanged += (_, _) => UpdateThemeMode();

        _singletonApplicationManager.Register(this, async () =>
        {
            IocConfiguration.Setup();
            GlobalExceptionHandler.SetupExceptionHandling();

            var appUISettings = ServiceLocator.Current.GetInstance<IAppUISettings>();
            await appUISettings.InitializeAsync();
        }, () => { });
    }

    private void UpdateThemeMode()
    {
        // Set the system's WPF theme mode (experimental in .NET 9)
        ThemeMode = ThemeManager.RequestedTheme switch
        {
            ElementTheme.Default => ThemeMode.None,
            ElementTheme.Light => ThemeMode.Light,
            ElementTheme.Dark => ThemeMode.Dark,
            ElementTheme.WindowsDefault => ThemeMode.System,
            _ => ThemeMode.None
        };
    }

    #endregion
}
