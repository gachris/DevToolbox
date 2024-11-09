using System;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Provides data for the column grouping event in a <see cref="GridControl"/>.
/// </summary>
public class DataGridColumnGroupByEventArgs : EventArgs
{
    /// <summary>
    /// Gets the <see cref="DataGridColumn"/> that is being grouped by.
    /// </summary>
    public DataGridColumn Column { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridColumnGroupByEventArgs"/> class with the specified column.
    /// </summary>
    /// <param name="column">The <see cref="DataGridColumn"/> that is being grouped by.</param>
    public DataGridColumnGroupByEventArgs(DataGridColumn column)
    {
        Column = column;
    }
}