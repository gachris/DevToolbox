using Microsoft.Windows.ApplicationModel.Resources;

namespace DevToolbox.WinUI.Helpers;

/// <summary>
/// Provides extension methods for retrieving localized resources using <see cref="ResourceLoader"/>.
/// </summary>
public static class ResourceExtensions
{
    private static readonly ResourceLoader _resourceLoader = new();

    /// <summary>
    /// Retrieves the localized string for the given resource key from the application's resource files.
    /// </summary>
    /// <param name="resourceKey">
    /// The resource identifier defined in the resource file (e.g., "AppTitle").
    /// </param>
    /// <returns>
    /// The localized string corresponding to <paramref name="resourceKey"/>,
    /// or an empty string if the key is not found.
    /// </returns>
    public static string GetLocalized(this string resourceKey)
    {
        return _resourceLoader.GetString(resourceKey);
    }
}
