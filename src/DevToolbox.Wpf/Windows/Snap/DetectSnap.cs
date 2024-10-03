using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;

namespace DevToolbox.Wpf.Windows.Snap;

/// <summary>
/// Provides methods for detecting if a window is snapped to the edges of the screen.
/// </summary>
internal static class DetectSnap
{
    /// <summary>
    /// Determines if the specified window location is snapped to any of the provided monitors.
    /// </summary>
    /// <param name="location">The rectangle representing the window's location.</param>
    /// <param name="monitors">A list of monitors that intersect with the window area.</param>
    /// <param name="offset">The offset values for window edges.</param>
    /// <returns>A <see cref="SnapResult"/> indicating whether the window is snapped and its snap bounds.</returns>
    internal static SnapResult IsSnapped(ref Rect location, List<Screen> monitors, Size offset)
    {
        var snapped = false;
        var monitor = monitors[0];
        var bounds = SnapBounds.None;

        for (var i = 0; i < monitors.Count; i++)
        {
            snapped = IsSnapped(ref location, monitors[i], offset, out bounds);

            if (snapped)
            {
                monitor = monitors[i];
                break;
            }
        }

        return new SnapResult(snapped, monitor, bounds);
    }

    /// <summary>
    /// Checks if the window is snapped to a specific monitor, adjusting for window height if necessary.
    /// </summary>
    /// <param name="location">The rectangle representing the window's location.</param>
    /// <param name="monitor">The monitor to check for snapping.</param>
    /// <param name="offset">The offset values for window edges.</param>
    /// <param name="bounds">The snap bounds if the window is snapped.</param>
    /// <returns>True if the window is snapped, otherwise false.</returns>
    private static bool IsSnapped(ref Rect location, Screen monitor, Size offset, out SnapBounds bounds)
    {
        var snapped = IsSnapped(ref location, monitor, out bounds);

        // If not detected, try again with an adjusted window height.
        // This is required to detect a top corner snap on a border 
        // styled window (Windows 10+)
        if (!snapped)
        {
            location.Height -= offset.Height;
            snapped = IsSnapped(ref location, monitor, out bounds);
        }

        return snapped;
    }

    /// <summary>
    /// Determines if the window is snapped based on its location and the monitor it is on.
    /// </summary>
    /// <param name="location">The rectangle representing the window's location.</param>
    /// <param name="monitor">The monitor to check for snapping.</param>
    /// <param name="bounds">The snap bounds if the window is snapped.</param>
    /// <returns>True if the window is snapped, otherwise false.</returns>
    private static bool IsSnapped(ref Rect location, Screen monitor, out SnapBounds bounds)
    {
        bounds = GetBounds(ref location, monitor);

        var windowHeight = location.Height;
        var workHeight = monitor.WorkingArea.Height;
        var displayHeight = monitor.Bounds.Height;

        var loHalfWorkHeight = Math.Floor(workHeight / 2.0d);
        var loHalfDisplayHeight = Math.Floor(displayHeight / 2.0d);
        var hiHalfWorkHeight = Math.Ceiling(workHeight / 2.0d);
        var hiHalfDisplayHeight = Math.Ceiling(displayHeight / 2.0d);

        return (windowHeight == workHeight ||
            windowHeight == displayHeight ||
            windowHeight == loHalfWorkHeight ||
            windowHeight == loHalfDisplayHeight ||
            windowHeight == hiHalfWorkHeight ||
            windowHeight == hiHalfDisplayHeight) && bounds != SnapBounds.None;
    }

    /// <summary>
    /// Gets the snap bounds based on the window's location and the monitor it is on.
    /// </summary>
    /// <param name="location">The rectangle representing the window's location.</param>
    /// <param name="monitor">The monitor to check for snapping.</param>
    /// <returns>The snap bounds as a <see cref="SnapBounds"/> enumeration.</returns>
    private static SnapBounds GetBounds(ref Rect location, Screen monitor)
    {
        var bounds = SnapBounds.None;

        var work = monitor.WorkingArea;
        var display = monitor.Bounds;

        var left = location.Left;
        var top = location.Top;
        var right = location.Right;
        var bottom = location.Bottom;

        // TOP
        if (top == work.Top || top == display.Top)
        {
            bounds |= SnapBounds.Top;
        }

        // LEFT
        if (left == work.Left || left == display.Left)
        {
            bounds |= SnapBounds.Left;
        }

        // RIGHT
        if (right == work.Right || right == display.Right)
        {
            bounds |= SnapBounds.Right;
        }

        // BOTTOM
        if (bottom == work.Bottom || bottom == display.Bottom)
        {
            bounds |= SnapBounds.Bottom;
        }

        return bounds;
    }
}
