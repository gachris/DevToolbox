using DevToolbox.Core.Contracts;
using DevToolbox.Core.Services;
using DevToolbox.Wpf.Demo.ViewModels;
using DevToolbox.Wpf.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DevToolbox.Wpf.Demo;

/// <summary>
/// Provides extension methods for registering UI services and view models into the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers UI-related services, dialog services, and view models with the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add UI services to.</param>
    /// <returns>The original <see cref="IServiceCollection"/>, for chaining.</returns>
    public static IServiceCollection AddUI(this IServiceCollection services)
    {
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<ILocalSettingsService, LocalSettingsService>();

        services.AddSingleton<IAppUISettings, AppUISettings>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<INavigationService, NavigationService>();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<SettingsViewModel>();

        return services;
    }
}
