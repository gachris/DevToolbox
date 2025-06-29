using System.Windows;
using System.Windows.Controls;
using DevToolbox.Wpf.Demo.ViewModels;

namespace DevToolbox.Wpf.Demo.Examples.Collections.DataGrid;

/// <summary>
/// Interaction logic for SimpleDataGrid.xaml
/// </summary>
public partial class SimpleDataGrid : UserControl
{
    public SimpleDataGrid()
    {
        InitializeComponent();

        DataContext = new ExampleDataGridViewModel();
    }

    #region Events Subscriptions

    private void Button_Checked(object sender, RoutedEventArgs e)
    {
        gridControl.SetBinding(ItemsControl.ItemsSourceProperty, nameof(ExampleDataGridViewModel.SourceTable));
    }

    private void Button_Unchecked(object sender, RoutedEventArgs e)
    {
        gridControl.SetBinding(ItemsControl.ItemsSourceProperty, nameof(ExampleDataGridViewModel.SourceCollection));
    }

    #endregion
}
