﻿using System.Runtime.InteropServices;

namespace DevToolbox.Wpf.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct RECT
{
    public int Left, Top, Right, Bottom;

    public RECT(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

    public int X
    {
        get => Left;
        set { Right -= (Left - value); Left = value; }
    }

    public int Y
    {
        get => Top;
        set { Bottom -= (Top - value); Top = value; }
    }

    public int Height
    {
        get => Bottom - Top;
        set => Bottom = value + Top;
    }

    public int Width
    {
        get => Right - Left;
        set => Right = value + Left;
    }

    public System.Drawing.Point Location
    {
        get => new System.Drawing.Point(Left, Top);
        set { X = value.X; Y = value.Y; }
    }

    public System.Drawing.Size Size
    {
        get => new System.Drawing.Size(Width, Height);
        set { Width = value.Width; Height = value.Height; }
    }

    public static implicit operator System.Drawing.Rectangle(RECT r) => new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);

    public static implicit operator RECT(System.Drawing.Rectangle r) => new RECT(r);

    public static bool operator ==(RECT r1, RECT r2) => r1.Equals(r2);

    public static bool operator !=(RECT r1, RECT r2) => !r1.Equals(r2);

    public bool Equals(RECT r) => r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;

    public override bool Equals(object? obj)
    {
        if (obj is RECT)
            return Equals((RECT)obj);
        else if (obj is System.Drawing.Rectangle)
            return Equals(new RECT((System.Drawing.Rectangle)obj));
        return false;
    }

    public override int GetHashCode() => ((System.Drawing.Rectangle)this).GetHashCode();

    public bool Contains(POINT point) => Left <= point.X && Right >= point.X && Top <= point.Y && Bottom >= point.Y;

    public override string ToString() => string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);

    public void Offset(int dx, int dy)
    {
        Left += dx;
        Top += dy;
        Right += dx;
        Bottom += dy;
    }

}
