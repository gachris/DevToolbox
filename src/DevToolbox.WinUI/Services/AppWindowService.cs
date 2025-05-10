using System;
using DevToolbox.WinUI.Contracts;
using WinUIEx;

namespace DevToolbox.WinUI.Services;

/// <summary>
/// Provides a concrete implementation of <see cref="IAppWindowService"/>,
/// managing the main application window instance.
/// </summary>
public class AppWindowService : IAppWindowService
{
    #region Properties

    /// <summary>
    /// Gets the primary <see cref="WindowEx"/> instance for the application.
    /// </summary>
    public WindowEx MainWindow { get; private set; } = null!;

    #endregion

    #region IAppWindowService Implementation

    /// <summary>
    /// Registers the specified <see cref="WindowEx"/> as the main window.
    /// </summary>
    /// <param name="windowEx">
    /// The <see cref="WindowEx"/> to set as the main application window.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the main window has already been registered.
    /// </exception>
    public void Register(WindowEx windowEx)
    {
        if (MainWindow != null)
        {
            throw new InvalidOperationException(
                "MainWindow has already been registered. It cannot be registered again.");
        }

        MainWindow = windowEx;
    }

    #endregion
}