using DevToolbox.Wpf.Interop;

namespace DevToolbox.Wpf.Windows.Effects;

/// <summary>
/// Represents a Mica effect for WPF windows, which provides a blurred, 
/// translucent background effect that adapts to the application's theme.
/// </summary>
public class Mica : Effect
{
    /// <summary>
    /// Gets the type of system backdrop associated with the Mica effect.
    /// This is set to <see cref="DWM_SYSTEMBACKDROP_TYPE.DWMSBT_MAINWINDOW"/> 
    /// for main windows to enable the Mica effect.
    /// </summary>
    internal override DWM_SYSTEMBACKDROP_TYPE SystemBackdropType => DWM_SYSTEMBACKDROP_TYPE.DWMSBT_MAINWINDOW;
}
