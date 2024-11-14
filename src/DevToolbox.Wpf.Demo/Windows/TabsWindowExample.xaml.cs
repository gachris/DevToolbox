using DevToolbox.Wpf.Demo.ViewModels;
using DevToolbox.Wpf.Windows;

namespace DevToolbox.Wpf.Demo.Windows;

public partial class TabsWindowExample : TabsWindow
{
    public TabsWindowExample()
    {
        InitializeComponent();
        DataContext = new TabsWindowExampleViewModel();
    }
}