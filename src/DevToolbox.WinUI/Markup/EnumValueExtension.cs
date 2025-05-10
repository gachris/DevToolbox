using System;
using Microsoft.UI.Xaml.Markup;

namespace DevToolbox.WinUI.Markup;

/// <summary>
/// A XAML markup extension that returns an <see cref="Enum"/> value for the specified type and member name.
/// </summary>
[MarkupExtensionReturnType(ReturnType = typeof(object))]
public class EnumValueExtension : MarkupExtension
{
    /// <summary>
    /// Gets or sets the <see cref="Type"/> of the enumeration.
    /// </summary>
    /// <remarks>
    /// Must be set to an enum <see cref="Type"/> before providing a value.
    /// </remarks>
    public Type? Type { get; set; }

    /// <summary>
    /// Gets or sets the name of the enumeration member to return.
    /// </summary>
    /// <remarks>
    /// Must match a valid member name of the specified <see cref="Type"/>.
    /// </remarks>
    public string? Member { get; set; }

    /// <summary>
    /// Returns the enum value corresponding to the specified <see cref="Type"/> and <see cref="Member"/>.
    /// </summary>
    /// <returns>
    /// An <see cref="object"/> representing the parsed enum value.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <see cref="Type"/> or <see cref="Member"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <see cref="Type"/> is not an enum type or if <see cref="Member"/> is not a valid member name.
    /// </exception>
    protected override object ProvideValue()
    {
        if (Type is null)
            throw new ArgumentNullException(nameof(Type));
        if (Member is null)
            throw new ArgumentNullException(nameof(Member));

        if (!Type.IsEnum)
            throw new ArgumentException($"Type {Type.Name} must be an Enum.", nameof(Type));

        return Enum.Parse(Type, Member);
    }
}
