using System.Windows;
using System.Windows.Media;
using DevToolbox.Wpf.Controls;

namespace DevToolbox.Wpf.Windows;

public partial class ColorPickerDialog : WindowEx
{
    internal ColorPickerDialog() => InitializeComponent();

    private void Accept(object sender, RoutedEventArgs e) => DialogResult = true;

    private void Cancel(object sender, RoutedEventArgs e) => DialogResult = false;

    private void EyeDropper_CaptureChanged(object sender, CaptureEventArgs e)
    {
        if (e.CaptureState == CaptureState.Finished)
            colorPicker.SelectedColor = ((SolidColorBrush)eyeDropper.Color).Color;
    }
}
