using System.Windows;
using System.Windows.Controls;
using DevToolbox.Wpf.Windows;
using DevToolbox.Wpf.Windows.Effects;

namespace DevToolbox.Wpf.Demo.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class SystemBackdrop : UserControl
{
    public SystemBackdrop()
    {
        InitializeComponent();
    }

    private void RadioButton_Checked(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);

        var flag = int.Parse((string)((RadioButton)sender).Tag);
        var windowEffect = default(Effect);

        switch (flag)
        {
            case 0:
                windowEffect = null;
                break;
            case 1:
                windowEffect = new Acrylic();
                break;
            case 2:
                windowEffect = new Mica();
                break;
            case 3:
                windowEffect = new Tabbed();
                break;
            default:
                break;
        }

        WindowBehaviour.SetWindowEffect(window, windowEffect);
    }
}
