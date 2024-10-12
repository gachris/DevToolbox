using System;

namespace DevToolbox.Wpf.Markup;

/// <summary>
/// An abstract base class for resource extensions in WPF that inherit from 
/// <see cref="ThemeResourceExtensionBase"/>. This class provides a foundation
/// for theme resource extensions by implementing the resource path retrieval mechanism.
/// </summary>
public abstract class ResourceExtensionBase : ThemeResourceExtensionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceExtensionBase"/> class
    /// with the specified resource path.
    /// </summary>
    /// <param name="resourcePath">The path to the resource being referenced.</param>
    protected ResourceExtensionBase(string resourcePath) : base(resourcePath)
    {
    }

    /// <summary>
    /// Gets the resource path for the theme resource.
    /// This method overrides the abstract method from the base class to provide
    /// the resource path defined in the <see cref="ThemeResourceExtensionBase.ResourcePath"/> property.
    /// </summary>
    /// <param name="serviceProvider">An object that provides services for the markup extension.</param>
    /// <returns>The resource path as a string.</returns>
    protected sealed override string GetResourcePath(IServiceProvider serviceProvider) => GetResourcePath();

    /// <summary>
    /// Retrieves the resource path for the theme resource.
    /// This virtual method can be overridden in derived classes to customize the
    /// resource path retrieval logic beyond the default behavior.
    /// </summary>
    /// <returns>The resource path as a string.</returns>
    protected virtual string GetResourcePath() => ResourcePath;
}
