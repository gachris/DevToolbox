using System;

namespace DevToolbox.Core.Data;

/// <summary>
/// Provides data for edit events, including the item being edited and whether it is a new item.
/// </summary>
/// <typeparam name="TEntityViewModel">The type of the entity view model.</typeparam>
public class EditEventArgs<TEntityViewModel> : EventArgs
{
    #region Properties

    /// <summary>
    /// Gets the item that is being edited.
    /// </summary>
    public TEntityViewModel Item { get; }

    /// <summary>
    /// Gets a value indicating whether the item is new.
    /// </summary>
    public bool IsNew { get; }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="EditEventArgs{TEntityViewModel}"/> class
    /// with the specified item and a flag indicating whether it is new.
    /// </summary>
    /// <param name="item">The item being edited.</param>
    /// <param name="isNew">A value indicating whether the item is new.</param>
    public EditEventArgs(TEntityViewModel item, bool isNew)
    {
        Item = item;
        IsNew = isNew;
    }
}
