using System;

namespace DevToolbox.Wpf.Interop;

[Flags]
internal enum SW : short
{
    /// <summary>
    /// Specified SW_HIDE enumeration value.
    /// </summary>
    HIDE = 0,

    /// <summary>
    /// Specified SW_SHOWNORMAL enumeration value.
    /// </summary>
    SHOWNORMAL = 1,

    /// <summary>
    /// Specified SW_NORMAL enumeration value.
    /// </summary>
    NORMAL = 1,

    /// <summary>
    /// Specified SW_SHOWMINIMIZED enumeration value.
    /// </summary>
    SHOWMINIMIZED = 2,

    /// <summary>
    /// Specified SW_SHOWMAXIMIZED enumeration value.
    /// </summary>
    SHOWMAXIMIZED = 3,

    /// <summary>
    /// Specified SW_MAXIMIZE enumeration value.
    /// </summary>
    MAXIMIZE = 3,

    /// <summary>
    /// Specified SW_SHOWNOACTIVATE enumeration value.
    /// </summary>
    SHOWNOACTIVATE = 4,

    /// <summary>
    /// Specified SW_SHOW enumeration value.
    /// </summary>
    SHOW = 5,

    /// <summary>
    /// Specified SW_MINIMIZE enumeration value.
    /// </summary>
    MINIMIZE = 6,

    /// <summary>
    /// Specified SW_SHOWMINNOACTIVE enumeration value.
    /// </summary>
    SHOWMINNOACTIVE = 7,

    /// <summary>
    /// Specified SW_SHOWNA enumeration value.
    /// </summary>
    SHOWNA = 8,

    /// <summary>
    /// Specified SW_RESTORE enumeration value.
    /// </summary>
    RESTORE = 9,

    /// <summary>
    /// Specified SW_SHOWDEFAULT enumeration value.
    /// </summary>
    SHOWDEFAULT = 10,

    /// <summary>
    /// Specified SW_FORCEMINIMIZE enumeration value.
    /// </summary>
    FORCEMINIMIZE = 11,

    /// <summary>
    /// Specified SW_MAX enumeration value.
    /// </summary>
    MAX = 11
}
