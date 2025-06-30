using System.Windows;
using System.Windows.Controls;
using DevToolbox.Wpf.Demo.ViewModels;

namespace DevToolbox.Wpf.Demo.Examples.Windowing.TabsWindows;

/// <summary>
/// Interaction logic for SimpleSidePanelWindow.xaml
/// </summary>
public partial class SimpleTabsWindow : UserControl
{
    public SimpleTabsWindow()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var tabsWindowExample = new TabsWindowExample();
        var vm = new TabsWindowExampleViewModel();
        tabsWindowExample.DataContext = vm;
        vm.Items.Add(new TabItemContent()
        {
            Header = "Header 1",
            Content = "Content 1"
        });

        tabsWindowExample.Show();
    }
}
