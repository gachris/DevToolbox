namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Specifies the validation modes for the <see cref="NumberBox"/> control.
/// This enum defines how the <see cref="NumberBox"/> behaves when invalid input is entered.
/// </summary>
public enum NumberBoxValidationMode
{
    /// <summary>
    /// Indicates that invalid input will be overwritten with the last valid value.
    /// </summary>
    InvalidInputOverwritten,

    /// <summary>
    /// Indicates that input validation is disabled.
    /// Invalid input will not trigger any validation behavior.
    /// </summary>
    Disabled
}
