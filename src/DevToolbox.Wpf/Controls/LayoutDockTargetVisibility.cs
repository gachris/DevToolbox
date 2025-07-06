using System;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Specifies possible docking positions for a <see cref="LayoutDockItemsControl"/>
/// relative to its container or an existing pane.
/// </summary>
[Flags]
public enum LayoutDockTargetVisibility
{
    /// <summary>
    /// No docking target.
    /// </summary>
    None = 0,

    /// <summary>
    /// Dock to the top edge of the container.
    /// </summary>
    Top = 1 << 0,

    /// <summary>
    /// Dock to the bottom edge of the container.
    /// </summary>
    Bottom = 1 << 1,

    /// <summary>
    /// Dock to the left edge of the container.
    /// </summary>
    Left = 1 << 2,

    /// <summary>
    /// Dock to the right edge of the container.
    /// </summary>
    Right = 1 << 3,

    /// <summary>
    /// Dock inside the container at its top edge.
    /// </summary>
    InnerTop = 1 << 4,

    /// <summary>
    /// Dock inside the container at its bottom edge.
    /// </summary>
    InnerBottom = 1 << 5,

    /// <summary>
    /// Dock inside the container at its left edge.
    /// </summary>
    InnerLeft = 1 << 6,

    /// <summary>
    /// Dock inside the container at its right edge.
    /// </summary>
    InnerRight = 1 << 7,

    /// <summary>
    /// Dock to the top edge of an existing pane.
    /// </summary>
    PaneTop = 1 << 8,

    /// <summary>
    /// Dock to the bottom edge of an existing pane.
    /// </summary>
    PaneBottom = 1 << 9,

    /// <summary>
    /// Dock to the left edge of an existing pane.
    /// </summary>
    PaneLeft = 1 << 10,

    /// <summary>
    /// Dock to the right edge of an existing pane.
    /// </summary>
    PaneRight = 1 << 11,

    /// <summary>
    /// Insert as a new tab inside an existing pane.
    /// </summary>
    PaneInto = 1 << 12,

    /// <summary>
    /// All possible docking targets (container edges, inner edges, and pane edges/tabs).
    /// </summary>
    All = Top
        | Bottom
        | Left
        | Right
        | InnerTop
        | InnerBottom
        | InnerLeft
        | InnerRight
        | PaneTop
        | PaneBottom
        | PaneLeft
        | PaneRight
        | PaneInto,
}