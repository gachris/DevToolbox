using System.Windows;
using DevToolbox.Wpf.Demo.Helpers;
using DevToolbox.Wpf.Media;

namespace DevToolbox.Wpf.Demo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
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

        _singletonApplicationManager.Register(this, () =>
        {
            IocConfiguration.Setup();
            GlobalExceptionHandler.SetupExceptionHandling();
        });
    }

    #endregion
}
