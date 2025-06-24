namespace DevToolbox.Core.Data;

/// <summary>
/// Represents a data field containing old and new values for change tracking or comparison.
/// </summary>
public class Field
{
    #region Properties

    /// <summary>
    /// Gets the original value of the field before the change.
    /// </summary>
    public object? OldValue { get; }

    /// <summary>
    /// Gets the new value of the field after the change.
    /// </summary>
    public object? NewValue { get; }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Field"/> class with the specified old and new values.
    /// </summary>
    /// <param name="oldValue">The original value of the field.</param>
    /// <param name="newValue">The new value of the field.</param>
    public Field(object? oldValue, object? newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}
