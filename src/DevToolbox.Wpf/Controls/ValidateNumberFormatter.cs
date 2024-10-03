using System.Globalization;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Provides functionality for formatting and parsing numeric values.
/// </summary>
public class ValidateNumberFormatter : INumberFormatter, INumberParser
{
    /// <summary>
    /// Formats a nullable double value to a string representation.
    /// </summary>
    /// <param name="value">The double value to format.</param>
    /// <returns>A formatted string if the value is not null; otherwise, an empty string.</returns>
    public string FormatDouble(double? value)
    {
        return value?.ToString(GetFormatSpecifier(), GetCurrentCultureConverter()) ?? string.Empty;
    }

    /// <summary>
    /// Formats a nullable integer value to a string representation.
    /// </summary>
    /// <param name="value">The integer value to format.</param>
    /// <returns>A formatted string if the value is not null; otherwise, an empty string.</returns>
    public string FormatInt(int? value)
    {
        return value?.ToString(GetFormatSpecifier(), GetCurrentCultureConverter()) ?? string.Empty;
    }

    /// <summary>
    /// Formats a nullable unsigned integer value to a string representation.
    /// </summary>
    /// <param name="value">The unsigned integer value to format.</param>
    /// <returns>A formatted string if the value is not null; otherwise, an empty string.</returns>
    public string FormatUInt(uint? value)
    {
        return value?.ToString(GetFormatSpecifier(), GetCurrentCultureConverter()) ?? string.Empty;
    }

    /// <summary>
    /// Parses a string to a nullable double value.
    /// </summary>
    /// <param name="value">The string representation of the double.</param>
    /// <returns>The parsed double if successful; otherwise, null.</returns>
    public double? ParseDouble(string? value)
    {
        return double.TryParse(value, out double d) ? d : null;
    }

    /// <summary>
    /// Parses a string to a nullable integer value.
    /// </summary>
    /// <param name="value">The string representation of the integer.</param>
    /// <returns>The parsed integer if successful; otherwise, null.</returns>
    public int? ParseInt(string? value)
    {
        return int.TryParse(value, out int i) ? i : null;
    }

    /// <summary>
    /// Parses a string to a nullable unsigned integer value.
    /// </summary>
    /// <param name="value">The string representation of the unsigned integer.</param>
    /// <returns>The parsed unsigned integer if successful; otherwise, null.</returns>
    public uint? ParseUInt(string? value)
    {
        return uint.TryParse(value, out uint ui) ? ui : (uint?)null;
    }

    /// <summary>
    /// Gets the format specifier for number formatting.
    /// </summary>
    /// <returns>A string representing the format specifier.</returns>
    private static string GetFormatSpecifier()
    {
        return "G"; // General format
    }

    /// <summary>
    /// Gets the current culture information for number formatting.
    /// </summary>
    /// <returns>The current <see cref="CultureInfo"/>.</returns>
    private static CultureInfo GetCurrentCultureConverter()
    {
        return CultureInfo.CurrentCulture;
    }
}
