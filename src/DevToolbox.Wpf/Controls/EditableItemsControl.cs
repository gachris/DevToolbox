using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DevToolbox.Wpf.Controls;

public class EditableItemsControl : ItemsControl
{
    private IEditableCollectionView EditableItems => Items;

    /// <summary>
    ///     An event that is raised when a new item is created so that
    ///     developers can initialize the item with custom default values.
    /// </summary>
    public event InitializingNewItemEventHandler? InitializingNewItem;

    /// <summary>
    ///     An event that is raised before a new item is created so that
    ///     developers can participate in the construction of the new item.
    /// </summary>
    public event EventHandler<AddingNewItemEventArgs>? AddingNewItem;

    public object Add() => AddNewItem();

    public object Add(object item) => AddNewItem(item);

    private object AddNewItem(object? item = null)
    {
        var addNewItem = (IEditableCollectionViewAddNewItem)Items;
        var isReadOnly = ((IList)Items).IsReadOnly;

        if (addNewItem.CanAddNewItem || !isReadOnly)
        {
            var e = new AddingNewItemEventArgs { NewItem = item };
            OnAddingNewItem(e);
            item = e.NewItem;
        }

        if (!isReadOnly)
        {
            if (item is null)
            {
                var generator = ItemContainerGenerator as IItemContainerGenerator;

                item = GetContainerForItemOverride();
                generator.PrepareItemContainer((DependencyObject)item);
            }
            else Items.Add(item);
        }
        else
        {
            item = item is not null ? addNewItem.AddNewItem(item) : EditableItems.AddNew();

            EditableItems.CommitNew();
        }

        OnInitializingNewItem(new InitializingNewItemEventArgs(item));

        CommandManager.InvalidateRequerySuggested();

        return item;
    }

    public void Remove(object item)
    {
        var isReadOnly = ((IList)Items).IsReadOnly;

        if (!isReadOnly)
            Items.Remove(item);
        else EditableItems.Remove(item);
    }

    /// <summary>
    ///     A method that is called before a new item is created so that
    ///     overrides can participate in the construction of the new item.
    /// </summary>
    /// <remarks>
    ///     The default implementation raises the AddingNewItem event.
    /// </remarks>
    /// <param name="e">Event arguments that provide access to the new item.</param>
    protected virtual void OnAddingNewItem(AddingNewItemEventArgs e) => AddingNewItem?.Invoke(this, e);

    /// <summary>
    ///     A method that is called when a new item is created so that
    ///     overrides can initialize the item with custom default values.
    /// </summary>
    /// <remarks>
    ///     The default implementation raises the InitializingNewItem event.
    /// </remarks>
    /// <param name="e">Event arguments that provide access to the new item.</param>
    protected virtual void OnInitializingNewItem(InitializingNewItemEventArgs e) => InitializingNewItem?.Invoke(this, e);
}
