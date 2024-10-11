using System.Windows.Media;

namespace DevToolbox.Wpf.Extensions;

internal static class ColorExtensions
{
    public static Color ToMediaColor(this System.Drawing.Color color) => Color.FromArgb(color.A, color.R, color.G, color.B);
}