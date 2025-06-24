using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DevToolbox.Core.Messages;

namespace DevToolbox.Core.Data;

/// <summary>
/// Provides a base class for view models representing list views with support for data retrieval,
/// editing, selection, and messaging.
/// </summary>
/// <typeparam name="TEntityViewModel">The type of the entity view model that implements <see cref="IUnique"/> and <see cref="ICloneable"/>.</typeparam>
public abstract partial class BaseListViewViewModel<TEntityViewModel> : ObservableRecipient
    where TEntityViewModel : class, IUnique, ICloneable
{
    #region Fields/Consts

    private readonly ObservableCollection<TEntityViewModel> _items = [];

    private CancellationTokenSource? _cancellationTokenSource;
    private TEntityViewModel? _selectedItem;
    private TEntityViewModel? _itemToBringIntoView;
    private bool _isBusy;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the currently selected item.
    /// Notifies CanExecuteChanged for related commands.
    /// </summary>
    public TEntityViewModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            SetProperty(ref _selectedItem, value);
            OnSelectedItemChanged(value);

            BeginEditCommand.NotifyCanExecuteChanged();
            EndEditCommand.NotifyCanExecuteChanged();
            CancelEditCommand.NotifyCanExecuteChanged();
            DeleteCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// Gets or sets the item to bring into view (e.g., scroll into view in UI).
    /// </summary>
    public TEntityViewModel? ItemToBringIntoView
    {
        get => _itemToBringIntoView;
        set => SetProperty(ref _itemToBringIntoView, value);
    }

    /// <summary>
    /// Gets the collection of items displayed in the view.
    /// </summary>
    protected ObservableCollection<TEntityViewModel> Items => _items;

    /// <summary>
    /// Gets or sets a value indicating whether the view model is currently performing a long-running operation.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        protected set => SetProperty(ref _isBusy, value);
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseListViewViewModel{TEntityViewModel}"/> class.
    /// </summary>
    protected BaseListViewViewModel() { }

    #region Methods

    /// <summary>
    /// Retrieves items asynchronously. Must be implemented by derived classes.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    protected abstract Task<bool> RetrieveItemsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Adds or updates a single item in the collection.
    /// </summary>
    protected virtual void AddOrUpdateItem(TEntityViewModel item, bool refreshView = true)
    {
        AddOrUpdateItems([item], refreshView);
    }

    /// <summary>
    /// Adds or updates a range of items in the collection.
    /// </summary>
    protected virtual void AddOrUpdateItems(IEnumerable<TEntityViewModel> items, bool refreshView = true)
    {
        foreach (var item in items)
        {
            var existingItem = _items.FirstOrDefault(x => x.Id == item.Id);
            if (existingItem != null)
            {
                existingItem.CopyFrom(item);
            }
            else
            {
                _items.Add(item);
            }
        }

        if (refreshView)
            RefreshView();
    }

    /// <summary>
    /// Replaces the current item collection with the provided items.
    /// </summary>
    protected virtual void UpdateItemsCollection(IEnumerable<TEntityViewModel> items, bool refreshView = true)
    {
        _items.Clear();
        foreach (var item in items)
        {
            _items.Add(item);
        }

        if (refreshView)
        {
            RefreshView();
        }
    }

    /// <summary>
    /// Deletes an item with the specified ID from the collection.
    /// </summary>
    protected virtual void DeleteItem(Guid id, bool refreshView = true)
    {
        var item = _items.Single(x => x.Id == id);
        _items.Remove(item);

        if (refreshView)
            RefreshView();
    }

    /// <summary>
    /// Called to manually trigger view updates (e.g., UI refresh).
    /// </summary>
    protected virtual void RefreshView() { }

    /// <summary>
    /// Called when the selected item changes.
    /// </summary>
    protected virtual void SelectedItemChanged(TEntityViewModel? value) { }

    /// <summary>
    /// Filter logic used in derived classes to include or exclude items from the view.
    /// </summary>
    protected virtual bool Filter(TEntityViewModel item) => true;

    /// <summary>
    /// Called when an error occurs during retrieval or processing.
    /// </summary>
    protected virtual void ShowError() { }

    /// <summary>
    /// Asynchronously refreshes the item list and handles cancellation and errors.
    /// </summary>
    protected async Task RefreshAsync()
    {
        try
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            IsBusy = true;
            _items.Clear();
            SelectedItem = null;

            var result = await RetrieveItemsAsync(_cancellationTokenSource.Token);

            if (!result)
                ShowError();

            RefreshView();
            _cancellationTokenSource = null;
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation if needed
        }
        finally
        {
            if (_cancellationTokenSource is null)
                IsBusy = false;
        }
    }

    /// <summary>
    /// Determines whether editing can be started for the given item.
    /// </summary>
    protected virtual bool CanBeginEdit(TEntityViewModel item) => item is not null;

    /// <summary>
    /// Determines whether editing can be canceled for the given item.
    /// </summary>
    protected virtual bool CanCancelEdit(TEntityViewModel item) => item is not null;

    /// <summary>
    /// Determines whether editing can be completed for the given item.
    /// </summary>
    protected virtual bool CanEndEdit(TEntityViewModel item) => item is not null;

    /// <summary>
    /// Determines whether the given item can be deleted.
    /// </summary>
    protected virtual bool CanDeleted(TEntityViewModel item) => item is not null;

    #endregion

    #region Relay Commands

    /// <summary>
    /// Refreshes the UI and reloads the data.
    /// </summary>
    [RelayCommand]
    protected virtual async Task RefreshUIAsync()
    {
        await RefreshAsync();
    }

    /// <summary>
    /// Creates a new instance of the entity. Must be implemented by derived classes.
    /// </summary>
    [RelayCommand]
    protected virtual void CreateNew()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Begins editing the specified item.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanBeginEdit))]
    protected virtual void BeginEdit(TEntityViewModel item)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Cancels editing of the specified item.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCancelEdit))]
    protected virtual void CancelEdit(TEntityViewModel item)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Ends editing of the specified item.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanEndEdit))]
    protected virtual Task EndEditAsync(TEntityViewModel item)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Deletes the specified item.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleted))]
    protected virtual Task DeleteAsync(TEntityViewModel item)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Partial Methods

    /// <summary>
    /// Called when the <see cref="SelectedItem"/> property changes.
    /// Sends a <see cref="SelectedItemChangedMessage{T}"/> via the messenger.
    /// </summary>
    /// <param name="value">The newly selected item.</param>
    private void OnSelectedItemChanged(TEntityViewModel? value)
    {
        SelectedItemChanged(value);
        Messenger.Send(new SelectedItemChangedMessage<TEntityViewModel>(value));
    }

    #endregion
}
