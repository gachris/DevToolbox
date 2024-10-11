namespace DevToolbox.Wpf.Extensions;

internal static class DoubleExtensions
{
    public static double ToRealNumber(this double value, double defaultValue = 0) => (double.IsInfinity(value) || double.IsNaN(value)) ? defaultValue : value;
}
