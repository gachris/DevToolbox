using DevToolbox.Wpf.Demo.ViewModels;
using DevToolbox.Wpf.Windows;

namespace DevToolbox.Wpf.Demo.Examples.Windowing.TabsWindows;

public partial class TabsWindowExample : TabsWindow
{
    public TabsWindowExample()
    {
        InitializeComponent();
        DataContext = new TabsWindowExampleViewModel();
    }
}