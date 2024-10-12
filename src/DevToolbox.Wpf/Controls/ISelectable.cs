using System.Windows;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Defines an interface for elements that can be selected within a user interface.
/// </summary>
public interface ISelectable : IInputElement
{
    /// <summary>
    /// Gets or sets a value indicating whether the element is selected.
    /// </summary>
    bool IsSelected { get; set; }
}