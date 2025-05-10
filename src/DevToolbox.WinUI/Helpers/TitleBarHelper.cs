﻿using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Windows.UI;
using Windows.UI.ViewManagement;
using WinUIEx;

namespace DevToolbox.WinUI.Helpers;

/// <summary>
/// Provides helper methods for updating the application title bar appearance
/// based on the current <see cref="ElementTheme"/> and system settings.
/// </summary>
public class TitleBarHelper
{
    private const int WAINACTIVE = 0x00;
    private const int WAACTIVE = 0x01;
    private const int WMACTIVATE = 0x0006;

    [DllImport("user32.dll")]
    private static extern nint GetActiveWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern nint SendMessage(nint hWnd, int msg, int wParam, nint lParam);

    /// <summary>
    /// Updates the title bar colors and activation state of the specified <see cref="WindowEx"/>
    /// according to the provided <see cref="ElementTheme"/> or system defaults.
    /// </summary>
    /// <param name="window">
    /// The <see cref="WindowEx"/> whose title bar should be updated. Must have ExtendsContentIntoTitleBar enabled.
    /// </param>
    /// <param name="theme">
    /// The desired <see cref="ElementTheme"/> (Default, Light, or Dark). If Default,
    /// system or application requested theme is used.
    /// </param>
    public static void UpdateTitleBar(WindowEx window, ElementTheme theme)
    {
        if (window!.ExtendsContentIntoTitleBar)
        {
            if (theme == ElementTheme.Default)
            {
                var uiSettings = new UISettings();
                var background = uiSettings.GetColorValue(UIColorType.Background);

                theme = background == Colors.White ? ElementTheme.Light : ElementTheme.Dark;
            }

            if (theme == ElementTheme.Default)
            {
                theme = Application.Current.RequestedTheme == ApplicationTheme.Light ? ElementTheme.Light : ElementTheme.Dark;
            }

            window!.AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;

            window!.AppWindow.TitleBar.ButtonForegroundColor = theme switch
            {
                ElementTheme.Dark => Colors.White,
                ElementTheme.Light => Colors.Black,
                _ => Colors.Transparent
            };

            window!.AppWindow.TitleBar.ButtonHoverForegroundColor = theme switch
            {
                ElementTheme.Dark => Colors.White,
                ElementTheme.Light => Colors.Black,
                _ => Colors.Transparent
            };

            window!.AppWindow.TitleBar.ButtonHoverBackgroundColor = theme switch
            {
                ElementTheme.Dark => Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF),
                ElementTheme.Light => Color.FromArgb(0x33, 0x00, 0x00, 0x00),
                _ => Colors.Transparent
            };

            window!.AppWindow.TitleBar.ButtonPressedBackgroundColor = theme switch
            {
                ElementTheme.Dark => Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF),
                ElementTheme.Light => Color.FromArgb(0x66, 0x00, 0x00, 0x00),
                _ => Colors.Transparent
            };

            window!.AppWindow.TitleBar.BackgroundColor = Colors.Transparent;

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            if (hwnd == GetActiveWindow())
            {
                SendMessage(hwnd, WMACTIVATE, WAINACTIVE, nint.Zero);
                SendMessage(hwnd, WMACTIVATE, WAACTIVE, nint.Zero);
            }
            else
            {
                SendMessage(hwnd, WMACTIVATE, WAACTIVE, nint.Zero);
                SendMessage(hwnd, WMACTIVATE, WAINACTIVE, nint.Zero);
            }
        }
    }
}
