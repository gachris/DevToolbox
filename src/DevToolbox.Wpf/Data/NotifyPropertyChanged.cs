using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DevToolbox.Wpf.Data;

/// <summary>
/// Abstract base class implementing <see cref="INotifyPropertyChanged"/> and <see cref="INotifyDataErrorInfo"/>,
/// providing notification of property changes and data validation error tracking.
/// </summary>
public abstract class NotifyPropertyChanged : INotifyPropertyChanged, INotifyDataErrorInfo
{
    #region Fields/Consts

    /// <summary>
    /// Stores the validation errors for properties.
    /// </summary>
    private readonly Dictionary<string, List<string>> _errors = [];

    #endregion

    #region INotifyPropertyChanged Implementation

    /// <summary>
    /// Event that is raised when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region INotifyDataErrorInfo Implementation

    /// <summary>
    /// Event that is raised when the validation errors for a property have changed.
    /// </summary>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <summary>
    /// Gets a value indicating whether the entity has validation errors.
    /// </summary>
    public bool HasErrors => _errors.Values.Any(list => list != null && list.Count > 0);

    /// <summary>
    /// Gets the validation errors for a specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property to retrieve errors for.</param>
    /// <returns>An <see cref="IEnumerable"/> of validation error messages for the property.</returns>
    public IEnumerable GetErrors(string? propertyName)
    {
        return propertyName != null && _errors.ContainsKey(propertyName) ? _errors[propertyName] : Array.Empty<string>();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Sets the value of a property and raises the <see cref="PropertyChanged"/> event if the value has changed.
    /// Also triggers validation for the property.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">The field storing the current value of the property.</param>
    /// <param name="value">The new value to set.</param>
    /// <param name="propertyName">The name of the property. This is automatically provided by the compiler.</param>
    /// <returns><c>true</c> if the property value was changed; otherwise, <c>false</c>.</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null!)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        ValidateProperty(propertyName);
        return true;
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for a specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Adds a validation error for a specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that has the error.</param>
    /// <param name="error">The error message to add.</param>
    protected void AddError(string propertyName, string error)
    {
        if (!_errors.ContainsKey(propertyName))
            _errors[propertyName] = [];

        _errors[propertyName].Add(error);
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Clears all validation errors for a specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property for which to clear errors.</param>
    protected void ClearErrors(string propertyName)
    {
        if (_errors.ContainsKey(propertyName))
        {
            _errors[propertyName].Clear();
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Validates the specified property. This method can be overridden in a derived class to provide custom validation logic.
    /// </summary>
    /// <param name="propertyName">The name of the property to validate.</param>
    protected virtual void ValidateProperty(string propertyName)
    {
    }

    #endregion
}