using DevToolbox.Wpf.Interop;

namespace DevToolbox.Wpf.Windows.Effects;

/// <summary>
/// Represents an acrylic effect that can be applied to a window.
/// This effect provides a translucent backdrop to create a sense of depth and focus.
/// </summary>
public class Acrylic : Effect
{
    /// <summary>
    /// Gets the type of system backdrop associated with this effect.
    /// Overrides the base implementation to specify the acrylic backdrop for transient windows.
    /// </summary>
    internal override DWM_SYSTEMBACKDROP_TYPE SystemBackdropType => DWM_SYSTEMBACKDROP_TYPE.DWMSBT_TRANSIENTWINDOW;
}
