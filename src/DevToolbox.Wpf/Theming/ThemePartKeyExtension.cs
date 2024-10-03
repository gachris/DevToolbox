using System;

namespace DevToolbox.Wpf.Theming;

/// <summary>
/// A markup extension that serves as a key for theme parts, 
/// providing additional functionality for managing theme resources.
/// </summary>
public class ThemePartKeyExtension : ThemePartLoaderExtension
{
    #region Properties

    /// <summary>
    /// Gets or sets the URI associated with the theme part.
    /// This property holds the URI after the theme part is loaded.
    /// </summary>
    internal Uri? Uri { get; set; }

    #endregion

    #region Methods Override

    /// <summary>
    /// Provides the value of the markup extension, which is a <see cref="Uri"/> 
    /// pointing to the theme part resource, and stores it in the Uri property.
    /// </summary>
    /// <param name="serviceProvider">An object that provides services for the markup extension.</param>
    /// <returns>The current instance of <see cref="ThemePartKeyExtension"/>.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        Uri = base.ProvideValue(serviceProvider) as Uri;
        return this;
    }

    /// <summary>
    /// Gets the hash code for the current instance, which is based on the AssemblyName.
    /// </summary>
    /// <returns>An integer that represents the hash code for the current instance.</returns>
    public override int GetHashCode()
    {
        return AssemblyName == null ? 0 : AssemblyName.GetHashCode();
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// The comparison is based on the AssemblyName property.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is equal to the current instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is ThemePartKeyExtension partKeyExtension &&
               string.Equals(AssemblyName, partKeyExtension.AssemblyName, StringComparison.InvariantCultureIgnoreCase);
    }

    #endregion
}
