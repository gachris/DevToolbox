using System;
using System.Collections.Generic;
using System.Windows;

namespace DevToolbox.Wpf.Utils;

internal static class PortraitViewItemHelpers
{
    public static List<byte> ToBytes(this string str)
    {
        List<byte> result = new();
        result.AddRange(BitConverter.GetBytes(str.Length));
        for (uint i = 0; i < str.Length; i++)
        {
            char ch = str[(int)i];
            result.AddRange(BitConverter.GetBytes(ch));
        }
        return result;
    }

    public static byte[] ToBytes(this int num)
    {
        byte[] intBytes = BitConverter.GetBytes(num);
        Array.Reverse(intBytes);
        return intBytes;
    }

    public static byte[] ToBytes(this double num)
    {
        byte[] intBytes = BitConverter.GetBytes(num);
        Array.Reverse(intBytes);
        return intBytes;
    }

    public static int FromBytesInt32(Queue<byte> q)
    {
        byte[] b = new byte[sizeof(Int32)];
        for (uint i = 0; i < b.Length; i++)
        {
            b[i] = q.Dequeue();
        }
        return BitConverter.ToInt32(b, 0);
    }

    public static string FromBytesString(Queue<byte> q)
    {
        int stringLength = FromBytesInt32(q);
        string s = "";
        for (uint i = 0; i < stringLength; i++)
        {
            s += FromBytesChar(q);
        }
        return s;
    }

    public static char FromBytesChar(Queue<byte> q)
    {
        byte[] b = new byte[sizeof(char)];
        for (uint i = 0; i < b.Length; i++)
        {
            b[i] = q.Dequeue();
        }
        return BitConverter.ToChar(b, 0);
    }

    public static List<byte> ToBytes(this Point point)
    {
        List<byte> result = new();
        result.AddRange(BitConverter.GetBytes(point.X));
        result.AddRange(BitConverter.GetBytes(point.Y));
        return result;
    }

    public static double FromBytesDouble(Queue<byte> q)
    {
        byte[] b = new byte[sizeof(double)];
        for (uint i = 0; i < b.Length; i++)
        {
            b[i] = q.Dequeue();
        }
        return BitConverter.ToDouble(b, 0);
    }

    public static Point FromBytesPoint(Queue<Byte> q) => new Point(
            FromBytesDouble(q),
            FromBytesDouble(q));
}