using System;

namespace DevToolbox.Wpf.Data;

/// <summary>
/// Represents an item in the column chooser for a <see cref="Controls.GridControl"/>, allowing users to toggle visibility and set display order.
/// </summary>
public class DataGridColumnChooserItem : NotifyPropertyChanged
{
    #region Fields/Consts

    private bool _isChecked;
    private bool _isEnabled;
    private object _header = null!;
    private string _columnField = null!;
    private int _displayIndex;

    /// <summary>
    /// Occurs when the item is checked.
    /// </summary>
    public event EventHandler? Checked;

    /// <summary>
    /// Occurs when the item is unchecked.
    /// </summary>
    public event EventHandler? Unchecked;

    /// <summary>
    /// Occurs when the display index of the item is changed.
    /// </summary>
    public event EventHandler? DisplayIndexChanged;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets a value indicating whether the item is checked.
    /// </summary>
    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (SetProperty(ref _isChecked, value, nameof(IsChecked)))
            {
                if (value)
                    Checked?.Invoke(this, EventArgs.Empty);
                else
                    Unchecked?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the item is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }

    /// <summary>
    /// Gets or sets the header of the item.
    /// </summary>
    public object Header
    {
        get => _header;
        set => SetProperty(ref _header, value);
    }

    /// <summary>
    /// Gets or sets the field name associated with the column.
    /// </summary>
    public string ColumnField
    {
        get => _columnField;
        set => SetProperty(ref _columnField, value);
    }

    /// <summary>
    /// Gets or sets the display index of the item.
    /// </summary>
    public int DisplayIndex
    {
        get => _displayIndex;
        set
        {
            if (SetProperty(ref _displayIndex, value))
            {
                DisplayIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Compares two <see cref="DataGridColumnChooserItem"/> objects based on their display index.
    /// </summary>
    /// <param name="x">The first item to compare.</param>
    /// <param name="y">The second item to compare.</param>
    /// <returns>
    /// -1 if <paramref name="x"/> is less than <paramref name="y"/>,
    /// 1 if <paramref name="x"/> is greater than <paramref name="y"/>, or
    /// 0 if they are equal.
    /// </returns>
    public static int Compare(DataGridColumnChooserItem? x, DataGridColumnChooserItem? y)
    {
        if (x is null && y is null) return 0;
        if (x is null) return -1;
        if (y is null) return 1;

        // Compare based on DisplayIndex.
        return x.DisplayIndex.CompareTo(y.DisplayIndex);
    }

    #endregion
}