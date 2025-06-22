using System;
using System.Windows;

namespace DevToolbox.Wpf.Markup;

/// <summary>
/// Represents a resource key for Chrome-style button brushes that optionally distinguishes "Close" buttons.
/// Extends <see cref="ComponentResourceKey"/> to support theme and style variations.
/// </summary>
public class ChromeButtonResourceKey : ComponentResourceKey
{
    /// <summary>
    /// Gets or sets a value indicating whether this key is intended for a close button.
    /// </summary>
    public bool IsClose { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeButtonResourceKey"/> class.
    /// </summary>
    public ChromeButtonResourceKey() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromeButtonResourceKey"/> class
    /// with a target type and resource ID.
    /// </summary>
    /// <param name="typeInTargetAssembly">The type that defines the resource.</param>
    /// <param name="resourceId">The unique identifier for the resource.</param>
    /// <param name="isClose">Indicates whether the key is for a "Close" button.</param>
    public ChromeButtonResourceKey(Type typeInTargetAssembly, object resourceId, bool isClose = false)
        : base(typeInTargetAssembly, resourceId)
    {
        IsClose = isClose;
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (obj is not ChromeButtonResourceKey other)
            return false;

        return Equals(TypeInTargetAssembly, other.TypeInTargetAssembly)
            && Equals(ResourceId, other.ResourceId)
            && IsClose == other.IsClose;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + (TypeInTargetAssembly?.GetHashCode() ?? 0);
            hash = hash * 23 + (ResourceId?.GetHashCode() ?? 0);
            hash = hash * 23 + IsClose.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    /// Returns a string that represents the current resource key.
    /// </summary>
    /// <returns>A string in the format "Close_{ResourceId}" or "{ResourceId}".</returns>
    public override string ToString()
    {
        return $"{(IsClose ? "Close_" : "")}{ResourceId}";
    }
}
