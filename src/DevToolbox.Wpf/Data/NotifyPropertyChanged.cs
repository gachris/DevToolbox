using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DevToolbox.Wpf.Data;

public abstract class NotifyPropertyChanged : INotifyPropertyChanged, INotifyDataErrorInfo
{
    #region Fields/Consts

    private readonly Dictionary<string, List<string>> _errors = [];

    #endregion

    #region INotifyPropertyChanged Implementation

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region INotifyDataErrorInfo Implementation

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public bool HasErrors => _errors.Values.Any(list => list != null && list.Count > 0);

    public IEnumerable GetErrors(string? propertyName)
    {
        return propertyName != null && _errors.ContainsKey(propertyName) ? _errors[propertyName] : Array.Empty<string>();
    }

    #endregion

    #region Methods

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null!)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        ValidateProperty(propertyName);
        return true;
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void AddError(string propertyName, string error)
    {
        if (!_errors.ContainsKey(propertyName))
            _errors[propertyName] = [];

        _errors[propertyName].Add(error);
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    protected void ClearErrors(string propertyName)
    {
        if (_errors.ContainsKey(propertyName))
        {
            _errors[propertyName].Clear();
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    protected virtual void ValidateProperty(string propertyName)
    {
    }

    #endregion
}