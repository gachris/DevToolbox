using System;
using System.Threading.Tasks;
using DevToolbox.Core.Data;

namespace DevToolbox.Core.Contracts;

/// <summary>
/// Provides general UI-related services for the application, such as
/// navigation, notifications, or view management.
/// </summary>
public interface IUIService
{
}

/// <summary>
/// Defines UI-related services specific to managing entities of type <typeparamref name="TEntityViewModel"/>.
/// </summary>
/// <typeparam name="TEntityViewModel">The type of the view model representing the entity.</typeparam>
public interface IUIService<TEntityViewModel> : IUIService where TEntityViewModel : IUnique
{
    /// <summary>
    /// Occurs when editing begins for an entity.
    /// </summary>
    event EventHandler<EditEventArgs<TEntityViewModel>>? EditBegin;

    /// <summary>
    /// Occurs when editing is canceled for an entity.
    /// </summary>
    event EventHandler<EditEventArgs<TEntityViewModel>>? EditCanceled;

    /// <summary>
    /// Occurs when editing ends for an entity.
    /// </summary>
    event EventHandler<EditEventArgs<TEntityViewModel>>? EditEnded;

    /// <summary>
    /// Occurs when an entity is deleted.
    /// </summary>
    event EventHandler<DeleteEventArgs>? Deleted;

    /// <summary>
    /// Creates a new instance of <typeparamref name="TEntityViewModel"/>.
    /// </summary>
    /// <returns>A new entity view model instance.</returns>
    TEntityViewModel CreateNew();

    /// <summary>
    /// Adds a new entity or updates an existing one asynchronously.
    /// </summary>
    /// <param name="item">The entity to add or update.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    Task<bool> AddOrUpdateAsync(TEntityViewModel item);

    /// <summary>
    /// Begins editing the specified entity.
    /// </summary>
    /// <param name="item">The entity to begin editing.</param>
    void BeginEdit(TEntityViewModel item);

    /// <summary>
    /// Cancels editing of the specified entity.
    /// </summary>
    /// <param name="item">The entity whose edit operation should be canceled.</param>
    void CancelEdit(TEntityViewModel item);

    /// <summary>
    /// Deletes an entity by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(Guid id);
}
