using CommonServiceLocator;
using DevToolbox.Wpf.Demo.ViewModels;
using DevToolbox.Wpf.Windows;

namespace DevToolbox.Wpf.Demo;

public partial class MainWindow : WindowEx
{
    private readonly MainViewModel _mainViewModel;

    public MainWindow()
    {
        _mainViewModel = ServiceLocator.Current.GetInstance<MainViewModel>();

        DataContext = _mainViewModel;
        ContentRendered += MainWindow_ContentRendered;

        InitializeComponent();

        Chrome.CaptionHeight = 44;
    }

    private async void MainWindow_ContentRendered(object? sender, EventArgs e) => await _mainViewModel.Initialize();
}
