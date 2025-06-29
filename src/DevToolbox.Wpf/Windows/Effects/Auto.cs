using DevToolbox.Wpf.Interop;

namespace DevToolbox.Wpf.Windows.Effects;

/// <summary>
/// Represents a tabbed effect that can be applied to a window.
/// This effect is designed for windows that are organized into tabbed groups, 
/// providing a distinct visual style that supports tabbed interactions.
/// </summary>
public class Auto : Effect
{
    /// <summary>
    /// Gets the type of system backdrop associated with this effect.
    /// Overrides the base implementation to specify the tabbed backdrop for windows.
    /// </summary>
    internal override DWM_SYSTEMBACKDROP_TYPE SystemBackdropType => DWM_SYSTEMBACKDROP_TYPE.DWMSBT_AUTO;
}
