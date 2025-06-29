using System.Windows;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Demo.Examples.Windowing.SidePanelWindows;

/// <summary>
/// Interaction logic for SimpleSidePanelWindow.xaml
/// </summary>
public partial class SimpleSidePanelWindowView : UserControl
{
    private CalendarWindow? _notificationWindow;

    public SimpleSidePanelWindowView()
    {
        InitializeComponent();

        Loaded += SimpleSidePanelWindowView_Loaded;
    }

    private void SimpleSidePanelWindowView_Loaded(object sender, RoutedEventArgs e)
    {
        var w = Window.GetWindow(this);
        w.Closed += W_Closed;
        _notificationWindow = new CalendarWindow();
    }

    private void W_Closed(object? sender, EventArgs e)
    {
        _notificationWindow!.Close();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        _notificationWindow!.IsExpanded = !_notificationWindow.IsExpanded;
    }
}
