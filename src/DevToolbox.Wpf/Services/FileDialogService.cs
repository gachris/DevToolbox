using System.Threading.Tasks;
using DevToolbox.Core.Contracts;
using DevToolbox.Core.Data;
using Microsoft.Win32;

namespace DevToolbox.Wpf.Services;

/// <summary>
/// Provides file open and save dialog functionality for WPF applications using
/// the Microsoft.Win32 file dialogs.
/// </summary>
public class FileDialogService : IFileDialogService
{
    #region IFileDialogService Implementation

    /// <summary>
    /// Displays an Open File dialog with the specified filter asynchronously.
    /// </summary>
    /// <param name="filter">
    /// A filter string in the format "Description|*.ext;*.ext|..." to restrict file types.
    /// </param>
    /// <returns>
    /// A <see cref="FileDialogResult"/> containing the dialog result and selected file path.
    /// </returns>
    public Task<FileDialogResult> OpenFileDialogAsync(string filter)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = filter
        };

        bool? result = openFileDialog.ShowDialog();
        return Task.FromResult(new FileDialogResult(result, openFileDialog.FileName));
    }

    /// <summary>
    /// Displays a Save File dialog with the specified filter and default file name asynchronously.
    /// </summary>
    /// <param name="filter">
    /// A filter string in the format "Description|*.ext;*.ext|..." to restrict file types.
    /// </param>
    /// <param name="fileName">
    /// The default file name to display in the Save File dialog.
    /// </param>
    /// <returns>
    /// A <see cref="FileDialogResult"/> containing the dialog result and selected file path.
    /// </returns>
    public Task<FileDialogResult> SaveFileDialogAsync(string filter, string fileName)
    {
        var saveFileDialog = new SaveFileDialog
        {
            Filter = filter,
            FileName = fileName
        };

        bool? result = saveFileDialog.ShowDialog();
        return Task.FromResult(new FileDialogResult(result, saveFileDialog.FileName));
    }

    #endregion
}