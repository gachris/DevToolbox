using System;
using System.Windows.Markup;

namespace DevToolbox.Wpf.Theming;

/// <summary>
/// A markup extension that facilitates loading theme parts from a specified assembly.
/// </summary>
public class ThemePartLoaderExtension : MarkupExtension
{
    #region Properties

    /// <summary>
    /// Gets or sets the name of the assembly from which to load the theme part.
    /// </summary>
    public string? AssemblyName { get; set; }

    /// <summary>
    /// Gets or sets the path to the resource within the assembly.
    /// This path should be relative to the assembly's root.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the name of the theme to load.
    /// This property may be used to specify which theme to apply from the resource path.
    /// </summary>
    public string? ThemeName { get; set; }

    #endregion

    #region Methods Overrides

    /// <summary>
    /// Provides the value of the markup extension, which is a URI pointing to the resource.
    /// </summary>
    /// <param name="serviceProvider">An object that provides services for the markup extension.</param>
    /// <returns>A <see cref="Uri"/> that points to the theme part resource.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var uriString = string.Format("{0};component{1}", AssemblyName, Path);
        return new Uri(uriString, UriKind.RelativeOrAbsolute);
    }

    #endregion
}