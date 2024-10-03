namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Defines an interface for parsing numeric values from strings.
/// This interface provides methods for parsing strings into double, int, and uint values.
/// </summary>
public interface INumberParser
{
    /// <summary>
    /// Parses a string value into a nullable double.
    /// </summary>
    /// <param name="value">The string value to parse, or null.</param>
    /// <returns>
    /// The parsed double value, or null if the parsing fails or the input is invalid.
    /// </returns>
    double? ParseDouble(string? value);

    /// <summary>
    /// Parses a string value into a nullable integer.
    /// </summary>
    /// <param name="value">The string value to parse, or null.</param>
    /// <returns>
    /// The parsed integer value, or null if the parsing fails or the input is invalid.
    /// </returns>
    int? ParseInt(string? value);

    /// <summary>
    /// Parses a string value into a nullable unsigned integer.
    /// </summary>
    /// <param name="value">The string value to parse, or null.</param>
    /// <returns>
    /// The parsed unsigned integer value, or null if the parsing fails or the input is invalid.
    /// </returns>
    uint? ParseUInt(string? value);
}
