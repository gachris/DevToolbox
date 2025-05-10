using System.Threading.Tasks;
using DevToolbox.Core.Data;

namespace DevToolbox.Core.Contracts;

/// <summary>
/// Provides methods for displaying file open and save dialogs.
/// </summary>
public interface IFileDialogService
{
    /// <summary>
    /// Displays an Open File dialog asynchronously.
    /// </summary>
    /// <param name="filter">
    /// A filter string that determines which file types are displayed (e.g.,
    /// "Text files (*.txt)|*.txt|All files (*.*)|*.*").
    /// </param>
    /// <returns>
    /// A task that completes with a <see cref="FileDialogResult"/> containing
    /// the file path chosen by the user and whether the dialog was confirmed.
    /// </returns>
    Task<FileDialogResult> OpenFileDialogAsync(string filter);

    /// <summary>
    /// Displays a Save File dialog asynchronously.
    /// </summary>
    /// <param name="filter">
    /// A filter string that determines which file types are displayed (e.g.,
    /// "Text files (*.txt)|*.txt|All files (*.*)|*.*").
    /// </param>
    /// <param name="fileName">
    /// The default file name to display in the Save File dialog.
    /// </param>
    /// <returns>
    /// A task that completes with a <see cref="FileDialogResult"/> containing
    /// the file path entered by the user and whether the dialog was confirmed.
    /// </returns>
    Task<FileDialogResult> SaveFileDialogAsync(string filter, string fileName);
}