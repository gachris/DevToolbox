using System;

namespace DevToolbox.Core.Contracts;

/// <summary>
/// Provides data for navigation events, carrying the content being navigated to and an optional parameter.
/// </summary>
public class NavigationEventArgs : EventArgs
{
    /// <summary>
    /// Gets the new page or control instance that has been navigated to.
    /// </summary>
    public object Content { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationEventArgs"/> class.
    /// </summary>
    /// <param name="content">The content (page or control) that was navigated to.</param>
    public NavigationEventArgs(object content)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }
}