using System;
using System.Threading.Tasks;
using DevToolbox.Core.Contracts;
using DevToolbox.Core.Data;

namespace DevToolbox.Core.Services;

/// <summary>
/// Provides a base implementation for application UI services.
/// </summary>
/// <remarks>
/// Derive from this class to implement navigation, notifications, view
/// management, or other UI-related functionality.
/// </remarks>
public abstract class UIService : IUIService
{
}

/// <summary>
/// Provides a base implementation of <see cref="IUIService{TEntityViewModel}"/> for managing
/// entity view models with editing and deletion functionality.
/// </summary>
/// <typeparam name="TEntityViewModel">
/// The type of the entity view model, which must implement
/// <see cref="IUnique"/>, <see cref="IObservableEditableObject"/>, and have a parameterless constructor.
/// </typeparam>
public abstract class UIService<TEntityViewModel> : UIService, IUIService<TEntityViewModel>
    where TEntityViewModel : IUnique, IObservableEditableObject, new()
{
    #region Fields/Consts

    /// <inheritdoc/>
    public event EventHandler<EditEventArgs<TEntityViewModel>>? EditBegin;

    /// <inheritdoc/>
    public event EventHandler<EditEventArgs<TEntityViewModel>>? EditCanceled;

    /// <inheritdoc/>
    public event EventHandler<EditEventArgs<TEntityViewModel>>? EditEnded;

    /// <inheritdoc/>
    public event EventHandler<DeleteEventArgs>? Deleted;

    #endregion

    #region Methods

    /// <inheritdoc/>
    public virtual TEntityViewModel CreateNew()
    {
        var id = Guid.NewGuid();
        var entity = new TEntityViewModel
        {
            IsNew = true,
            Id = id
        };

        return entity;
    }

    /// <inheritdoc/>
    public virtual Task<bool> AddOrUpdateAsync(TEntityViewModel item)
    {
        if (!item.HasChanges || item.HasErrors)
        {
            CancelEdit(item);
            return Task.FromResult(false);
        }

        var isNew = item.IsNew;

        item.EndEdit();
        EditEnded?.Invoke(this, new EditEventArgs<TEntityViewModel>(item, isNew));

        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public virtual Task DeleteAsync(Guid id)
    {
        Deleted?.Invoke(this, new DeleteEventArgs(id));
        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public virtual void BeginEdit(TEntityViewModel item)
    {
        var isNew = item.IsNew;

        item.BeginEdit();
        EditBegin?.Invoke(this, new EditEventArgs<TEntityViewModel>(item, isNew));
    }

    /// <inheritdoc/>
    public virtual void CancelEdit(TEntityViewModel item)
    {
        var isNew = item.IsNew;

        item.CancelEdit();
        EditCanceled?.Invoke(this, new EditEventArgs<TEntityViewModel>(item, isNew));
    }

    #endregion
}
