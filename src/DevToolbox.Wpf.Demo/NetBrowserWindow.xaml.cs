using DevToolbox.Wpf.Demo.ViewModels;
using DevToolbox.Wpf.Windows;

namespace DevToolbox.Wpf.Demo;

public partial class NetBrowserWindow : TabsWindow
{
    public NetBrowserWindow()
    {
        DataContext = new NetBrowserViewModel();
        InitializeComponent();
    }

    protected override TabsWindow GetContainerForOverride()
    {
        return new NetBrowserWindow();
    }
}
