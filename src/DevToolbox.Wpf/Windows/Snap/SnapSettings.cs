using System;
using DevToolbox.Wpf.Interop;

namespace DevToolbox.Wpf.Windows.Snap;

/// <summary>
/// Provides access to system-level settings related to window snapping behavior in WPF applications.
/// This static class allows for getting and setting various parameters that affect how windows are arranged, 
/// sized, and docked within the desktop environment.
/// </summary>
public static class SnapSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether the window arranging feature is enabled.
    /// </summary>
    public static bool WindowArranging
    {
        get => GetValue(SPI.GETWINARRANGING);
        set => SetValue(SPI.SETWINARRANGING, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether snap sizing is enabled. 
    /// This is only effective if window arranging is enabled.
    /// </summary>
    public static bool SnapSizing
    {
        get => WindowArranging && GetValue(SPI.GETSNAPSIZING);
        set => SetValue(SPI.SETSNAPSIZING, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether dock moving is enabled. 
    /// This is only effective if window arranging is enabled.
    /// </summary>
    public static bool DockMoving
    {
        get => WindowArranging && GetValue(SPI.GETDOCKMOVING);
        set => SetValue(SPI.SETDOCKMOVING, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether dragging from a maximized window is enabled. 
    /// This is only effective if window arranging is enabled.
    /// </summary>
    public static bool DragFromMaximize
    {
        get => WindowArranging && GetValue(SPI.GETDRAGFROMMAXIMIZE);
        set => SetValue(SPI.SETDRAGFROMMAXIMIZE, value);
    }

    /// <summary>
    /// Retrieves the current value of a specified system parameter.
    /// </summary>
    /// <param name="parameter">The parameter to retrieve.</param>
    /// <returns>A boolean value indicating the current setting for the specified parameter.</returns>
    /// <exception cref="Exception">Thrown when the SystemParametersInfo call fails.</exception>
    private static bool GetValue(SPI parameter)
    {
        var value = IntPtr.Zero;
        var success = User32.SystemParametersInfo(parameter, 0, ref value, SPIF.NONE);

        return !success ? throw new Exception("SystemParametersInfo call failed.") : value != IntPtr.Zero;
    }

    /// <summary>
    /// Sets the value of a specified system parameter.
    /// </summary>
    /// <param name="parameter">The parameter to set.</param>
    /// <param name="value">The new value for the parameter.</param>
    /// <exception cref="Exception">Thrown when the SystemParametersInfo call fails.</exception>
    private static void SetValue(SPI parameter, bool value)
    {
        var nullPtr = IntPtr.Zero;
        var success = User32.SystemParametersInfo(parameter, (uint)(value ? 1 : 0), ref nullPtr, SPIF.NONE);

        if (!success) throw new Exception("SystemParametersInfo call failed.");
    }
}
