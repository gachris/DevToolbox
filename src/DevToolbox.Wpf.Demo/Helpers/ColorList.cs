using System.Windows.Media;

namespace DevToolbox.Wpf.Demo.Helpers;

public class ColorList
{
    private static IEnumerable<KeyValuePair<string, SolidColorBrush>>? _colors;

    public static IEnumerable<KeyValuePair<string, SolidColorBrush>> Colors => _colors ??= GetColors();

    private static IEnumerable<KeyValuePair<string, SolidColorBrush>> GetColors() => typeof(Brushes)
            .GetProperties()
            .Where(prop => typeof(SolidColorBrush).IsAssignableFrom(prop.PropertyType))
            .Select(prop => new KeyValuePair<string, SolidColorBrush>(prop.Name, (SolidColorBrush)prop.GetValue(null)!));
}
