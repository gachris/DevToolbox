using System.Windows;

namespace DevToolbox.Wpf.Data;

/// <summary>
/// Represents a cell in a DataGrid summary table.
/// Inherits from <see cref="NotifyPropertyChanged"/> to provide property change notification.
/// </summary>
public class DataGridSummaryTableCell : NotifyPropertyChanged
{
    #region Fields/Consts

    private object? _value;
    private Style? _style;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the value of the cell.
    /// </summary>
    public object? Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    /// <summary>
    /// Gets or sets the style of the cell.
    /// </summary>
    public Style? Style
    {
        get => _style;
        set => SetProperty(ref _style, value);
    }

    #endregion
}