using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace DevToolbox.Wpf.Helpers;

/// <summary>
/// Provides helper methods for retrieving and parsing clipboard data in a structured format.
/// Supports both CSV and plain text data from the clipboard.
/// </summary>
internal static class ClipboardHelper
{
    private delegate string[] ParseFormat(string value);

    /// <summary>
    /// Retrieves the clipboard data as a list of string arrays.
    /// Each array represents a row of data, and each element in the array represents a cell value.
    /// Supports CSV and plain text formats.
    /// </summary>
    /// <returns>A list of string arrays representing the clipboard data, or <c>null</c> if no data is available.</returns>
    public static List<string[]>? GetData()
    {
        List<string[]>? clipboardData = null;
        object? clipboardRawData = null;
        ParseFormat? parseFormat = null;

        // Get the clipboard data object.
        var dataObj = Clipboard.GetDataObject();
        if (dataObj == null)
        {
            return null;
        }

        // Determine the format of the clipboard data (CSV or plain text).
        if ((clipboardRawData = dataObj.GetData(DataFormats.CommaSeparatedValue)) != null)
        {
            parseFormat = ParseCsvFormat;
        }
        else if ((clipboardRawData = dataObj.GetData(DataFormats.Text)) != null)
        {
            parseFormat = ParseTextFormat;
        }

        // Parse the clipboard data using the appropriate format.
        if (parseFormat != null)
        {
            var rawDataStr = clipboardRawData as string;

            // Read the data from a MemoryStream if necessary.
            if (rawDataStr == null && clipboardRawData is MemoryStream ms)
            {
                using var sr = new StreamReader(ms);
                rawDataStr = sr.ReadToEnd();
            }

            // Split the raw data into rows and parse each row.
            var rows = rawDataStr?.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            clipboardData = rows.Select(row => parseFormat(row)).ToList();
        }

        return clipboardData;
    }

    /// <summary>
    /// Parses a row of CSV data into an array of strings.
    /// </summary>
    /// <param name="value">The row of CSV data to parse.</param>
    /// <returns>An array of strings representing the parsed cell values.</returns>
    private static string[] ParseCsvFormat(string value) => ParseCsvOrTextFormat(value, true);

    /// <summary>
    /// Parses a row of plain text data into an array of strings.
    /// </summary>
    /// <param name="value">The row of plain text data to parse.</param>
    /// <returns>An array of strings representing the parsed cell values.</returns>
    private static string[] ParseTextFormat(string value) => ParseCsvOrTextFormat(value, false);

    /// <summary>
    /// Parses a row of data (CSV or plain text) into an array of strings.
    /// Supports quoted CSV fields.
    /// </summary>
    /// <param name="value">The row of data to parse.</param>
    /// <param name="isCsv">Indicates whether the data is in CSV format.</param>
    /// <returns>An array of strings representing the parsed cell values.</returns>
    private static string[] ParseCsvOrTextFormat(string value, bool isCsv)
    {
        var outputList = new List<string>();
        var separator = isCsv ? ',' : '\t';
        var startIndex = 0;
        var inQuotes = false;

        for (var i = 0; i < value.Length; i++)
        {
            var ch = value[i];

            // Handle quoted CSV fields.
            if (isCsv && ch == '\"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            // Add the current value if the separator is found outside of quotes.
            if (ch == separator && !inQuotes)
            {
                outputList.Add(value.Substring(startIndex, i - startIndex).Trim('\"'));
                startIndex = i + 1;
            }
            // Add the last value when the end of the line is reached.
            else if (i == value.Length - 1)
            {
                outputList.Add(value.Substring(startIndex, i - startIndex + 1).Trim('\"'));
            }
        }

        return outputList.ToArray();
    }
}