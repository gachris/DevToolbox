using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevToolbox.Wpf.Controls;

namespace DevToolbox.Wpf.Helpers;

public class DataGridHelper
{
    private delegate string[] ParseFormat(string value);

    public static readonly DependencyProperty ShowRowNumberProperty =
        DependencyProperty.RegisterAttached("ShowRowNumber", typeof(bool), typeof(DataGridHelper), new FrameworkPropertyMetadata(false, OnShowRowNumberChanged));

    public static readonly DependencyProperty AllowUserToCopyProperty =
        DependencyProperty.RegisterAttached("AllowUserToCopy", typeof(bool), typeof(DataGridHelper), new FrameworkPropertyMetadata(true, OnAllowUserToCopyChanged));

    private static readonly RoutedUICommand _clearRowFilterCommand = new RoutedUICommand(nameof(ClearRowFilterCommand), nameof(ClearRowFilterCommand), typeof(DataGrid));

    public static RoutedUICommand ClearRowFilterCommand => _clearRowFilterCommand;

    public static RoutedUICommand PasteCommand => ApplicationCommands.Paste;

    static DataGridHelper()
    {
        CommandManager.RegisterClassCommandBinding(typeof(DataGrid), new CommandBinding(PasteCommand, OnPasteExecuted, OnPasteCanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(DataGrid), new CommandBinding(ClearRowFilterCommand, OnClearRowFilterExecuted, OnClearRowFilterCanExecute));
    }

    private static void OnPasteExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        List<string[]>? clipboardData = null;
        object? clipboardRawData = null;
        ParseFormat? parseFormat = null;

        var dataObj = Clipboard.GetDataObject();
        if (dataObj != null && (clipboardRawData = dataObj.GetData(DataFormats.CommaSeparatedValue)) != null)
        {
            parseFormat = ParseCsvFormat;
        }
        else if (dataObj != null && (clipboardRawData = dataObj.GetData(DataFormats.Text)) != null)
        {
            parseFormat = ParseTextFormat;
        }

        if (parseFormat != null)
        {
            var rawDataStr = clipboardRawData as string;

            if (rawDataStr == null && clipboardRawData is MemoryStream ms)
            {
                var sr = new StreamReader(ms);
                rawDataStr = sr.ReadToEnd();
            }

            var rows = rawDataStr?.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries) ?? [];
            if (rows.Length > 0)
            {
                clipboardData = rows.Select(row => parseFormat(row)).ToList();
            }
        }

        if (clipboardData is null)
        {
            return;
        }

        var dataGrid = (DataGrid)sender;
        var minRowIndex = dataGrid.Items.IndexOf(dataGrid.CurrentItem);
        var maxRowIndex = dataGrid.Items.Count - 1;
        var minColumnDisplayIndex = (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow) ? dataGrid.Columns.IndexOf(dataGrid.CurrentColumn) : 0;
        var maxColumnDisplayIndex = dataGrid.Columns.Count - 1;
        var rowDataIndex = 0;
        for (var i = minRowIndex; i <= maxRowIndex && rowDataIndex < clipboardData.Count; i++, rowDataIndex++)
        {
            var columnDataIndex = 0;
            for (var j = minColumnDisplayIndex;
                 j <= maxColumnDisplayIndex && columnDataIndex < clipboardData[rowDataIndex].Length;
                 j++, columnDataIndex++)
            {
                DataGridColumn column = dataGrid.ColumnFromDisplayIndex(j);
                column.OnPastingCellClipboardContent(dataGrid.Items[i], clipboardData[rowDataIndex][columnDataIndex]);
            }
        }
    }

    private static string[] ParseCsvFormat(string value)
    {
        return ParseCsvOrTextFormat(value, true);
    }

    private static string[] ParseTextFormat(string value)
    {
        return ParseCsvOrTextFormat(value, false);
    }

    private static string[] ParseCsvOrTextFormat(string value, bool isCsv)
    {
        var outputList = new List<string>();
        var separator = isCsv ? ',' : '\t';
        var startIndex = 0;
        var endIndex = 0;

        for (var i = 0; i < value.Length; i++)
        {
            var ch = value[i];

            if (ch == separator)
            {
                outputList.Add(value.Substring(startIndex, endIndex - startIndex));

                startIndex = endIndex + 1;
                endIndex = startIndex;
            }
            else if (ch == '\"' && isCsv)
            {
                i++;
                if (i >= value.Length)
                {
                    throw new FormatException(string.Format("value: {0} had a format exception", value));
                }

                var tempCh = value[i];
                while (tempCh != '\"' && i < value.Length)
                {
                    i++;
                }

                endIndex = i;
            }
            else if (i + 1 == value.Length)
            {
                outputList.Add(value.Substring(startIndex));
                break;
            }
            else
            {
                endIndex++;
            }
        }

        return [.. outputList];
    }

    private static void OnPasteCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is DataGrid;
    }

    private static void OnClearRowFilterExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var dataGrid = (DataGrid)sender;

        foreach (var dataGridColumnHeader in dataGrid.Columns)
        {
            var autoFilter = AutoFilter.GetAutoFilter(dataGrid);
            autoFilter.RemoveAllFilter(dataGridColumnHeader.SortMemberPath);
        }
    }

    private static void OnClearRowFilterCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is DataGrid;
    }

    public static bool GetShowRowNumber(DependencyObject obj)
    {
        return (bool)obj.GetValue(ShowRowNumberProperty);
    }

    public static void SetShowRowNumber(DependencyObject obj, bool value)
    {
        obj.SetValue(ShowRowNumberProperty, value);
    }

    public static bool GetAllowUserToCopy(DependencyObject obj)
    {
        return (bool)obj.GetValue(AllowUserToCopyProperty);
    }

    public static void SetAllowUserToCopy(DependencyObject obj, bool value)
    {
        obj.SetValue(AllowUserToCopyProperty, value);
    }

    private static void OnAllowUserToCopyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var dataGrid = (DataGrid)d;
        var allowUserToCopy = (bool)e.NewValue;

        if (!allowUserToCopy)
        {
            dataGrid.PreviewKeyDown += DataGrid_PreviewKeyDown;
        }
        else
        {
            dataGrid.PreviewKeyDown -= DataGrid_PreviewKeyDown;
        }
    }

    private static void OnShowRowNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var dataGrid = (DataGrid)d;

        var showRowNumber = (bool)e.NewValue;

        if (showRowNumber)
        {
            dataGrid.LoadingRow += RefreshRowNumbers;
            dataGrid.UnloadingRow += RefreshRowNumbers;
        }
        else
        {
            dataGrid.LoadingRow -= RefreshRowNumbers;
            dataGrid.UnloadingRow -= RefreshRowNumbers;
        }

        UpdateRowNumbers(dataGrid, showRowNumber);
    }

    private static void UpdateRowNumbers(DataGrid dataGrid, bool show)
    {
        foreach (var item in dataGrid.Items)
        {
            if (dataGrid.ItemContainerGenerator.ContainerFromItem(item) is DataGridRow row && row.GetIndex() != -1)
            {
                row.Header = show ? (row.GetIndex() + 1).ToString() : null;
            }
        }
    }

    private static void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.C && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
        {
            e.Handled = true;
        }
    }

    private static void RefreshRowNumbers(object? sender, DataGridRowEventArgs args)
    {
        if (sender is not DataGrid dataGrid)
        {
            return;
        }

        UpdateRowNumbers(dataGrid, true);
    }
}
