using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using DevToolbox.Core.Contracts;
using DevToolbox.Core.Services;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering core DevToolbox services into an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the current <see cref="SynchronizationContext"/> as a singleton service if one is available.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to add the synchronization context to.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/>, for chaining.
    /// </returns>
    public static IServiceCollection AddSynchronizationContext(this IServiceCollection services)
    {
        if (SynchronizationContext.Current is not null)
        {
            services.AddSingleton(_ => SynchronizationContext.Current);
        }

        return services;
    }

    /// <summary>
    /// Registers a <see cref="PageService"/> as an <see cref="IPageService"/> singleton, with optional configuration.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to add the page service to.
    /// </param>
    /// <param name="configure">
    /// An optional action to configure the created <see cref="PageService"/> instance.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/>, for chaining.
    /// </returns>
    public static IServiceCollection AddPageService(this IServiceCollection services, Action<PageService>? configure = null)
    {
        var pageService = new PageService();

        configure?.Invoke(pageService);

        services.AddSingleton<IPageService>(pageService);

        return services;
    }

    /// <summary>
    /// Scans the specified assembly for implementations of <see cref="IUIService"/> and registers each
    /// concrete type as a singleton for its interface.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to add UI service registrations to.
    /// </param>
    /// <param name="assembly">
    /// The <see cref="Assembly"/> to scan for <see cref="IUIService"/> implementations.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/>, for chaining.
    /// </returns>
    public static IServiceCollection AddIUIServices(this IServiceCollection services, Assembly assembly)
    {
        var uiServiceType = typeof(IUIService);

        // Find all non-generic interfaces that implement IUIService
        var uiServiceInterfaces = assembly.GetTypes()
            .Where(t => t.IsInterface && !t.IsGenericType && t.GetInterfaces().Contains(uiServiceType))
            .ToList();

        // Find all concrete types in the assembly
        var types = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .ToList();

        foreach (var type in types)
        {
            var implementedInterfaces = type.GetInterfaces()
                .Where(uiServiceInterfaces.Contains)
                .ToList();

            foreach (var interfaceType in implementedInterfaces)
            {
                services.AddSingleton(interfaceType, type);
            }
        }

        return services;
    }
}