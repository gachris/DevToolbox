using System.Windows.Media;
using DevToolbox.Wpf.Interop;

namespace DevToolbox.Wpf.Helpers;

/// <summary>
/// Contains helper methods for retrieving system brushes based on the active user theme.
/// </summary>
public class SystemBrushesHelper
{
    /// <summary>
    /// Retrieves the accent brush for the active user theme.
    /// </summary>
    /// <returns>
    /// A <see cref="SolidColorBrush"/> representing the current accent color, or <c>null</c> if the accent brush cannot be retrieved.
    /// </returns>
    public static SolidColorBrush? GetAccentBrush()
    {
        try
        {
            return ActiveUserThemeReader.GetAccentBrushForActiveUser();
        }
        catch
        {
            // Suppress any exceptions thrown while retrieving the accent brush
        }

        return null;
    }
}
