using System;

namespace DevToolbox.Wpf.Data;

public class FilterListItem : NotifyPropertyChanged
{
    #region Fields/Consts

    public event EventHandler? ItemChecked;
    public event EventHandler? ItemUnChecked;

    private object? _name;
    private bool _isChecked;
    private bool _isSelectAll;
    private bool _hide;

    #endregion

    #region Properties

    public object? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

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

    public bool IsSelectAll
    {
        get => _isSelectAll;
        set => SetProperty(ref _isSelectAll, value);
    }

    public bool Hide
    {
        get => _hide;
        set => SetProperty(ref _hide, value);
    }

    #endregion
}