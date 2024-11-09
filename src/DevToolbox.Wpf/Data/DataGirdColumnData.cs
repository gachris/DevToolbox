using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevToolbox.Wpf.Data;

/// <summary>
/// Represents the essential data of a <see cref="DataGridColumn"/>, including header, property path,
/// sorting information, display index, width, visibility, and sort member path.
/// </summary>
internal struct DataGridColumnData
{
    #region Fields/Consts

    /// <summary>
    /// The header of the column.
    /// </summary>
    public object Header;

    /// <summary>
    /// The property path used for data binding.
    /// </summary>
    public string PropertyPath;

    /// <summary>
    /// The sort direction (ascending or descending) for the column.
    /// </summary>
    public ListSortDirection? SortDirection;

    /// <summary>
    /// The display index of the column in the DataGrid.
    /// </summary>
    public int DisplayIndex;

    /// <summary>
    /// The width value of the column.
    /// </summary>
    public double WidthValue;

    /// <summary>
    /// The unit type of the column width (e.g., Pixel, Star).
    /// </summary>
    public DataGridLengthUnitType WidthType;

    /// <summary>
    /// The sort member path used for sorting the column.
    /// </summary>
    public string SortMemberPath;

    /// <summary>
    /// Indicates whether the column is visible.
    /// </summary>
    public bool IsVisible;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridColumnData"/> struct with data from the specified <see cref="DataGridColumn"/>.
    /// </summary>
    /// <param name="column">The <see cref="DataGridColumn"/> from which to extract data.</param>
    public DataGridColumnData(DataGridColumn column)
    {
        Header = column.Header;

        SortMemberPath = column.SortMemberPath;
        WidthValue = column.Width.DisplayValue;
        WidthType = column.Width.UnitType;
        SortDirection = column.SortDirection;
        DisplayIndex = column.DisplayIndex;
        IsVisible = column.Visibility == Visibility.Visible;

        if (column is DataGridBoundColumn boundColumn)
        {
            PropertyPath = (boundColumn.Binding as Binding)?.Path?.Path ?? string.Empty;
        }
        else
        {
            PropertyPath = column.SortMemberPath ?? string.Empty;
        }
    }

    #region Methods

    /// <summary>
    /// Applies the stored column data to the specified <see cref="DataGridColumn"/>.
    /// This includes setting the width, sort direction, display index, visibility, and sort member path.
    /// </summary>
    /// <param name="column">The <see cref="DataGridColumn"/> to which the data will be applied.</param>
    /// <param name="gridColumnCount">The total count of columns in the DataGrid.</param>
    /// <param name="sortDescriptions">The collection of sort descriptions to update.</param>
    public readonly void Apply(DataGridColumn column, int gridColumnCount, SortDescriptionCollection sortDescriptions)
    {
        column.Width = new DataGridLength(WidthValue, WidthType);
        column.SortDirection = SortDirection;

        // Add sort description if sort direction is specified.
        if (SortDirection != null)
        {
            sortDescriptions.Add(new SortDescription(PropertyPath, SortDirection.Value));
        }

        // Adjust display index if necessary.
        if (column.DisplayIndex != DisplayIndex)
        {
            var maxIndex = gridColumnCount == 0 ? 0 : gridColumnCount - 1;
            column.DisplayIndex = DisplayIndex <= maxIndex ? DisplayIndex : maxIndex;
        }

        // Set column visibility and sort member path.
        column.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
        column.SortMemberPath = SortMemberPath;
    }

    #endregion
}
