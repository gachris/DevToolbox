using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DevToolbox.Wpf.Data;
using DevToolbox.Wpf.Extensions;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A control that provides a UI for choosing which columns of a <see cref="DataGrid"/> should be visible.
/// </summary>
internal partial class DataGridColumnChooser : UserControl
{
    #region Fields/Consts

    private readonly DataGrid _dataGrid;
    private readonly ICollectionView _collectionView;
    private bool _loading;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the collection of items representing the columns in the DataGrid.
    /// </summary>
    public ObservableCollection<DataGridColumnChooserItem> ItemsSource { get; } = [];

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridColumnChooser"/> class.
    /// </summary>
    /// <param name="dataGrid">The <see cref="DataGrid"/> for which column visibility is managed.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dataGrid"/> is null.</exception>
    public DataGridColumnChooser(DataGrid dataGrid)
    {
        InitializeComponent();

        _dataGrid = dataGrid ?? throw new ArgumentNullException(nameof(dataGrid));
        _dataGrid.Columns.CollectionChanged += Columns_CollectionChanged;
        _dataGrid.ColumnReordered += DataGrid_ColumnReordered;
        _dataGrid.ColumnDisplayIndexChanged += DataGrid_ColumnDisplayIndexChanged;

        _collectionView = CollectionViewSource.GetDefaultView(_dataGrid.Items);
        _collectionView.GroupDescriptions.CollectionChanged += GroupDescriptions_CollectionChanged;

        UpdateItems();
    }

    #region Methods

    /// <summary>
    /// Updates the collection of items based on the current state of the DataGrid columns.
    /// </summary>
    private void UpdateItems()
    {
        if (_loading)
        {
            return;
        }

        _loading = true;

        foreach (var item in ItemsSource)
        {
            item.Checked -= ItemCheckedChanged;
            item.Unchecked -= ItemCheckedChanged;
        }

        var columns = _dataGrid.Columns;
        var groupedColumnsSet = new HashSet<DataGridColumn>();
        var newItemsSource = new ObservableCollection<DataGridColumnChooserItem>();

        foreach (var group in _collectionView.GroupDescriptions.OfType<PropertyGroupDescription>())
        {
            var groupedColumn = columns.FirstOrDefault(c => c.SortMemberPath == group.PropertyName);
            if (groupedColumn != null)
            {
                groupedColumnsSet.Add(groupedColumn);
            }
        }

        foreach (var column in columns)
        {
            var isHiddenColumn = column.Visibility is not Visibility.Visible;
            var item = new DataGridColumnChooserItem
            {
                IsEnabled = !groupedColumnsSet.Contains(column),
                Header = column.Header,
                ColumnField = column.SortMemberPath,
                IsChecked = !isHiddenColumn || groupedColumnsSet.Contains(column),
                DisplayIndex = column.DisplayIndex
            };

            item.Checked += ItemCheckedChanged;
            item.Unchecked += ItemCheckedChanged;

            newItemsSource.Add(item);
        }

        var visibleCount = columns.Count(c => c.Visibility == Visibility.Visible);
        if (visibleCount <= 1)
        {
            var firstEnabledItem = newItemsSource.FirstOrDefault(x => x.IsChecked && x.IsEnabled);
            if (firstEnabledItem != null)
            {
                firstEnabledItem.IsEnabled = false;
            }
        }

        var sortedList = newItemsSource.OrderBy(x => x.DisplayIndex);
        ItemsSource.Clear();
        ItemsSource.AddRange(sortedList);

        _loading = false;
    }

    /// <summary>
    /// Toggles the visibility of the specified column in the DataGrid.
    /// </summary>
    /// <param name="column">The <see cref="DataGridColumn"/> to update.</param>
    /// <param name="visibility">The desired visibility state.</param>
    private void ToggleColumnVisibility(DataGridColumn column, Visibility visibility)
    {
        var enableColumnVirtualization = _dataGrid.EnableColumnVirtualization;
        _dataGrid.EnableColumnVirtualization = false;
        column.Visibility = visibility;
        _dataGrid.EnableColumnVirtualization = enableColumnVirtualization;
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles the Columns collection changed event to update the items.
    /// </summary>
    private void Columns_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateItems();
    }

    /// <summary>
    /// Handles the ColumnReordered event of the DataGrid.
    /// </summary>
    private void DataGrid_ColumnReordered(object? sender, DataGridColumnEventArgs e)
    {
        UpdateItems();
    }

    /// <summary>
    /// Handles the ColumnDisplayIndexChanged event of the DataGrid.
    /// </summary>
    private void DataGrid_ColumnDisplayIndexChanged(object? sender, DataGridColumnEventArgs e)
    {
        UpdateItems();
    }

    /// <summary>
    /// Handles the GroupDescriptions collection changed event to update the items.
    /// </summary>
    private void GroupDescriptions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateItems();
    }

    /// <summary>
    /// Handles the Checked and Unchecked events of the DataGridColumnChooserItem.
    /// Toggles the visibility of the corresponding column in the DataGrid.
    /// </summary>
    private void ItemCheckedChanged(object? sender, EventArgs e)
    {
        if (_loading || sender is not DataGridColumnChooserItem model || string.IsNullOrEmpty(model.ColumnField))
        {
            return;
        }

        var column = _dataGrid.Columns.FirstOrDefault(c => c.SortMemberPath == model.ColumnField);
        if (column != null)
        {
            var visibility = model.IsChecked ? Visibility.Visible : Visibility.Hidden;
            ToggleColumnVisibility(column, visibility);
        }

        UpdateItems();
    }

    #endregion
}