using System.Windows.Controls;
using DevToolbox.Wpf.Demo.ViewModels;

namespace DevToolbox.Wpf.Demo.Examples.Layout.DockManager;

/// <summary>
/// Interaction logic for SimpleDockManager.xaml
/// </summary>
public partial class SimpleDockManager : UserControl
{
    public SimpleDockManager()
    {
        InitializeComponent();
        DataContext = new DockManagerViewModel();
    }
}
