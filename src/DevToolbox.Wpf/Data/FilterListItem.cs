using System;

namespace DevToolbox.Wpf.Data;

/// <summary>
/// Represents an item in a filter list that can be checked or unchecked.
/// Inherits from <see cref="NotifyPropertyChanged"/> to provide property change notification and validation.
/// </summary>
public class FilterListItem : NotifyPropertyChanged
{
    #region Fields/Consts

    /// <summary>
    /// Event raised when the item is checked.
    /// </summary>
    public event EventHandler? ItemChecked;

    /// <summary>
    /// Event raised when the item is unchecked.
    /// </summary>
    public event EventHandler? ItemUnChecked;

    private object? _name;
    private bool _isChecked;
    private bool _isSelectAll;
    private bool _hide;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the name of the filter list item.
    /// </summary>
    public object? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the item is checked.
    /// Raises <see cref="ItemChecked"/> or <see cref="ItemUnChecked"/> events accordingly.
    /// </summary>
    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            SetProperty(ref _isChecked, value);
            if (value)
                ItemChecked?.Invoke(this, EventArgs.Empty);
            else
                ItemUnChecked?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this item represents a "Select All" option.
    /// </summary>
    public bool IsSelectAll
    {
        get => _isSelectAll;
        set => SetProperty(ref _isSelectAll, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the item is hidden.
    /// </summary>
    public bool Hide
    {
        get => _hide;
        set => SetProperty(ref _hide, value);
    }

    #endregion
}