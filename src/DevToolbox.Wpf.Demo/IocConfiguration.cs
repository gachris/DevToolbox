using CommonServiceLocator;
using DevToolbox.Core;
using DevToolbox.Core.Contracts;
using DevToolbox.Core.Services;
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
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices((context, services) =>
            {
                var localSettingsOptionsConfiguration = context.Configuration.GetSection(nameof(LocalSettingsOptions));

                services.AddSynchronizationContext();
                services.AddSingleton(localSettingsOptionsConfiguration);
                services.AddSingleton<IApplication>(_ => (App)System.Windows.Application.Current);
                services.AddSingleton(_ => ServiceLocator.Current);
                services.AddUI();
            })
            .Build();

        ServiceLocator.SetLocatorProvider(() => new AppServiceLocator(AppHost.Services));
    }

    #endregion
}
