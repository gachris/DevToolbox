using System;

namespace DevToolbox.Wpf.Interop;

/// <summary>
/// SystemParameterInfo flag values, SPIF_*
/// </summary>
[Flags]
internal enum SPIF
{
    NONE = 0,
    UPDATEINIFILE = 0x01,
    SENDCHANGE = 0x02,
    SENDWININICHANGE = SENDCHANGE,
}
