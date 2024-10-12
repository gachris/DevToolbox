using System;
using System.Windows;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Defines an interface for elements that can be grouped in a design canvas.
/// </summary>
public interface IGroupable : IInputElement
{
    /// <summary>
    /// Gets the unique identifier of the element.
    /// </summary>
    Guid ID { get; }

    /// <summary>
    /// Gets or sets the unique identifier of the parent element or group.
    /// </summary>
    Guid ParentID { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the element is part of a group.
    /// </summary>
    bool IsGroup { get; set; }
}