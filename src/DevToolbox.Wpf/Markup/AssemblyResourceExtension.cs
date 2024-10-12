namespace DevToolbox.Wpf.Markup;

/// <summary>
/// A sealed class that extends <see cref="ResourceExtensionBase"/> to provide 
/// a mechanism for accessing resources defined in a specific assembly.
/// </summary>
internal sealed class AssemblyResourceExtension : ResourceExtensionBase
{
    /// <summary>
    /// Gets the namespace for the assembly resources.
    /// This property overrides the abstract property in the base class to
    /// specify the correct namespace for resources.
    /// </summary>
    protected override string Namespace => "DevToolbox.Wpf";

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyResourceExtension"/> class
    /// with the specified resource path.
    /// </summary>
    /// <param name="resourcePath">The path to the resource within the assembly.</param>
    public AssemblyResourceExtension(string resourcePath) : base(resourcePath)
    {
    }
}
