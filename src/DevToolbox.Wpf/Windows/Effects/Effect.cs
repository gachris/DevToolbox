using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using DevToolbox.Wpf.Interop;
using DevToolbox.Wpf.Media;

namespace DevToolbox.Wpf.Windows.Effects;

/// <summary>
/// Represents a base class for applying visual effects to WPF windows.
/// This class handles the setup, application, and removal of effects 
/// utilizing the Desktop Window Manager (DWM) API.
/// </summary>
public abstract class Effect
{
    #region Fields/Consts

    private Window? _window;
    private IntPtr _hwnd;
    private HwndSource? _hwndSource;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the type of system backdrop associated with this effect.
    /// Derived classes must override this property to specify the 
    /// appropriate backdrop type.
    /// </summary>
    internal abstract DWM_SYSTEMBACKDROP_TYPE SystemBackdropType { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Attaches the effect to the specified <see cref="Window"/> instance.
    /// This method sets up the necessary interop handles and applies 
    /// the effect when the window is loaded.
    /// </summary>
    /// <param name="window">The window to which the effect will be attached.</param>
    internal void Attach(Window window)
    {
        _window = window;
        _hwnd = new WindowInteropHelper(_window).Handle;

        if (IntPtr.Zero != _hwnd)
        {
            _hwndSource = HwndSource.FromHwnd(_hwnd);
        }
        else
        {
            _window.SourceInitialized += (sender, e) =>
            {
                _hwnd = new WindowInteropHelper(_window).Handle;
                _hwndSource = HwndSource.FromHwnd(_hwnd);

                Apply();
            };
        }

        if (_window.IsLoaded)
        {
            Apply();
        }
    }

    /// <summary>
    /// Detaches the effect from the associated <see cref="Window"/> 
    /// and resets any DWM attributes.
    /// </summary>
    internal void Detach()
    {
        if (_hwnd == IntPtr.Zero)
            return;

        var systemBackdropType = (uint)DWM_SYSTEMBACKDROP_TYPE.DWMSBT_NONE;
        _ = Dwmapi.DwmSetWindowAttribute(_hwnd, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, ref systemBackdropType, sizeof(uint));
    }

    /// <summary>
    /// Applies the visual effect to the associated window, setting 
    /// the system backdrop type and refreshing the frame and dark mode.
    /// </summary>
    private void Apply()
    {
        if (_window is null)
        {
            throw new ArgumentNullException(nameof(_window));
        }

        ThemeManager.ApplicationThemeCoreChanged += (_, _) => RefreshDarkMode();

        RefreshFrame();
        RefreshDarkMode();

        var systemBackdropType = (uint)SystemBackdropType;
        _ = Dwmapi.DwmSetWindowAttribute(_hwnd, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, ref systemBackdropType, sizeof(uint));
    }

    /// <summary>
    /// Refreshes the window frame to extend the DWM frame into the 
    /// client area, allowing for custom window designs.
    /// </summary>
    private void RefreshFrame()
    {
        if (_hwndSource is null)
            return;

        _hwndSource.CompositionTarget.BackgroundColor = SystemParameters.HighContrast ? Colors.White : Colors.Transparent;

        var margins = new MARGINS
        {
            cxLeftWidth = -1,
            cxRightWidth = -1,
            cyTopHeight = -1,
            cyBottomHeight = -1
        };

        _ = Dwmapi.DwmExtendFrameIntoClientArea(_hwndSource.Handle, ref margins);
    }

    /// <summary>
    /// Refreshes the dark mode setting for the window based on the 
    /// current application theme.
    /// </summary>
    private void RefreshDarkMode()
    {
        var flag = ThemeManager.ApplicationTheme is ApplicationTheme.Dark ? (uint)1 : 0;
        _ = Dwmapi.DwmSetWindowAttribute(_hwnd, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, ref flag, sizeof(uint));
    }

    #endregion
}