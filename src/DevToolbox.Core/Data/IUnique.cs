using System;

namespace DevToolbox.Core.Data;

/// <summary>
/// Represents an entity with a unique identifier and selection state.
/// </summary>
public interface IUnique
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    Guid Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity is currently selected.
    /// </summary>
    bool IsSelected { get; set; }
}
