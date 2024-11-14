using DevToolbox.Wpf.Demo.ViewModels;
using DevToolbox.Wpf.Windows;

namespace DevToolbox.Wpf.Demo.Windows;

public partial class DockManagerWindow : WindowEx
{
    public DockManagerWindow()
    {
        InitializeComponent();
        DataContext = new DockManagerViewModel();
    }
}