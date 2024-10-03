namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Defines an interface for formatting numeric values as strings.
/// This interface provides methods for formatting double, int, and uint values.
/// </summary>
public interface INumberFormatter
{
    /// <summary>
    /// Formats a nullable double value as a string.
    /// </summary>
    /// <param name="value">The double value to format, or null.</param>
    /// <returns>A formatted string representation of the double value.</returns>
    string FormatDouble(double? value);

    /// <summary>
    /// Formats a nullable integer value as a string.
    /// </summary>
    /// <param name="value">The integer value to format, or null.</param>
    /// <returns>A formatted string representation of the integer value.</returns>
    string FormatInt(int? value);

    /// <summary>
    /// Formats a nullable unsigned integer value as a string.
    /// </summary>
    /// <param name="value">The unsigned integer value to format, or null.</param>
    /// <returns>A formatted string representation of the unsigned integer value.</returns>
    string FormatUInt(uint? value);
}
