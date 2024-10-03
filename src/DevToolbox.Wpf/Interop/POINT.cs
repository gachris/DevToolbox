using System.Runtime.InteropServices;

namespace DevToolbox.Wpf.Interop;

/// <summary>
/// The Point structure defines the X- and Y- coordinates of a point. 
/// </summary>
/// <remarks>
/// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/gdi/rectangl_0tiq.asp
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
internal struct POINT
{
    /// <summary>
    /// Specifies the X-coordinate of the point. 
    /// </summary>
    public int X;
    /// <summary>
    /// Specifies the Y-coordinate of the point. 
    /// </summary>
    public int Y;

    public POINT(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static bool operator ==(POINT a, POINT b) => a.X == b.X && a.Y == b.Y;

    public static bool operator !=(POINT a, POINT b) => !(a == b);

    public bool Equals(POINT other) => other.X == X && other.Y == Y;

    public override bool Equals(object? obj) => ReferenceEquals(null, obj) ? false : obj.GetType() != typeof(POINT) ? false : Equals((POINT)obj);

    public override int GetHashCode()
    {
        unchecked
        {
            return (X * 397) ^ Y;
        }
    }
}