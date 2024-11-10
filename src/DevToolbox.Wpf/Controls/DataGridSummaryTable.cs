using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DevToolbox.Wpf.Data;
using DevToolbox.Wpf.Extensions;
using DevToolbox.Wpf.Helpers;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a control that displays summary information for a DataGrid.
/// </summary>
public class DataGridSummaryTable : ItemsControl
{
    #region Fields/Consts

    private readonly ObservableCollection<DataGridSummaryTableColumn> _columns = [];
    private GridControl? _gridControl;
    private ScrollViewer? _scrollViewer;

    #endregion

    static DataGridSummaryTable()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridSummaryTable), new FrameworkPropertyMetadata(typeof(DataGridSummaryTable)));
    }

    #region Methods Overrides

    /// <summary>
    /// Called when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_scrollViewer is not null)
        {
            _scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
        }

        _scrollViewer = Template.FindName("DG_ScrollViewer", this) as ScrollViewer;

        if (_scrollViewer is not null)
        {
            _scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
        }

        if (_gridControl is not null)
        {
            _gridControl.Columns.CollectionChanged -= Columns_CollectionChanged;
            _gridControl.ColumnReordered -= GridControl_ColumnReordered;
        }

        _gridControl = this.FindVisualAncestor<GridControl>();

        if (_gridControl is not null)
        {
            _gridControl.Columns.CollectionChanged += Columns_CollectionChanged;
            _gridControl.ColumnReordered += GridControl_ColumnReordered;
        }

        CalculateSummaryValues();

        EventManager.RegisterClassHandler(typeof(DataGridColumnHeader), DataGridColumnHeader.SizeChangedEvent, new SizeChangedEventHandler(ColumnSplitter_SizeChanged));

        void ColumnSplitter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSummaries();
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Computes the summary values for the table and updates the columns.
    /// </summary>
    public void CalculateSummaryValues()
    {
        if (_gridControl is null || !_gridControl.TotalSummary.Any() || !_gridControl.Columns.Any())
            return;

        _columns.Clear();

        foreach (var column in _gridControl.Columns.OrderBy(x => x.DisplayIndex))
        {
            _columns.Add(new DataGridSummaryTableColumn
            {
                ColumnName = column.SortMemberPath,
                Visibility = column.Visibility,
                Width = column.ActualWidth
            });
        }

        var rowCountByColumn = new Dictionary<string, int>();
        var itemsSource = GetItemsSourceAsDataTable() ?? throw new ArgumentNullException("itemsSource", "ItemsSource cannot be null.");

        foreach (var gridSummaryItem in _gridControl.TotalSummary)
        {
            var showInColumn = gridSummaryItem.ShowInColumn ?? gridSummaryItem.FieldName;
            var value = DataGridSummaryHelper.GetValue(itemsSource, gridSummaryItem);

            if (!rowCountByColumn.TryGetValue(showInColumn, out int index))
            {
                index = 0;
                rowCountByColumn[showInColumn] = index;
            }
            else
            {
                rowCountByColumn[showInColumn] = ++index;
            }

            _columns.FirstOrDefault(c => c.ColumnName == showInColumn)?.Cells.Insert(index, new DataGridSummaryTableCell
            {
                Value = value,
                Style = gridSummaryItem.Style
            });
        }

        ItemsSource = _columns;
    }

    /// <summary>
    /// Updates the visibility and width of summary columns based on the corresponding columns in the GridControl.
    /// </summary>
    private void UpdateSummaries()
    {
        if (_gridControl is null) return;

        foreach (var column in _gridControl.Columns)
        {
            var summaryRowGridColumn = _columns.FirstOrDefault(c => c.ColumnName == column.SortMemberPath);
            if (summaryRowGridColumn != null)
            {
                summaryRowGridColumn.Visibility = column.Visibility;
                summaryRowGridColumn.Width = column.ActualWidth;
            }
        }
    }

    /// <summary>
    /// Converts the ItemsSource to a DataTable for easier summary computation.
    /// </summary>
    /// <returns>A DataTable representing the ItemsSource.</returns>
    private DataTable? GetItemsSourceAsDataTable()
    {
        if (_gridControl?.ItemsSource is DataView dataView)
        {
            return dataView.ToTable();
        }
        else if (_gridControl?.ItemsSource != null)
        {
            return DataTableHelper.ToDataTable(_gridControl.ItemsSource.Cast<object>());
        }
        else if (_gridControl?.Items != null)
        {
            return DataTableHelper.ToDataTable(_gridControl.Items.Cast<object>());
        }

        return null;
    }

    #endregion

    #region Events Subscriptions

    /// <summary>
    /// Handles the ScrollChanged event of the ScrollViewer to synchronize scrolling with the GridControl.
    /// </summary>
    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (e.HorizontalChange == 0)
        {
            return;
        }

        if (_gridControl?.ScrollViewer is null)
        {
            return;
        }

        UpdateLayout();
        _gridControl.ScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
    }

    /// <summary>
    /// Handles the ColumnReordered event to recompute the summary values.
    /// </summary>
    private void GridControl_ColumnReordered(object? sender, DataGridColumnEventArgs e)
    {
        CalculateSummaryValues();
    }

    /// <summary>
    /// Handles the CollectionChanged event of the GridControl columns to recompute the summary values.
    /// </summary>
    private void Columns_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CalculateSummaryValues();
    }

    #endregion
}
