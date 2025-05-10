using WinUIEx;

namespace DevToolbox.WinUI.Contracts;

/// <summary>
/// Manages application windows, providing access to the main window and
/// registration of additional windows.
/// </summary>
public interface IAppWindowService
{
    /// <summary>
    /// Gets the primary <see cref="WindowEx"/> instance used by the application.
    /// </summary>
    WindowEx MainWindow { get; }

    /// <summary>
    /// Registers an additional <see cref="WindowEx"/> with the service for
    /// lifecycle management and global coordination.
    /// </summary>
    /// <param name="windowEx">
    /// The <see cref="WindowEx"/> instance to register.
    /// </param>
    void Register(WindowEx windowEx);
}