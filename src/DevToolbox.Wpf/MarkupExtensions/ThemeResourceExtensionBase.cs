using System;
using System.Windows.Interop;
using System.Windows.Markup;

namespace DevToolbox.Wpf.MarkupExtensions;

/// <summary>
/// An abstract base class for theme resource extensions used in XAML.
/// This class serves as a foundation for creating specific theme resource extensions,
/// allowing for dynamic resource loading based on the provided resource path.
/// </summary>
public abstract class ThemeResourceExtensionBase : MarkupExtension
{
    /// <summary>
    /// Gets or sets the path to the resource.
    /// This property should contain the relative path to the theme resource being referenced.
    /// </summary>
    public string ResourcePath { get; set; }

    /// <summary>
    /// Gets the namespace of the derived theme resource extension.
    /// This property must be implemented in derived classes to specify the appropriate namespace.
    /// </summary>
    protected abstract string Namespace { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeResourceExtensionBase"/> class
    /// with the specified resource path.
    /// </summary>
    /// <param name="resourcePath">The path to the resource being referenced.</param>
    protected ThemeResourceExtensionBase(string resourcePath)
    {
        ResourcePath = resourcePath; // Set the resource path
    }

    /// <summary>
    /// Provides the value of the resource specified by the <see cref="ResourcePath"/>.
    /// This method constructs a URI for the resource based on the namespace and resource path.
    /// </summary>
    /// <param name="serviceProvider">An object that provides services for the markup extension.</param>
    /// <returns>A <see cref="Uri"/> representing the location of the theme resource.</returns>
    public sealed override object ProvideValue(IServiceProvider serviceProvider)
    {
        var path = !BrowserInteropHelper.IsBrowserHosted ? GetResourcePath(serviceProvider) : "Themes/Fake.xaml";
        var uriString = string.Format("/{0};component/{1}", Namespace, path); // Format URI string
        return new Uri(uriString, UriKind.RelativeOrAbsolute); // Return the constructed URI
    }

    /// <summary>
    /// Retrieves the resource path for the theme resource based on the provided service provider.
    /// This method must be implemented in derived classes to specify how the resource path is determined.
    /// </summary>
    /// <param name="serviceProvider">An object that provides services for the markup extension.</param>
    /// <returns>A string representing the resource path.</returns>
    protected abstract string GetResourcePath(IServiceProvider serviceProvider);
}
