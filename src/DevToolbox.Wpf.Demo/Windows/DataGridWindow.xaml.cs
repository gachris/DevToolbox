using System.Windows;
using System.Windows.Controls;
using DevToolbox.Wpf.Demo.ViewModels;
using DevToolbox.Wpf.Windows;

namespace DevToolbox.Wpf.Demo.Windows;

public partial class DataGridWindow : WindowEx
{
    public DataGridWindow()
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