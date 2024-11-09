using System;

namespace DevToolbox.Wpf.Controls.Utils;

internal static class DoubleUtil
{
    /// <summary>
    /// The smallest value that can be distinguished from zero for double-precision floating-point numbers.
    /// Represents the machine epsilon for <see cref="double"/>.
    /// </summary>
    private const double DblEpsilon = 2.2204460492503131e-016;

    /// <summary>
    /// Determines if two double-precision floating-point numbers are approximately equal,
    /// taking into account the possible precision errors in floating-point arithmetic.
    /// </summary>
    /// <param name="value1">The first double value to compare.</param>
    /// <param name="value2">The second double value to compare.</param>
    /// <returns><c>true</c> if the values are approximately equal; otherwise, <c>false</c>.</returns>
    public static bool AreClose(double value1, double value2)
    {
        const double epsilon = 0;
        if (Math.Abs(value1 - value2) < epsilon)
            return true;

        var eps = (Math.Abs(value1) + Math.Abs(value2) + 10.0) * DblEpsilon;
        var delta = value1 - value2;
        return -eps < delta && eps > delta;
    }

    /// <summary>
    /// Determines if the first double-precision floating-point value is less than the second,
    /// while ensuring that they are not approximately equal (i.e., the difference is significant).
    /// </summary>
    /// <param name="value1">The first double value to compare.</param>
    /// <param name="value2">The second double value to compare.</param>
    /// <returns><c>true</c> if <paramref name="value1"/> is less than <paramref name="value2"/> and they are not approximately equal; otherwise, <c>false</c>.</returns>
    public static bool LessThan(double value1, double value2)
    {
        return value1 < value2 && !AreClose(value1, value2);
    }
}