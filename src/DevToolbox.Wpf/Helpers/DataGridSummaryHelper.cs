using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using DevToolbox.Wpf.Controls;

namespace DevToolbox.Wpf.Helpers;

internal static class DataGridSummaryHelper
{
    /// <summary>
    /// Gets the summary value based on the specified <see cref="DataGridSummaryItem"/>.
    /// </summary>
    /// <param name="itemsSource">The source <see cref="DataTable"/>.</param>
    /// <param name="gridSummaryItem">The summary item configuration.</param>
    /// <returns>The formatted summary value.</returns>
    public static object GetValue(DataTable itemsSource, DataGridSummaryItem gridSummaryItem)
    {
        if (itemsSource.Rows.Count == 0)
            return FormatValue(0, gridSummaryItem);

        object? value = null;
        var fieldName = gridSummaryItem.FieldName;

        switch (gridSummaryItem.SummaryType)
        {
            case DataGridSummaryItemType.Count:
                value = itemsSource.Rows.Count;
                break;

            case DataGridSummaryItemType.Sum:
                value = SafeCompute(itemsSource, $"Sum({fieldName})");
                break;

            case DataGridSummaryItemType.Average:
                value = GetAverage(itemsSource, fieldName);
                break;

            case DataGridSummaryItemType.Min:
                value = SafeCompute(itemsSource, $"Min({fieldName})");
                break;

            case DataGridSummaryItemType.Max:
                value = SafeCompute(itemsSource, $"Max({fieldName})");
                break;

            case DataGridSummaryItemType.Smallest:
                value = GetSmallestValue(itemsSource, fieldName);
                break;

            case DataGridSummaryItemType.Largest:
                value = GetLargestValue(itemsSource, fieldName);
                break;

            case DataGridSummaryItemType.Custom:
                if (gridSummaryItem.CustomCallback != null)
                {
                    value = gridSummaryItem.CustomCallback.Invoke(itemsSource, fieldName);
                }
                else
                {
                    throw new InvalidOperationException("Custom summary type requires a non-null CustomCallback.");
                }
                break;

            case DataGridSummaryItemType.None:
                value = null;
                break;

            default:
                throw new InvalidEnumArgumentException("Unsupported summary type encountered.", (int)gridSummaryItem.SummaryType, typeof(DataGridSummaryItemType));
        }

        return FormatValue(value ?? string.Empty, gridSummaryItem);
    }

    /// <summary>
    /// Safely computes an aggregate function on the DataTable.
    /// </summary>
    private static object SafeCompute(DataTable table, string expression)
    {
        try
        {
            return Convert.ToDouble(table.Compute(expression, ""), CultureInfo.CurrentCulture);
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Gets the average value of a column.
    /// </summary>
    private static double GetAverage(DataTable table, string fieldName)
    {
#if NET462_OR_GREATER
        return table.DefaultView.Cast<DataRowView>().Average(row => Convert.ToDouble(row[fieldName]));
#elif NET6_0_OR_GREATER
        return table.AsEnumerable().Average(row => Convert.ToDouble(row[fieldName], CultureInfo.CurrentCulture));
#endif
    }

    /// <summary>
    /// Gets the smallest string value based on its length.
    /// </summary>
    private static string GetSmallestValue(DataTable table, string fieldName)
    {
#if NET462_OR_GREATER
        var collection = table.DefaultView.Cast<DataRowView>();
#elif NET6_0_OR_GREATER
        var collection = table.AsEnumerable();
#endif
        return collection
            .Select(row => Convert.ToString(row[fieldName]) ?? string.Empty)
            .OrderBy(str => str.Length)
            .FirstOrDefault() ?? string.Empty;
    }

    /// <summary>
    /// Gets the largest string value based on its length.
    /// </summary>
    private static string GetLargestValue(DataTable table, string fieldName)
    {
#if NET462_OR_GREATER
        var collection = table.DefaultView.Cast<DataRowView>();
#elif NET6_0_OR_GREATER
        var collection = table.AsEnumerable();
#endif
        return collection
            .Select(row => Convert.ToString(row[fieldName]) ?? string.Empty)
            .OrderByDescending(str => str.Length)
            .FirstOrDefault() ?? string.Empty;
    }

    /// <summary>
    /// Formats the summary value using the specified display format.
    /// </summary>
    private static string FormatValue(object value, DataGridSummaryItem gridSummaryItem)
    {
        return string.Format(CultureInfo.CurrentCulture, gridSummaryItem.DisplayFormat ?? "{0}", Convert.ToDecimal(value, CultureInfo.CurrentCulture));
    }
}