using System.Collections.ObjectModel;
using System.Windows;

namespace DevToolbox.Wpf.Data;

/// <summary>
/// Represents a column in a DataGrid summary table.
/// Inherits from <see cref="NotifyPropertyChanged"/> to provide property change notification.
/// </summary>
public class DataGridSummaryTableColumn : NotifyPropertyChanged
{
    #region Fields/Consts

    private ObservableCollection<DataGridSummaryTableCell> _cells = [];
    private string _columnName = null!;
    private Visibility _visibility;
    private double _width;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the collection of cells in this column.
    /// </summary>
    public ObservableCollection<DataGridSummaryTableCell> Cells => _cells;

    /// <summary>
    /// Gets or sets the name of the column.
    /// </summary>
    public string ColumnName
    {
        get => _columnName;
        set => SetProperty(ref _columnName, value);
    }

    /// <summary>
    /// Gets or sets the visibility of the column.
    /// </summary>
    public Visibility Visibility
    {
        get => _visibility;
        set => SetProperty(ref _visibility, value);
    }

    /// <summary>
    /// Gets or sets the width of the column.
    /// </summary>
    public double Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }

    #endregion
}