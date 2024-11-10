using System.Windows;

namespace DevToolbox.Wpf.Demo.Data;

public class DragObject
{
    public string Xaml { get; set; } = null!;

    public Size? DesiredSize { get; set; }
}