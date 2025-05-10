using System;

namespace DevToolbox.Core.Contracts;

/// <summary>
/// Provides methods for configuring and retrieving page types by key for navigation purposes.
/// </summary>
public interface IPageService
{
    /// <summary>
    /// Gets the <see cref="Type"/> associated with the specified page key.
    /// </summary>
    /// <param name="key">The key representing the page.</param>
    /// <returns>The <see cref="Type"/> of the page.</returns>
    Type GetPageType(string key);

    /// <summary>
    /// Configures a mapping between a page key and the page type specified by the generic type parameter.
    /// </summary>
    /// <typeparam name="T">The page type to associate with the key.</typeparam>
    /// <param name="key">The key to associate with the page type.</param>
    void Configure<T>(string key);

    /// <summary>
    /// Configures a mapping between a page key and a page <see cref="Type"/>.
    /// </summary>
    /// <param name="key">The key to associate with the page type.</param>
    /// <param name="type">The <see cref="Type"/> of the page to associate with the key.</param>
    void Configure(string key, Type type);
}
