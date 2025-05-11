using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DevToolbox.Wpf.Data;

#nullable disable

namespace DevToolbox.Wpf.Controls;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class AutoFilter : DependencyObject
{
    #region Fields/Consts

    private readonly DataGrid _currentGrid;
    private ICollectionView _view;
    private DataTable _collectionViewDt;

    public static readonly DependencyProperty AutoFilterProperty =
        DependencyProperty.RegisterAttached("AutoFilter", typeof(AutoFilter), typeof(AutoFilter), new PropertyMetadata(default));

    public static readonly DependencyProperty FilterExpressionProperty =
        DependencyProperty.Register("FilterExpression", typeof(string), typeof(AutoFilter), new FrameworkPropertyMetadata(default, OnFilterExpressionChanged));

    #endregion

    public string FilterExpression
    {
        get { return (string)GetValue(FilterExpressionProperty); }
        set { SetValue(FilterExpressionProperty, value); }
    }

    public AutoFilter(DataGrid grid)
    {
        _currentGrid = grid;
    }

    public static AutoFilter GetAutoFilter(DependencyObject obj)
    {
        return (AutoFilter)obj.GetValue(AutoFilterProperty);
    }

    public static void SetAutoFilter(DependencyObject obj, AutoFilter value)
    {
        obj.SetValue(AutoFilterProperty, value);
    }

    private static void OnFilterExpressionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var autoFilter = (AutoFilter)d;
        autoFilter.OnFilterExpressionChanged((string)e.OldValue, (string)e.NewValue);
    }

    private void OnFilterExpressionChanged(string oldValue, string newValue)
    {
        if (_view is null)
        {
            return;
        }

        UpdateFilter();

        _view.Filter = null;

        if (!string.IsNullOrEmpty(newValue))
        {
            _view.Filter = FilterPredicate;
        }
    }

    public ObservableCollection<FilterListItem> GetDistictValues(DataGrid grid, string columnName)
    {
        if (grid.ItemsSource is DataView itemsSource)
        {
            var ditictValues = new ObservableCollection<FilterListItem>() { new FilterListItem() { IsChecked = false, Name = "All", IsSelectAll = true } };
            var table = itemsSource.Table;
            var filteredValues = GetFilteredColumnValues(columnName, itemsSource);
            foreach (DataRow row in table.Rows)
            {
                var value = row[columnName];
                if (ditictValues.Count(c => Convert.ToString(c.Name) == Convert.ToString(value) && !c.IsSelectAll) == 0)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(value)))
                        ditictValues.Add(new FilterListItem { Name = value, IsChecked = filteredValues.Contains("'" + value + "'") });
                    else if (value == null)
                        ditictValues.Add(new FilterListItem { Name = null, IsChecked = filteredValues.Contains(null) });
                    else if (string.IsNullOrEmpty(Convert.ToString(value)))
                        ditictValues.Add(new FilterListItem { Name = value, IsChecked = filteredValues.Contains("") });
                }
            }
            if (ditictValues.Count(c => c.IsChecked) == ditictValues.Count - 1)
                ditictValues[0].IsChecked = true;
            return ditictValues;
        }
        else if (CollectionViewSource.GetDefaultView(grid.ItemsSource) != null)
        {
            var ditictValues = new ObservableCollection<FilterListItem>() { new FilterListItem() { IsChecked = false, Name = "All", IsSelectAll = true } };
            var view = CollectionViewSource.GetDefaultView(grid.ItemsSource);
            _view = view;
            var unquieValues = new HashSet<string>();
            foreach (var rowData in grid.ItemsSource)
            {
                var propertyValue = rowData.GetType().GetProperty(columnName);
                if (propertyValue != null)
                {
                    var data = propertyValue.GetValue(rowData, null) == null ? null : Convert.ToString(propertyValue.GetValue(rowData, null));
                    if (data != null && !unquieValues.Contains(data))
                        unquieValues.Add(data);
                }
            }
            var filteredValues = string.IsNullOrEmpty(FilterExpression) ? new List<string>() : GetFilteredColumnValues(columnName, FilterExpression);
            foreach (var value in unquieValues)
            {
                if (ditictValues.Count(c => Convert.ToString(c.Name) == value && !c.IsSelectAll) == 0)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(value)))
                        ditictValues.Add(new FilterListItem { Name = value, IsChecked = filteredValues.Contains("'" + value + "'") });
                    else if (value == null)
                        ditictValues.Add(new FilterListItem { Name = null, IsChecked = filteredValues.Contains(null) });
                    else if (string.IsNullOrEmpty(Convert.ToString(value)))
                        ditictValues.Add(new FilterListItem { Name = value, IsChecked = filteredValues.Contains("") });
                }
            }
            return ditictValues;
        }

        return null;
    }

    public void ApplyFilters(DataGrid grid, string columnName, object value)
    {
        value = AddEscapeSequece(value);
        if (grid.ItemsSource is DataView itemsSource)
        {
            if (itemsSource == null) return;
            if (!string.IsNullOrEmpty(itemsSource.RowFilter))
            {
                string filter = FilterGenerator(itemsSource.RowFilter, columnName, value);
                if (filter != itemsSource.RowFilter)
                {
                    itemsSource.RowFilter = CorrectRowFilter(filter);
                    grid.Items.Refresh();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(Convert.ToString(value)))
                {
                    var dummy = value == null ? null : "";
                    switch (dummy)
                    {
                        case "":
                            itemsSource.RowFilter = "[" + columnName + "]" + " = ''";
                            break;
                        case null:
                            itemsSource.RowFilter = "[" + columnName + "]" + " IS Null";
                            break;
                    }
                }
                else
                    if (itemsSource.Table.Columns[columnName].DataType.BaseType.ToString() == "System.Enum")
                {
                    var value1 = Convert.ToInt32(Enum.Parse(itemsSource.Table.Columns[columnName].DataType, Convert.ToString(value)));
                    itemsSource.RowFilter = "[" + columnName + "]" + " " + " IN " + "(" + value1 + ")";
                }
                else
                    itemsSource.RowFilter = "[" + columnName + "]" + " " + " IN " + "(" + "'" + value + "'" + ")";
            }

            FilterExpression = itemsSource.RowFilter;
        }
        else if (CollectionViewSource.GetDefaultView(grid.ItemsSource) != null)
        {
            _view = CollectionViewSource.GetDefaultView(grid.ItemsSource);
            if (!string.IsNullOrEmpty(FilterExpression))
            {
                string filter = FilterGenerator(FilterExpression, columnName, value);
                if (filter != FilterExpression)
                {
                    FilterExpression = CorrectRowFilter(filter);
                    grid.Items.Refresh();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(Convert.ToString(value)))
                {
                    var dummy = value == null ? null : "";
                    switch (dummy)
                    {
                        case "":
                            FilterExpression = "[" + columnName + "]" + " = ''";
                            break;
                        case null:
                            FilterExpression = "[" + columnName + "]" + " IS Null";
                            break;
                    }
                }
                else
                {
                    var readOnlyCollection = ((ListCollectionView)_view).ItemProperties;
                    if (readOnlyCollection != null)
                    {
                        var item =
                            readOnlyCollection.First(
                                c => c.Name == columnName);
                        if (item.PropertyType.BaseType != null && item.PropertyType.BaseType.ToString() == "System.Enum")
                        {
                            var value1 = Convert.ToInt32(Enum.Parse(item.PropertyType, Convert.ToString(value)));
                            FilterExpression = "[" + columnName + "]" + " " + " IN " + "(" + value1 + ")";
                        }
                        else
                            FilterExpression = "[" + columnName + "]" + " " + " IN " + "(" + "'" + value + "'" + ")";
                    }
                }
            }
        }
    }

    private object AddEscapeSequece(object value)
    {
        if (string.IsNullOrEmpty(Convert.ToString(value))) return value;
        string newValue = EscapeLikeValue(value.ToString());
        return newValue;
    }

    private string FilterNullAndBlanlValues(object value, string columnName, string existingFilter, int startIndex)
    {
        string newFilter = existingFilter.Trim();
        if (startIndex != -1)
        {
            if (Convert.ToString(value) == "" && value != null)
            {
                //NOT TESTED
                int startIndex1 = newFilter.IndexOf("[" + columnName + "]" + " = ''", StringComparison.Ordinal);
                if (startIndex1 != -1)
                {
                    int lastIndex1 = startIndex1 + Convert.ToString("[" + columnName + "]" + " = ''").Length;
                    if (lastIndex1 == newFilter.Length)
                    {
                        if (!newFilter.Contains("[" + columnName + "]" + " IS Null"))
                        {
                            newFilter = newFilter.Insert(lastIndex1, " Or ([" + columnName + "]" + " IS Null)");
                        }
                    }
                    else
                    {
                        if (!newFilter.Contains("[" + columnName + "]" + " = ''"))
                        {
                            newFilter = newFilter.Insert(lastIndex1, " Or ([" + columnName + "]" + " = '') AND");
                        }
                    }
                }
                else
                {
                    //TESTED
                    string actaulFilter = newFilter.Substring(startIndex + (columnName + " " + " IN ").Length + 1);
                    int lastIndex = actaulFilter.IndexOf(")", StringComparison.Ordinal);
                    actaulFilter = actaulFilter.Substring(0, lastIndex);
                    int isNullLastIndex = newFilter.IndexOf("[" + columnName + "] IS Null)", StringComparison.Ordinal);
                    int lastIndex1 = startIndex + ("[" + columnName + "]" + " " + " IN ").Length +
                                     actaulFilter.Length;
                    if (lastIndex1 < isNullLastIndex)
                    {
                        lastIndex1 = isNullLastIndex + Convert.ToString("[" + columnName + "] IS Null)").Length;
                    }
                    if (lastIndex1 + 1 == newFilter.Length) lastIndex1 = newFilter.Length;
                    if (lastIndex1 == newFilter.Length)
                    {
                        if (!newFilter.Contains("[" + columnName + "]" + " = ''"))
                        {
                            newFilter = newFilter.Insert(lastIndex1, " Or ([" + columnName + "]" + " = '')");
                        }
                    }
                    else
                    {
                        if (!newFilter.Contains("[" + columnName + "]" + " = ''"))
                        {
                            newFilter = newFilter.Insert(lastIndex1, " Or ([" + columnName + "]" + " = '') AND");
                        }
                    }
                }
            }
            else if (value == null)
            {
                //NOT TESTED
                int startIndex1 = newFilter.IndexOf("[" + columnName + "]" + " IS Null)", StringComparison.Ordinal);
                if (startIndex1 != -1)
                {
                    if (!newFilter.Contains("[" + columnName + "]" + " IS Null"))
                    {
                        int lastIndex1 = startIndex1 + Convert.ToString("[" + columnName + "]" + " IS Null)").Length;
                        int x = 0;
                        if (lastIndex1 == newFilter.Length)
                            x = 0;
                        else if (lastIndex1 + 1 == newFilter.Length)
                            x = 1;
                        newFilter = lastIndex1 + x == newFilter.Length
                                        ? newFilter.Insert(lastIndex1, " Or ([" + columnName + "]" + " IS Null)")
                                        : newFilter.Insert(lastIndex1, " Or ([" + columnName + "]" + " IS Null) AND");
                    }
                }
                else
                {
                    //TESTED
                    if (!newFilter.Contains("[" + columnName + "]" + " IS Null"))
                    {
                        string actaulFilter =
                            newFilter.Substring(startIndex + (columnName + " " + " IN ").Length + 1);
                        int lastIndex = actaulFilter.IndexOf(")", StringComparison.Ordinal);
                        actaulFilter = actaulFilter.Substring(0, lastIndex);


                        int isNullLastIndex = newFilter.IndexOf("([" + columnName + "]" + " = '')", StringComparison.Ordinal);
                        int lastIndex1 = startIndex + ("[" + columnName + "]" + " " + " IN ").Length +
                                         actaulFilter.Length + 1;
                        if (isNullLastIndex > lastIndex1)
                        {
                            lastIndex1 = isNullLastIndex +
                                         Convert.ToString("([" + columnName + "]" + " = '')").Length;
                        }
                        if (lastIndex1 > newFilter.Length)
                            lastIndex1 = newFilter.Length;

                        newFilter = lastIndex1 == newFilter.Length
                                        ? newFilter.Insert(lastIndex1, " Or ([" + columnName + "]" + " IS Null)")
                                        : newFilter.Insert(lastIndex1, " Or ([" + columnName + "]" + " IS Null) AND");
                    }
                }
            }
        }
        else if (newFilter.Contains("[" + columnName + "]" + " = ''"))
        {
            if (Convert.ToString(value) == "")
            {
                //NOT TESTED
                int startIndex1 = newFilter.IndexOf("[" + columnName + "]" + " = ''", StringComparison.Ordinal);
                if (startIndex1 != -1)
                {
                    int lastIndex1 = startIndex1 + Convert.ToString("[" + columnName + "]" + " = ''").Length;
                    int x = 0;
                    if (lastIndex1 == newFilter.Length)
                        x = 0;
                    else if (lastIndex1 + 1 == newFilter.Length)
                        x = 1;
                    if (!newFilter.Contains("[" + columnName + "]" + " IS Null"))
                    {
                        newFilter = lastIndex1 + x == newFilter.Length
                                        ? newFilter.Insert(lastIndex1, " Or ([" + columnName + "]" + " IS Null)")
                                        : newFilter.Insert(lastIndex1, " Or ([" + columnName + "]" + " IS Null) AND");
                    }
                }
            }
            else
            {
                //NOT TESTED
                int startIndex1 = newFilter.IndexOf("[" + columnName + "]" + " = ''", StringComparison.Ordinal);
                int startIndex2 = newFilter.IndexOf("[" + columnName + "]" + " IS Null", StringComparison.Ordinal);
                if (startIndex2 != -1)
                {
                    startIndex1 = Math.Min(startIndex1, startIndex2);
                }
                if (startIndex1 != -1)
                {
                    newFilter = newFilter.Insert(startIndex1,
                                                 "[" + columnName + "]" + " " + " IN " + "(" + "'" + value + "'" +
                                                 ") Or ");
                }
            }
        }
        else if (newFilter.Contains("[" + columnName + "]" + " IS Null"))
        {
            if (Convert.ToString(value) == "")
            {
                //TESTED
                int startIndex1 = newFilter.IndexOf("[" + columnName + "]" + " IS Null", StringComparison.Ordinal);
                if (startIndex1 != -1)
                {
                    int lastIndex1 = startIndex1 + Convert.ToString("[" + columnName + "]" + " IS Null").Length;
                    if (!newFilter.Contains("[" + columnName + "]" + " = ''"))
                    {
                        int x = 0;
                        if (lastIndex1 == newFilter.Length)
                            x = 0;
                        else if (lastIndex1 + 1 == newFilter.Length)
                            x = 1;
                        else if (lastIndex1 + 2 == newFilter.Length)
                            x = 2;
                        newFilter = lastIndex1 + x == newFilter.Length
                                        ? newFilter.Insert(lastIndex1, " Or ([" + columnName + "]" + " = '')")
                                        : newFilter.Insert(lastIndex1, " Or ([" + columnName + "]" + " = '') AND");
                    }
                }
            }
            else
            {
                //NOT TESTED
                int startIndex1 = newFilter.IndexOf("[" + columnName + "]" + " = ''", StringComparison.Ordinal);
                int startIndex2 = newFilter.IndexOf("[" + columnName + "]" + " IS Null", StringComparison.Ordinal);
                if (startIndex2 != -1)
                {
                    startIndex1 = Math.Min(startIndex1, startIndex2);
                }
                if (startIndex1 != -1)
                {
                    newFilter = newFilter.Insert(startIndex1,
                                                 "[" + columnName + "]" + " " + " IN " + "(" + "'" + value + "'" +
                                                 ") Or ");
                }
            }
        }
        else
        {
            if (Convert.ToString(value) == "" && value != null)
            {
                newFilter = newFilter + "AND ([" + columnName + "]" + " = '')";
            }
            else if (value == null)
            {
                newFilter = newFilter + "AND ([" + columnName + "]" + " IS Null)";
            }
        }

        return newFilter;
    }

    private string FilterGenerator(string exisitngFilter, string columnName, object value)
    {
        var isEnumType = false;
        int enumValue = -1;
        if (_currentGrid.ItemsSource is DataView)
        {
            var dv = _currentGrid.ItemsSource as DataView;
            var baseType = dv.Table.Columns[columnName].DataType.BaseType;
            if (baseType != null)
            {
                isEnumType = baseType.ToString().Equals("System.Enum");
                if (isEnumType)
                    enumValue = Convert.ToInt32(Enum.Parse(dv.Table.Columns[columnName].DataType, Convert.ToString(value)));
            }
        }
        else if (CollectionViewSource.GetDefaultView(_currentGrid.ItemsSource) != null)
        {
            var readOnlyCollection = ((ListCollectionView)CollectionViewSource.GetDefaultView(_currentGrid.ItemsSource)).ItemProperties;
            if (readOnlyCollection != null)
            {
                var item =
                    readOnlyCollection.First(
                        c => c.Name == columnName);
                if (item.PropertyType.BaseType != null && item.PropertyType.BaseType.ToString() == "System.Enum")
                {
                    isEnumType = true;
                    enumValue = Convert.ToInt32(Enum.Parse(item.PropertyType, Convert.ToString(value)));
                }
            }
        }
        string newFilter = exisitngFilter;

        if (newFilter.Contains("[" + columnName + "]" + " " + " IN ") ||
            newFilter.Contains("[" + columnName + "]" + " = ''") ||
            newFilter.Contains("[" + columnName + "]" + " IS Null"))
        {
            int startIndex = newFilter.IndexOf("[" + columnName + "]" + " " + " IN ", StringComparison.Ordinal);
            if (startIndex != -1)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(value)))
                {
                    string actaulFilter =
                        newFilter.Substring(startIndex + ("[" + columnName + "]" + " " + " IN ").Length + 1);
                    int lastIndex = actaulFilter.IndexOf(")", StringComparison.Ordinal);
                    actaulFilter = actaulFilter.Substring(0, lastIndex);
                    string[] listOfFilter = actaulFilter.Split(',');
                    if (listOfFilter.Contains("'" + value + "'"))
                        return exisitngFilter;
                    int indexToput = startIndex + ("[" + columnName + "]" + " " + " IN ").Length +
                                     actaulFilter.Length + 1;
                    newFilter = !isEnumType ? newFilter.Insert(indexToput, "," + "'" + value + "'") : newFilter.Insert(indexToput, "," + enumValue);
                }
                else
                {
                    newFilter = FilterNullAndBlanlValues(value, columnName, newFilter, startIndex);
                }
            }
            else
            {
                newFilter = FilterNullAndBlanlValues(value, columnName, newFilter, startIndex);
            }

            return newFilter.Trim();
        }
        newFilter = newFilter.Trim();

        if (!string.IsNullOrEmpty(newFilter))
        {
            if (!string.IsNullOrEmpty(Convert.ToString(value)))
            {
                int idexAppliedOn = _currentGrid.Columns.Count(gridColumn => newFilter.Contains("[" + gridColumn.SortMemberPath + "]" + " " + " IN ") || newFilter.Contains("[" + gridColumn.SortMemberPath + "]" + " = ''") || newFilter.Contains("[" + gridColumn.SortMemberPath + "]" + " IS Null"));
                if (idexAppliedOn == 1)
                    newFilter = "(" + newFilter + ")";
                newFilter = !isEnumType
                    ? newFilter + " AND " + "(" + "[" + columnName + "]" + " " + " IN " + "(" + "'" + value +
                              "'" + ")" + ")"
                    : newFilter + " AND " + "(" + "[" + columnName + "]" + " " + " IN " + "(" + enumValue +
                           ")" + ")";
            }
            else
            {
                int startIndex = newFilter.IndexOf("[" + columnName + "]" + " " + " IN ", StringComparison.Ordinal);
                newFilter = FilterNullAndBlanlValues(value, columnName, newFilter, startIndex);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(Convert.ToString(value)))
            {
                newFilter = !isEnumType
                    ? "[" + columnName + "]" + " " + " IN " + "(" + "'" + value + "'" + ")"
                    : "[" + columnName + "]" + " " + " IN " + "(" + value + ")";
            }
            else
            {
                int startIndex = newFilter.IndexOf("[" + columnName + "]" + " " + " IN ", StringComparison.Ordinal);
                newFilter = FilterNullAndBlanlValues(value, columnName, newFilter, startIndex);
            }
        }


        return newFilter.Trim();
    }

    private List<string> GetFilteredColumnValues(string columnName, DataView view)
    {
        string newFilter = view.RowFilter;
        if (newFilter.Contains("[" + columnName + "]" + " " + " IN "))
        {
            int startIndex = newFilter.IndexOf("[" + columnName + "]" + " " + " IN ", StringComparison.Ordinal);
            string actaulFilter =
                newFilter.Substring(startIndex + ("[" + columnName + "]" + " " + " IN ").Length + 1);
            int lastIndex = actaulFilter.IndexOf(")", StringComparison.Ordinal);
            actaulFilter = actaulFilter.Substring(0, lastIndex);
            string[] listOfFilter = actaulFilter.Split(',');
            List<string> list = listOfFilter.ToList();
            var newList = new List<string>();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].IndexOf("'", StringComparison.Ordinal) == 0 && list[i].LastIndexOf("'", StringComparison.Ordinal) == list[i].Length - 1)
                {
                    newList.Add(list[i]);
                    continue;
                }
                string newfilterYoBeAdded = list[i];

                for (int j = i + 1; j < list.Count; j++)
                {
                    i = j;
                    string current = list[j];
                    if (current.LastIndexOf("'", StringComparison.Ordinal) == current.Length - 1)
                    {
                        if (current.Length > 1 && current[current.Length - 2].ToString(CultureInfo.InvariantCulture) != "'")
                        {
                            newfilterYoBeAdded = newfilterYoBeAdded + "," + current;
                            newList.Add(newfilterYoBeAdded);
                            break;
                        }
                    }
                    else
                    {
                        newfilterYoBeAdded = newfilterYoBeAdded + "," + current;
                    }
                }
            }
            if (newFilter.Contains("([" + columnName + "] IS Null)"))
            {
                newList.Add(null);
            }
            if (newFilter.Contains("([" + columnName + "] = '')"))
            {
                newList.Add("");
            }

            return newList;
        }

        return new List<string>();
    }

    private List<string> GetFilteredColumnValues(string columnName, string rowFilter)
    {
        string newFilter = rowFilter;
        if (newFilter.Contains("[" + columnName + "]" + " " + " IN "))
        {
            int startIndex = newFilter.IndexOf("[" + columnName + "]" + " " + " IN ", StringComparison.Ordinal);
            string actaulFilter =
                newFilter.Substring(startIndex + ("[" + columnName + "]" + " " + " IN ").Length + 1);
            int lastIndex = actaulFilter.IndexOf(")", StringComparison.Ordinal);
            actaulFilter = actaulFilter.Substring(0, lastIndex);
            string[] listOfFilter = actaulFilter.Split(',');
            List<string> list = listOfFilter.ToList();
            var newList = new List<string>();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].IndexOf("'", StringComparison.Ordinal) == 0 && list[i].LastIndexOf("'", StringComparison.Ordinal) == list[i].Length - 1)
                {
                    newList.Add(list[i]);
                    continue;
                }
                string newfilterYoBeAdded = list[i];

                for (int j = i + 1; j < list.Count; j++)
                {
                    i = j;
                    string current = list[j];
                    if (current.LastIndexOf("'", StringComparison.Ordinal) == current.Length - 1)
                    {
                        if (current.Length > 1 && current[current.Length - 2].ToString(CultureInfo.InvariantCulture) != "'")
                        {
                            newfilterYoBeAdded = newfilterYoBeAdded + "," + current;
                            newList.Add(newfilterYoBeAdded);
                            break;
                        }
                    }
                    else
                    {
                        newfilterYoBeAdded = newfilterYoBeAdded + "," + current;
                    }
                }
            }
            if (newFilter.Contains("([" + columnName + "] IS Null)"))
            {
                newList.Add(null);
            }
            if (newFilter.Contains("([" + columnName + "] = '')"))
            {
                newList.Add("");
            }

            return newList;
        }

        return new List<string>();
    }

    public void RemoveAllFilter(string currentColumn)
    {
        if (_currentGrid.ItemsSource == null) return;
        string newFilter = null;
        if (_currentGrid.ItemsSource is DataView)
        {
            var itemsSource = (DataView)_currentGrid.ItemsSource;
            newFilter = itemsSource.RowFilter;
        }
        else if (CollectionViewSource.GetDefaultView(_currentGrid.ItemsSource) != null)
        {
            newFilter = FilterExpression;
        }

        if (newFilter is null || string.IsNullOrEmpty(newFilter))
        {
            return;
        }

        if (newFilter.Contains("[" + currentColumn + "]" + " " + " IN "))
        {
            int startIndex = newFilter.IndexOf("[" + currentColumn + "]" + " " + " IN ", StringComparison.Ordinal);
            string actaulFilter =
                newFilter.Substring(startIndex + ("[" + currentColumn + "]" + " " + " IN ").Length + 1);
            int lastIndex = actaulFilter.IndexOf(")", StringComparison.Ordinal);
            actaulFilter = actaulFilter.Substring(0, lastIndex);
            string relalValue = "[" + currentColumn + "]" + " " + " IN " + "(" + actaulFilter + ")";
            if (newFilter.Contains("(" + "[" + relalValue + "]" + ")"))
            {
                newFilter = newFilter.Replace("(" + "[" + relalValue + "]" + ")", "");
                if (newFilter.IndexOf("( AND (", StringComparison.Ordinal) == 0)
                {
                    newFilter = newFilter.Replace("( AND (", "");
                }
                if (newFilter.IndexOf("(((", StringComparison.Ordinal) == 0)
                {
                    newFilter = newFilter.Substring(2);
                }
                if (newFilter.Contains("))) AND "))
                {
                    newFilter = newFilter.Replace("))) AND ", ")");
                }

                if (newFilter.LastIndexOf(" AND", StringComparison.Ordinal) == newFilter.Length - 5)
                {
                    newFilter = newFilter.Substring(0, newFilter.Length - 5);
                }
                if (newFilter.Contains(" AND )"))
                {
                    newFilter = newFilter.Replace(" AND )", "");
                }
                if (newFilter.IndexOf("((", StringComparison.Ordinal) == 0)
                {
                    newFilter = newFilter.Substring(2);
                }
                if (newFilter.IndexOf(" AND ", StringComparison.Ordinal) == 0)
                {
                    newFilter = newFilter.Substring(5);
                }
            }
            else
            {
                newFilter = newFilter.Replace(relalValue, "");
                if (newFilter == "')")
                {
                    newFilter = "";

                    if (_currentGrid.ItemsSource is DataView itemsSource)
                    {
                        itemsSource.RowFilter = CorrectRowFilter(newFilter);
                    }
                    else if (CollectionViewSource.GetDefaultView(_currentGrid.ItemsSource) != null)
                    {
                        FilterExpression = CorrectRowFilter(newFilter);
                    }
                    return;
                }
            }
        }

        newFilter = RemoveIsNullAndBlankFilter(newFilter, currentColumn);

        if (_currentGrid.ItemsSource is DataView)
        {
            var itemsSource = (DataView)_currentGrid.ItemsSource;
            itemsSource.RowFilter = CorrectRowFilter(newFilter);
            FilterExpression = itemsSource.RowFilter;
        }
        else if (CollectionViewSource.GetDefaultView(_currentGrid.ItemsSource) != null)
        {
            FilterExpression = CorrectRowFilter(newFilter);
        }

        _currentGrid.Items.Refresh();
    }

    public void RemoveFilters(DataGrid grid, string columnName, object value)
    {
        var isEnumType = false;
        var enumValue = -1;
        if (_currentGrid.ItemsSource is DataView)
        {
            var dv = _currentGrid.ItemsSource as DataView;
            var baseType = dv?.Table?.Columns[columnName]?.DataType.BaseType;
            if (baseType != null)
            {
                isEnumType = baseType.ToString().Equals("System.Enum");
                if (isEnumType)
                {
                    enumValue = Convert.ToInt32(Enum.Parse(dv.Table.Columns[columnName].DataType, Convert.ToString(value)));
                }
            }
        }
        else if (CollectionViewSource.GetDefaultView(_currentGrid.ItemsSource) != null)
        {
            var readOnlyCollection = ((ListCollectionView)CollectionViewSource.GetDefaultView(_currentGrid.ItemsSource)).ItemProperties;
            if (readOnlyCollection != null)
            {
                var item = readOnlyCollection.First(c => c.Name == columnName);
                if (item.PropertyType.BaseType != null && item.PropertyType.BaseType.ToString() == "System.Enum")
                {
                    isEnumType = true;
                    enumValue = Convert.ToInt32(Enum.Parse(item.PropertyType, Convert.ToString(value)));
                }
            }
        }
        value = AddEscapeSequece(value);
        string rowFilter = null;
        if (grid.ItemsSource == null)
            return;
        if (grid.ItemsSource is DataView dataView)
        {
            rowFilter = dataView.RowFilter;
        }
        else if (CollectionViewSource.GetDefaultView(grid.ItemsSource) != null)
        {
            rowFilter = GetAutoFilter(grid).FilterExpression;
        }

        if (!string.IsNullOrEmpty(rowFilter))
        {
            var newFilter = rowFilter;
            if (newFilter != null && newFilter.Contains("[" + columnName + "]" + " " + " IN ") &&
                !string.IsNullOrEmpty(Convert.ToString(value)))
            {
                int startIndex = newFilter.IndexOf("[" + columnName + "]" + " " + " IN ", StringComparison.Ordinal);
                string actaulFilter =
                    newFilter.Substring(startIndex + ("[" + columnName + "]" + " " + " IN ").Length + 1);
                int lastIndex = actaulFilter.LastIndexOf(")", StringComparison.Ordinal);
                actaulFilter = actaulFilter.Substring(0, lastIndex);
                string[] listOfFilter = actaulFilter.Split(',');
                if (!isEnumType)
                {
                    if (listOfFilter.Contains("'" + value + "'"))
                    {
                        string realFilter = "[" + columnName + "]" + " " + " IN " + "(" + actaulFilter + ")";
                        string replaced = realFilter.Replace("'" + value + "'", "");
                        if (replaced.Contains(",,"))
                        {
                            replaced = replaced.Replace(",,", ",");
                        }
                        if (replaced.Contains(",)"))
                        {
                            replaced = replaced.Replace(",)", ")");
                        }
                        if (replaced.Contains("()"))
                        {
                            replaced = "";
                        }
                        if (replaced.Contains("(,"))
                        {
                            replaced = replaced.Replace("(,", "(");
                        }
                        if (replaced.Contains(",)"))
                        {
                            replaced = replaced.Replace(",)", ")");
                        }
                        if (newFilter.Contains(" AND ()"))
                        {
                            newFilter = newFilter.Replace(" AND ()", "");
                        }
                        newFilter = newFilter.Replace(realFilter, replaced);
                        if (newFilter.Contains("() AND "))
                        {
                            newFilter = newFilter.Replace("() AND ", "");
                        }
                        if (newFilter.IndexOf("(((", StringComparison.Ordinal) == 0)
                        {
                            newFilter = newFilter.Substring(2);
                        }
                        if (newFilter.Contains(")))"))
                        {
                            newFilter = newFilter.Replace(")))", "))");
                        }
                        if (newFilter.Contains("()"))
                        {
                            newFilter = newFilter.Replace("()", "");
                        }
                        if (newFilter.LastIndexOf(" AND ", StringComparison.Ordinal) == newFilter.Length - 5)
                        {
                            newFilter = newFilter.Substring(0, newFilter.Length - 5);
                        }
                        if (newFilter.IndexOf("((", StringComparison.Ordinal) == 0)
                        {
                            newFilter = newFilter.Substring(1);
                        }
                        newFilter = newFilter.Trim();
                        if (newFilter.IndexOf("Or", StringComparison.Ordinal) == 0)
                        {
                            newFilter = newFilter.Substring(3);
                        }

                        switch (newFilter)
                        {
                            case "":
                                if (grid.ItemsSource is DataView view)
                                {
                                    view.RowFilter = null;
                                }
                                else if (CollectionViewSource.GetDefaultView(grid.ItemsSource) != null)
                                {
                                    GetAutoFilter(grid).FilterExpression = null;
                                }
                                return;
                            default:
                                if (grid.ItemsSource is DataView defaultView)
                                {
                                    defaultView.RowFilter = CorrectRowFilter(newFilter);
                                }
                                else if (CollectionViewSource.GetDefaultView(grid.ItemsSource) != null)
                                {
                                    GetAutoFilter(grid).FilterExpression = CorrectRowFilter(newFilter);
                                }
                                break;
                        }
                    }
                }
                else
                {
                    if (listOfFilter.Contains(enumValue.ToString(CultureInfo.InvariantCulture)))
                    {
                        string realFilter = "[" + columnName + "]" + " " + " IN " + "(" + actaulFilter + ")";
                        string replaced = realFilter.Replace(enumValue.ToString(CultureInfo.InvariantCulture), "");
                        if (replaced.Contains(",,"))
                        {
                            replaced = replaced.Replace(",,", ",");
                        }
                        if (replaced.Contains(",)"))
                        {
                            replaced = replaced.Replace(",)", ")");
                        }
                        if (replaced.Contains("()"))
                        {
                            replaced = "";
                        }
                        if (replaced.Contains("(,"))
                        {
                            replaced = replaced.Replace("(,", "(");
                        }
                        if (replaced.Contains(",)"))
                        {
                            replaced = replaced.Replace(",)", ")");
                        }
                        if (newFilter.Contains(" AND ()"))
                        {
                            newFilter = newFilter.Replace(" AND ()", "");
                        }
                        newFilter = newFilter.Replace(realFilter, replaced);
                        if (newFilter.Contains("() AND "))
                        {
                            newFilter = newFilter.Replace("() AND ", "");
                        }
                        if (newFilter.IndexOf("(((", StringComparison.Ordinal) == 0)
                        {
                            newFilter = newFilter.Substring(2);
                        }
                        if (newFilter.Contains(")))"))
                        {
                            newFilter = newFilter.Replace(")))", "))");
                        }
                        if (newFilter.Contains("()"))
                        {
                            newFilter = newFilter.Replace("()", "");
                        }
                        if (newFilter.LastIndexOf(" AND ", StringComparison.Ordinal) == newFilter.Length - 5)
                        {
                            newFilter = newFilter.Substring(0, newFilter.Length - 5);
                        }
                        if (newFilter.IndexOf("((", StringComparison.Ordinal) == 0)
                        {
                            newFilter = newFilter.Substring(1);
                        }
                        newFilter = newFilter.Trim();
                        if (newFilter.IndexOf("Or", StringComparison.Ordinal) == 0)
                        {
                            newFilter = newFilter.Substring(3);
                        }

                        switch (newFilter)
                        {
                            case "":
                                if (grid.ItemsSource is DataView view)
                                {
                                    view.RowFilter = null;
                                }
                                else if (CollectionViewSource.GetDefaultView(grid.ItemsSource) != null)
                                {
                                    GetAutoFilter(grid).FilterExpression = null;
                                }
                                return;
                            default:
                                if (grid.ItemsSource is DataView DataView)
                                {
                                    DataView.RowFilter = CorrectRowFilter(newFilter);
                                }
                                else if (CollectionViewSource.GetDefaultView(grid.ItemsSource) != null)
                                {
                                    GetAutoFilter(grid).FilterExpression = CorrectRowFilter(newFilter);
                                }
                                break;
                        }
                    }
                }

            }
            else
            {
                if (!string.IsNullOrEmpty(Convert.ToString(value)))
                {
                    if (grid.ItemsSource is DataView)
                    {
                        var actualValue = "";
                        var itemsSource = grid.ItemsSource as DataView;
                        var unquieValues = new HashSet<string>();
                        foreach (DataRow row in itemsSource.Table.Rows)
                        {
                            var val = Convert.ToString(row[columnName]);
                            if (val == Convert.ToString(value))
                                continue;
                            if (!unquieValues.Contains(val))
                                unquieValues.Add(val);
                            else
                                continue;
                            actualValue = actualValue == "" ? val + "'" : actualValue + "," + "'" + val + "'";
                        }
                        actualValue = actualValue.Substring(0, actualValue.Length - 1);
                        ApplyFilters(grid, columnName, actualValue);
                    }
                    else if (CollectionViewSource.GetDefaultView(grid.ItemsSource) != null)
                    {
                        ICollectionView view = CollectionViewSource.GetDefaultView(grid.ItemsSource);
                        var unquieValues = new HashSet<string>();
                        string actualValue = "";
                        foreach (var rowData in grid.ItemsSource)
                        {
                            var propertyValue = rowData.GetType().GetProperty(columnName);
                            if (propertyValue != null)
                            {
                                var data = Convert.ToString(propertyValue.GetValue(rowData, null));
                                if (data != null && !unquieValues.Contains(data) && data != Convert.ToString(value))
                                {
                                    actualValue = actualValue == "" ? data + "'" : actualValue + "," + "'" + data + "'";
                                    unquieValues.Add(data);
                                }
                            }
                        }
                        actualValue = actualValue.Substring(0, actualValue.Length - 1);
                        ApplyFilters(grid, columnName, actualValue);
                    }


                }
                else
                {
                    if (value == null)
                    {
                        if (grid.ItemsSource is DataView itemsSource)
                        {
                            if (newFilter.Contains("Or ([" + columnName + "] IS Null)"))
                            {
                                newFilter = newFilter.Replace("Or ([" + columnName + "] IS Null)", "");
                                itemsSource.RowFilter = CorrectRowFilter(newFilter);
                            }
                            else if (newFilter.Contains("([" + columnName + "] IS Null)"))
                            {
                                newFilter = newFilter.Replace("([" + columnName + "] IS Null)", "");
                                itemsSource.RowFilter = CorrectRowFilter(newFilter);
                            }
                            else if (newFilter.Contains("Or [" + columnName + "] IS Null"))
                            {
                                newFilter = newFilter.Replace("Or [" + columnName + "] IS Null", "");
                                itemsSource.RowFilter = CorrectRowFilter(newFilter);
                            }
                            else if (newFilter.Contains("[" + columnName + "] IS Null"))
                            {
                                newFilter = newFilter.Replace("[" + columnName + "] IS Null", "");
                                itemsSource.RowFilter = CorrectRowFilter(newFilter);
                            }
                        }
                        else if (CollectionViewSource.GetDefaultView(grid.ItemsSource) != null && grid is DataGrid mainGrid)
                        {
                            if (newFilter.Contains("Or ([" + columnName + "] IS Null)"))
                            {
                                newFilter = newFilter.Replace("Or ([" + columnName + "] IS Null)", "");
                                GetAutoFilter(mainGrid).FilterExpression = CorrectRowFilter(newFilter);
                            }
                            else if (newFilter.Contains("([" + columnName + "] IS Null)"))
                            {
                                newFilter = newFilter.Replace("([" + columnName + "] IS Null)", "");
                                GetAutoFilter(mainGrid).FilterExpression = CorrectRowFilter(newFilter);
                            }
                            else if (newFilter.Contains("Or [" + columnName + "] IS Null"))
                            {
                                newFilter = newFilter.Replace("Or [" + columnName + "] IS Null", "");
                                GetAutoFilter(mainGrid).FilterExpression = CorrectRowFilter(newFilter);
                            }
                            else if (newFilter.Contains("[" + columnName + "] IS Null"))
                            {
                                newFilter = newFilter.Replace("[" + columnName + "] IS Null", "");
                                GetAutoFilter(mainGrid).FilterExpression = CorrectRowFilter(newFilter);
                            }
                        }
                    }
                    else
                    {
                        if (grid.ItemsSource is DataView itemsSource)
                        {
                            if (newFilter.Contains("Or ([" + columnName + "] = '')"))
                            {
                                newFilter = newFilter.Replace("Or ([" + columnName + "] = '')", "");
                                itemsSource.RowFilter = CorrectRowFilter(newFilter);
                            }
                            else if (newFilter.Contains("([" + columnName + "] = '')"))
                            {
                                newFilter = newFilter.Replace("([" + columnName + "] = '')", "");
                                itemsSource.RowFilter = CorrectRowFilter(newFilter);
                            }
                            else if (newFilter.Contains("Or [" + columnName + "] = ''"))
                            {
                                newFilter = newFilter.Replace("Or [" + columnName + "] = ''", "");
                                itemsSource.RowFilter = CorrectRowFilter(newFilter);
                            }
                            else if (newFilter.Contains("[" + columnName + "] = ''"))
                            {
                                newFilter = newFilter.Replace("[" + columnName + "] = ''", "");
                                itemsSource.RowFilter = CorrectRowFilter(newFilter);
                            }
                        }
                        else if (CollectionViewSource.GetDefaultView(grid.ItemsSource) != null && grid is DataGrid mainGrid)
                        {
                            if (newFilter.Contains("Or ([" + columnName + "] = '')"))
                            {
                                newFilter = newFilter.Replace("Or ([" + columnName + "] = '')", "");
                                GetAutoFilter(mainGrid).FilterExpression = CorrectRowFilter(newFilter);
                            }
                            else if (newFilter.Contains("([" + columnName + "] = '')"))
                            {
                                newFilter = newFilter.Replace("([" + columnName + "] = '')", "");
                                GetAutoFilter(mainGrid).FilterExpression = CorrectRowFilter(newFilter);
                            }
                            else if (newFilter.Contains("Or [" + columnName + "] = ''"))
                            {
                                newFilter = newFilter.Replace("Or [" + columnName + "] = ''", "");
                                GetAutoFilter(mainGrid).FilterExpression = CorrectRowFilter(newFilter);
                            }
                            else if (newFilter.Contains("[" + columnName + "] = ''"))
                            {
                                newFilter = newFilter.Replace("[" + columnName + "] = ''", "");
                                GetAutoFilter(mainGrid).FilterExpression = CorrectRowFilter(newFilter);
                            }
                        }


                    }
                }
                grid.Items.Refresh();
            }
        }
        else
        {
            if (grid.ItemsSource is DataView)
            {
                var itemsSource = grid.ItemsSource as DataView;
                var actualValue = "";
                var unquieValues = new HashSet<string>();
                foreach (DataRow row in itemsSource.Table.Rows)
                {
                    var val = Convert.ToString(row[columnName]);
                    if (val == Convert.ToString(value))
                        continue;
                    if (!unquieValues.Contains(val))
                        unquieValues.Add(val);
                    else
                        continue;
                    actualValue = actualValue == "" ? val + "'" : actualValue + "," + "'" + val + "'";
                }
                actualValue = actualValue.Substring(0, actualValue.Length - 1);
                ApplyFilters(grid, columnName, actualValue);
                grid.Items.Refresh();
            }
            else if (CollectionViewSource.GetDefaultView(grid.ItemsSource) != null)
            {
                var view = CollectionViewSource.GetDefaultView(grid.ItemsSource);
                var unquieValues = new HashSet<string>();
                var actualValue = "";
                foreach (var rowData in grid.ItemsSource)
                {
                    var propertyValue = rowData.GetType().GetProperty(columnName);
                    if (propertyValue != null)
                    {
                        var data = Convert.ToString(propertyValue.GetValue(rowData, null));
                        if (data != null && !unquieValues.Contains(data) && data != Convert.ToString(value))
                        {
                            actualValue = actualValue == "" ? data + "'" : actualValue + "," + "'" + data + "'";
                            unquieValues.Add(data);
                        }
                    }
                }
                actualValue = actualValue.Substring(0, actualValue.Length - 1);
                ApplyFilters(grid, columnName, actualValue);
                grid.Items.Refresh();
            }

        }
        grid.Items.Refresh();

    }

    private static string RemoveIsNullAndBlankFilter(string existingFilter, string columnName)
    {
        var newFilter = existingFilter.Trim();

        //IS NULL Values

        if (newFilter.Contains("Or ([" + columnName + "] IS Null)"))
        {
            newFilter = newFilter.Replace("Or ([" + columnName + "] IS Null)", "");
            newFilter = CorrectRowFilter(newFilter);
        }
        else if (newFilter.Contains("([" + columnName + "] IS Null)"))
        {
            newFilter = newFilter.Replace("([" + columnName + "] IS Null)", "");
            newFilter = CorrectRowFilter(newFilter);
        }
        else if (newFilter.Contains("Or [" + columnName + "] IS Null"))
        {
            newFilter = newFilter.Replace("Or [" + columnName + "] IS Null", "");
            newFilter = CorrectRowFilter(newFilter);
        }
        else if (newFilter.Contains("[" + columnName + "] IS Null"))
        {
            newFilter = newFilter.Replace("[" + columnName + "] IS Null", "");
            newFilter = CorrectRowFilter(newFilter);
        }

        //BLANK values

        if (newFilter == null)
            return null;

        if (newFilter.Contains("Or ([" + columnName + "] = '')"))
        {
            newFilter = newFilter.Replace("Or ([" + columnName + "] = '')", "");

            newFilter = CorrectRowFilter(newFilter);
        }
        else if (newFilter.Contains("([" + columnName + "] = '')"))
        {
            newFilter = newFilter.Replace("([" + columnName + "] = '')", "");
            newFilter = CorrectRowFilter(newFilter);
        }
        else if (newFilter.Contains("Or [" + columnName + "] = ''"))
        {
            newFilter = newFilter.Replace("Or [" + columnName + "] = ''", "");
            newFilter = CorrectRowFilter(newFilter);
        }
        else if (newFilter.Contains("[" + columnName + "] = ''"))
        {
            newFilter = newFilter.Replace("[" + columnName + "] = ''", "");
            newFilter = CorrectRowFilter(newFilter);
        }
        else if (newFilter.IndexOf("() AND ", StringComparison.Ordinal) == 0)
        {
            newFilter = newFilter.Substring(7);
            newFilter = CorrectRowFilter(newFilter);
        }

        return newFilter;
    }

    private static string CorrectRowFilter(string exsistingFilter)
    {
        var newFilter = Convert.ToString(exsistingFilter);

        if (string.IsNullOrEmpty(newFilter))
            return null;

        newFilter = exsistingFilter.Trim();

        if (string.IsNullOrEmpty(newFilter))
            return null;

        if (newFilter.Contains(",)"))
        {
            newFilter = newFilter.Replace(",)", ")");
        }
        if (newFilter.Contains("()"))
        {
            newFilter = "";
        }
        if (newFilter.Contains("(,"))
        {
            newFilter = newFilter.Replace("(,", "(");
        }
        if (newFilter.Contains(",)"))
        {
            newFilter = newFilter.Replace(",)", ")");
        }
        if (newFilter.Contains(" AND ()"))
        {
            newFilter = newFilter.Replace(" AND ()", "");
        }
        newFilter = newFilter.Replace(exsistingFilter, newFilter);
        if (newFilter.Contains("() AND "))
        {
            newFilter = newFilter.Replace("() AND ", "");
        }
        if (newFilter.IndexOf("(((", StringComparison.Ordinal) == 0)
        {
            newFilter = newFilter.Substring(2);
        }
        if (newFilter.Contains(")))"))
        {
            newFilter = newFilter.Replace(")))", "))");
        }
        if (newFilter.Contains("()"))
        {
            newFilter = newFilter.Replace("()", "");
        }
        if (newFilter.LastIndexOf(" AND ", StringComparison.Ordinal) == newFilter.Length - 5)
        {
            newFilter = newFilter.Substring(0, newFilter.Length - 5);
        }
        if (newFilter.IndexOf("((", StringComparison.Ordinal) == 0)
        {
            newFilter = newFilter.Substring(1);
        }
        newFilter = newFilter.Trim();
        if (newFilter.IndexOf("Or", StringComparison.Ordinal) == 0)
        {
            newFilter = newFilter.Substring(3);
        }

        if (newFilter.LastIndexOf(" )", StringComparison.Ordinal) == newFilter.Length - 2 && newFilter.Length - 2 > 0)
        {
            newFilter = newFilter.Substring(0, newFilter.Length - 2) + newFilter.Substring(newFilter.Length - 1);
        }
        if (newFilter.LastIndexOf("AND", StringComparison.Ordinal) == newFilter.Length - 3)
        {
            newFilter = newFilter.Substring(0, newFilter.Length - 3);
        }

        if (newFilter == "( )")
            newFilter = null;

        return newFilter switch
        {
            "" => null,
            _ => newFilter,
        };
    }

    private bool FilterPredicate(object obj)
    {
        if (_collectionViewDt == null)
            return false;
        // populate the row
        var row = _collectionViewDt.Rows[0];
        foreach (var pi in obj.GetType().GetProperties())
        {
            // Bugfix: do not evaluate indexed propertiess
            if (pi.GetIndexParameters().Any() == false)
            {
                if (pi.PropertyType.BaseType != null && pi.PropertyType.BaseType.FullName == "System.Enum")
                {
                    var value = Convert.ToInt32(Enum.Parse(pi.PropertyType, Convert.ToString(pi.GetValue(obj, null))));
                    row[pi.Name] = value;
                }
                else
                {
                    try
                    {
                        row[pi.Name] = pi.GetValue(obj, null);
                    }
                    catch (Exception)
                    {
                        row[pi.Name] = DBNull.Value;
                    }
                }
            }
        }
        bool.TryParse(Convert.ToString(row["_filter"]), out bool returnValue);
        // compute the expression
        return returnValue;
    }

    private static string EscapeLikeValue(string value)
    {
        var sb = new StringBuilder(value.Length);
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            switch (c)
            {
                case ']':
                case '[':
                case '%':
                case '*':

                    sb.Append("[").Append(c).Append("]");
                    break;
                //case '\'':

                //    sb.Append("''");
                //    break;
                default:
                    sb.Append(c);
                    break;
            }
        }
        return sb.ToString();
    }

    private void UpdateFilter()
    {
        _collectionViewDt = null;
        var enumerator = _view?.GetEnumerator() ?? _currentGrid.ItemsSource.GetEnumerator();
        enumerator.MoveNext(); // sets it to the first element
        var firstElement = enumerator.Current;
        if (firstElement != null && !string.IsNullOrEmpty(FilterExpression))
        {
            // build/rebuild data table
            var dt = new DataTable();
            foreach (var pi in firstElement.GetType().GetProperties())
            {
                dt.Columns.Add(pi.Name, Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType);
            }

            // add calculated column
            dt.Columns.Add("_filter", typeof(bool), FilterExpression);

            // create a single row for evaluating expressions
            if (dt.Rows.Count == 0)
            {
                dt.Rows.Add(dt.NewRow());
            }

            // done, save table
            _collectionViewDt = dt;
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member