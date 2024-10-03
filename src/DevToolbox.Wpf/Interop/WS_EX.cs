using System;

namespace DevToolbox.Wpf.Interop;

[Flags]
internal enum WS_EX
{
    /// <summary>
    /// Specified WS_EX_DLGMODALFRAME enumeration value.
    /// </summary>
    DLGMODALFRAME = 0x00000001,

    /// <summary>
    /// Specified WS_EX_NOPARENTNOTIFY enumeration value.
    /// </summary>
    NOPARENTNOTIFY = 0x00000004,

    /// <summary>
    /// Specified WS_EX_TOPMOST enumeration value.
    /// </summary>
    TOPMOST = 0x00000008,

    /// <summary>
    /// Specified WS_EX_ACCEPTFILES enumeration value.
    /// </summary>
    ACCEPTFILES = 0x00000010,

    /// <summary>
    /// Specified WS_EX_TRANSPARENT enumeration value.
    /// </summary>
    TRANSPARENT = 0x00000020,

    /// <summary>
    /// Specified WS_EX_MDICHILD enumeration value.
    /// </summary>
    MDICHILD = 0x00000040,

    /// <summary>
    /// Specified WS_EX_TOOLWINDOW enumeration value.
    /// </summary>
    TOOLWINDOW = 0x00000080,

    /// <summary>
    /// Specified WS_EX_WINDOWEDGE enumeration value.
    /// </summary>
    WINDOWEDGE = 0x00000100,

    /// <summary>
    /// Specified WS_EX_CLIENTEDGE enumeration value.
    /// </summary>
    CLIENTEDGE = 0x00000200,

    /// <summary>
    /// Specified WS_EX_CONTEXTHELP enumeration value.
    /// </summary>
    CONTEXTHELP = 0x00000400,

    /// <summary>
    /// Specified WS_EX_RIGHT enumeration value.
    /// </summary>
    RIGHT = 0x00001000,

    /// <summary>
    /// Specified WS_EX_LEFT enumeration value.
    /// </summary>
    LEFT = 0x00000000,

    /// <summary>
    /// Specified WS_EX_RTLREADING enumeration value.
    /// </summary>
    RTLREADING = 0x00002000,

    /// <summary>
    /// Specified WS_EX_LTRREADING enumeration value.
    /// </summary>
    LTRREADING = 0x00000000,

    /// <summary>
    /// Specified WS_EX_LEFTSCROLLBAR enumeration value.
    /// </summary>
    LEFTSCROLLBAR = 0x00004000,

    /// <summary>
    /// Specified WS_EX_RIGHTSCROLLBAR enumeration value.
    /// </summary>
    RIGHTSCROLLBAR = 0x00000000,

    /// <summary>
    /// Specified WS_EX_CONTROLPARENT enumeration value.
    /// </summary>
    CONTROLPARENT = 0x00010000,

    /// <summary>
    /// Specified WS_EX_STATICEDGE enumeration value.
    /// </summary>
    STATICEDGE = 0x00020000,

    /// <summary>
    /// Specified WS_EX_APPWINDOW enumeration value.
    /// </summary>
    APPWINDOW = 0x00040000,

    /// <summary>
    /// Specified WS_EX_OVERLAPPEDWINDOW enumeration value.
    /// </summary>
    OVERLAPPEDWINDOW = 0x00000300,

    /// <summary>
    /// Specified WS_EX_PALETTEWINDOW enumeration value.
    /// </summary>
    PALETTEWINDOW = 0x00000188,

    /// <summary>
    /// Specified WS_EX_LAYERED enumeration value.
    /// </summary>
    LAYERED = 0x00080000,

    /// <summary>
    /// Specified WS_EX_NOACTIVATE enumeration value.
    /// </summary>
    NOACTIVATE = 0x08000000,
}