using System.Windows;
using System.Windows.Controls;

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
        new TabsWindowExample().Show();
    }
}
