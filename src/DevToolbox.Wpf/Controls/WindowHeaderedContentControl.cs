using System.Windows;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A custom control that inherits from <see cref="HeaderedContentControl"/> and sets its own default style key.
/// Typically used for creating a window-like control with a header and content.
/// </summary>
public class WindowHeaderedContentControl : HeaderedContentControl
{
    /// <summary>
    /// Static constructor for <see cref="WindowHeaderedContentControl"/>.
    /// Overrides the default style key to associate the control with its style in XAML.
    /// This allows custom styling through generic.xaml or other resource dictionaries.
    /// </summary>
    static WindowHeaderedContentControl()
    {
        var handlerType = typeof(WindowHeaderedContentControl);
        DefaultStyleKeyProperty.OverrideMetadata(handlerType, new FrameworkPropertyMetadata(handlerType));
    }
}
