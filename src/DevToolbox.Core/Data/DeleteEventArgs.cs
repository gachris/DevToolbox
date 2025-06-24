using System;

namespace DevToolbox.Core.Data;

/// <summary>
/// Provides data for delete events, containing the ID of the entity to be deleted.
/// </summary>
public class DeleteEventArgs : EventArgs
{
    #region Properties

    /// <summary>
    /// Gets the unique identifier of the entity to be deleted.
    /// </summary>
    public Guid Id { get; }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteEventArgs"/> class with the specified ID.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to be deleted.</param>
    public DeleteEventArgs(Guid id)
    {
        Id = id;
    }
}
