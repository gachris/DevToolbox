using CommonServiceLocator;
using DevToolbox.Wpf.Demo.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DevToolbox.Wpf.Demo;

public static class IocConfiguration
{
    public static IHost? AppHost { get; private set; }

    #region Methods

    public static void Setup()
    {
        AppHost = Host.CreateDefaultBuilder()
            //.ConfigureAppConfiguration((hostContext, configureDelegate) =>
            //{
            //    var baseDirectory = Directory.GetCurrentDirectory();
            //    configureDelegate.SetBasePath(baseDirectory);
            //    configureDelegate.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            //})
            .ConfigureServices((hostContext, services) =>
            {
                if (SynchronizationContext.Current is not null)
                {
                    services.AddSingleton(SynchronizationContext.Current);
                }

                services.AddSingleton<IFileService, FileService>();
                services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
                services.AddSingleton(t => new AppUISettings(System.Windows.Application.Current, t.GetService<ILocalSettingsService>()!));
                services.AddSingleton(t => ServiceLocator.Current);
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<SettingsViewModel>();
            })
            .Build();

        ServiceLocator.SetLocatorProvider(() => new AppServiceLocator(AppHost.Services));
    }

    #endregion
}