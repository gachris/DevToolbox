using System.Runtime.InteropServices;

namespace DevToolbox.Wpf.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct SIZE
{
    /// <summary>
    /// cx field of structure
    /// </summary>
    public int cx;

    /// <summary>
    /// cy field of structure
    /// </summary>
    public int cy;

    public SIZE(int cx, int cy)
    {
        this.cx = cx;
        this.cy = cy;
    }

    public override bool Equals(object? obj) => base.Equals(obj);

    public override int GetHashCode() => base.GetHashCode();

    public static bool operator ==(SIZE left, SIZE right) => left.Equals(right);

    public static bool operator !=(SIZE left, SIZE right) => !(left == right);
}
