using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevToolbox.Wpf.Helpers;

public class DataGridHelper
{
    public static readonly DependencyProperty ShowRowNumberProperty =
        DependencyProperty.RegisterAttached("ShowRowNumber", typeof(bool), typeof(DataGridHelper), new FrameworkPropertyMetadata(false, OnShowRowNumberChanged));

    public static readonly DependencyProperty AllowUserToCopyProperty =
        DependencyProperty.RegisterAttached("AllowUserToCopy", typeof(bool), typeof(DataGridHelper), new FrameworkPropertyMetadata(true, OnAllowUserToCopyChanged));

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
