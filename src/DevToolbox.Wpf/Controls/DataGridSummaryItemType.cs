namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents the types of summary calculations that can be applied in a DataGrid.
/// </summary>
public enum DataGridSummaryItemType
{
    /// <summary>
    /// Represents the sum of all values.
    /// </summary>
    Sum = 0,

    /// <summary>
    /// Represents the average of all values.
    /// </summary>
    Average = 1,

    /// <summary>
    /// Represents the count of all items.
    /// </summary>
    Count = 2,

    /// <summary>
    /// Represents the minimum value in the dataset.
    /// </summary>
    Min = 3,

    /// <summary>
    /// Represents the maximum value in the dataset.
    /// </summary>
    Max = 4,

    /// <summary>
    /// Represents the smallest (lowest) value in the dataset.
    /// </summary>
    Smallest = 5,

    /// <summary>
    /// Represents the largest (highest) value in the dataset.
    /// </summary>
    Largest = 6,

    /// <summary>
    /// Represents a custom summary calculation defined by the user.
    /// </summary>
    Custom = 7,

    /// <summary>
    /// Represents no summary calculation.
    /// </summary>
    None = 8
}