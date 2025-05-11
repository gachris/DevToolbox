using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DevToolbox.Wpf.Data;
using DevToolbox.Wpf.Extensions;

namespace DevToolbox.Wpf.Controls;

[TemplatePart(Name = "PART_DropDown", Type = typeof(DropDown))]
[TemplatePart(Name = "PART_ListBox", Type = typeof(ListBox))]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class DataGridColumnHeaderFilter : Control
{
    #region Fields/Consts

    private bool _isLoading;
    private DropDown? _dropDown;
    private ListBox? _listBox;

    #endregion

    static DataGridColumnHeaderFilter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridColumnHeaderFilter), new FrameworkPropertyMetadata(typeof(DataGridColumnHeaderFilter)));
    }

    #region Methods Overrides

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_dropDown != null)
        {
            _dropDown.Opened -= DropDown_Opened;
        }

        _dropDown = Template.FindName("PART_DropDown", this) as DropDown;

        if (_dropDown != null)
        {
            _dropDown.Opened += DropDown_Opened;
        }

        if (_listBox != null)
        {
            _listBox.SelectionChanged -= ItemsSelectionChanged;
        }

        _listBox = Template.FindName("PART_ListBox", this) as ListBox;

        if (_listBox != null)
        {
            _listBox.SelectionChanged += ItemsSelectionChanged;
        }
    }

    #endregion

    #region Events Subscriptions

    private void DropDown_Opened(object? sender, EventArgs e)
    {
        if (_dropDown == null || _listBox == null)
        {
            return;
        }

        var column = this.FindVisualAncestor<DataGridColumnHeader>();
        if (column == null || string.IsNullOrEmpty(column.Column.SortMemberPath))
        {
            return;
        }

        var dataGrid = this.FindVisualAncestor<DataGrid>();
        if (dataGrid == null)
        {
            return;
        }

        _isLoading = true;

        if (!dataGrid.CommitEdit())
        {
            _isLoading = false;
            return;
        }

        var autoFilter = AutoFilter.GetAutoFilter(dataGrid);
        if (autoFilter is null)
        {
            _isLoading = false;
            return;
        }

        var previousValues = (ObservableCollection<FilterListItem>)_listBox.ItemsSource;
        var currentValues = autoFilter.GetDistictValues(dataGrid, column.Column.SortMemberPath) ?? [];

        if (previousValues != null)
        {
            foreach (var item in currentValues)
            {
                if (!previousValues.Any(c => Convert.ToString(c.Name) == Convert.ToString(item.Name) && !c.IsSelectAll) && !item.IsSelectAll)
                {
                    previousValues.Add(new FilterListItem { Name = item.Name });
                }
            }
        }
        else
        {
            previousValues = currentValues;
        }

        _listBox.ItemsSource = previousValues;

        _isLoading = false;
    }

    private void ItemsSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoading || _dropDown is null)
        {
            return;
        }

        var dataGrid = this.FindVisualAncestor<DataGrid>();
        var gridColumnHeader = this.FindVisualAncestor<DataGridColumnHeader>();
        if (dataGrid is null || gridColumnHeader is null)
        {
            return;
        }

        _isLoading = true;
        var sortMemberPath = gridColumnHeader.Column.SortMemberPath;

        var autoFilter = AutoFilter.GetAutoFilter(dataGrid);
        if (autoFilter is null)
        {
            _isLoading = false;
            return;
        }

        if (e.AddedItems?.Count > 0 && e.AddedItems[0] is FilterListItem checkedListItem)
        {
            if (!checkedListItem.IsSelectAll)
            {
                autoFilter.ApplyFilters(dataGrid, sortMemberPath, checkedListItem.Name!);
            }
            else
            {
                SelectAllItems();
                autoFilter.RemoveAllFilter(sortMemberPath);
            }
        }

        _isLoading = false;
    }

    private void SelectAllItems()
    {
        if (_listBox?.ItemsSource is ObservableCollection<FilterListItem> distinctValues)
        {
            foreach (var item in distinctValues)
            {
                if (!item.IsSelectAll)
                {
                    item.IsChecked = true;
                }
            }
        }
    }

    #endregion
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member