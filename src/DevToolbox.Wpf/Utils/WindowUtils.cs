using System.Reflection;
using System.Windows;

namespace DevToolbox.Wpf.Utils;

/// <summary>
/// A utility class providing extension methods for working with <see cref="Window"/> instances.
/// </summary>
public static class WindowUtils
{
    /// <summary>
    /// Gets the DPI (Dots Per Inch) scaling for the specified <see cref="Window"/>.
    /// This method accesses the non-public properties of the <see cref="SystemParameters"/> class
    /// to retrieve the DPI values.
    /// </summary>
    /// <param name="window">The <see cref="Window"/> instance to get the DPI for.</param>
    /// <returns>A <see cref="DpiScale"/> object containing the X and Y DPI values.</returns>
    public static DpiScale GetDpi(this Window window)
    {
        var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
        var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);

        _ = int.TryParse(dpiXProperty?.GetValue(window, null)?.ToString(), out var dpiX);
        _ = int.TryParse(dpiYProperty?.GetValue(window, null)?.ToString(), out var dpiY);

        return new DpiScale(dpiX, dpiY);
    }

    /// <summary>
    /// Determines whether the specified <see cref="Window"/> is being shown as a dialog.
    /// This method accesses the non-public field of the <see cref="Window"/> class
    /// to check the dialog state.
    /// </summary>
    /// <param name="window">The <see cref="Window"/> instance to check.</param>
    /// <returns><c>true</c> if the window is shown as a dialog; otherwise, <c>false</c>;
    /// returns <c>null</c> if the value cannot be determined.</returns>
    public static bool? ShowingAsDialog(this Window window)
        => (bool?)typeof(Window).GetField("_showingAsDialog", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(window);
}