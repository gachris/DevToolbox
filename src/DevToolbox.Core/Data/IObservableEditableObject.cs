using System.ComponentModel;

namespace DevToolbox.Core.Data;

/// <summary>
/// Represents an editable object that supports change tracking and error notification.
/// </summary>
public interface IObservableEditableObject : IEditableObject
{
    /// <summary>
    /// Gets or sets a value indicating whether the object is newly created.
    /// </summary>
    bool IsNew { get; set; }

    /// <summary>
    /// Gets a value indicating whether the object has changes that have not been committed.
    /// </summary>
    bool HasChanges { get; }

    /// <summary>
    /// Gets a value indicating whether the object has any validation or business logic errors.
    /// </summary>
    bool HasErrors { get; }
}
