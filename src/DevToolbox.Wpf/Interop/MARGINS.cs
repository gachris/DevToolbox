using System.Runtime.InteropServices;

namespace DevToolbox.Wpf.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct MARGINS
{
    public int cxLeftWidth;      // width of left border that retains its size
    public int cxRightWidth;     // width of right border that retains its size
    public int cyTopHeight;      // height of top border that retains its size
    public int cyBottomHeight;   // height of bottom border that retains its size
}