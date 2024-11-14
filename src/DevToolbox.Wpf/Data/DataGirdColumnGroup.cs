using System.ComponentModel;

namespace DevToolbox.Wpf.Data;

/// <summary>
/// Represents data used for grouping in a <see cref="Controls.GridControl"/>, including information about the column, sorting, and group index.
/// </summary>
public class DataGirdColumnGroup : NotifyPropertyChanged
{
    #region Fields/Consts

    private string _columnName = null!;
    private ListSortDirection? _sortDirection;
    private int _index;
    private string _sortMemberPath = null!;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the name of the column used for grouping.
    /// </summary>
    public string ColumnName
    {
        get => _columnName;
        set => SetProperty(ref _columnName, value);
    }

    /// <summary>
    /// Gets or sets the sort direction (ascending or descending) for the grouped column.
    /// </summary>
    public ListSortDirection? SortDirection
    {
        get => _sortDirection;
        set => SetProperty(ref _sortDirection, value);
    }

    /// <summary>
    /// Gets or sets the index of the group in the grouped collection.
    /// </summary>
    public int Index
    {
        get => _index;
        set => SetProperty(ref _index, value);
    }

    /// <summary>
    /// Gets or sets the path to the member used for sorting within the group.
    /// </summary>
    public string SortMemberPath
    {
        get => _sortMemberPath;
        set => SetProperty(ref _sortMemberPath, value);
    }

    #endregion
}