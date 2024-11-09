using System.Windows;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Helpers;

public class DataGridHelper
{
    public static readonly DependencyProperty ShowRowNumberProperty =
        DependencyProperty.RegisterAttached("ShowRowNumber", typeof(bool), typeof(DataGridHelper), new FrameworkPropertyMetadata(false, OnShowRowNumberChanged));

    public static bool GetShowRowNumber(DependencyObject obj)
    {
        return (bool)obj.GetValue(ShowRowNumberProperty);
    }

    public static void SetShowRowNumber(DependencyObject obj, bool value)
    {
        obj.SetValue(ShowRowNumberProperty, value);
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

    private static void RefreshRowNumbers(object? sender, DataGridRowEventArgs args)
    {
        if (sender is not DataGrid dataGrid)
        {
            return;
        }

        UpdateRowNumbers(dataGrid, true);
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
}
