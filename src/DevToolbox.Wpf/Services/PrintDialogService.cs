using System.Threading.Tasks;
using System.Windows.Controls;
using DevToolbox.Core.Contracts;
using DevToolbox.Core.Properties;
using DevToolbox.Wpf.Windows;

namespace DevToolbox.Wpf.Services;

/// <summary>
/// Implements <see cref="IPrintDialogService"/> to display a print dialog
/// for WPF content using a custom <see cref="DocumentPrintWindow"/>.
/// </summary>
public class PrintDialogService : IPrintDialogService
{
    #region IPrintDialogService Implementation

    /// <summary>
    /// Displays a modal print dialog window for the specified content.
    /// </summary>
    /// <param name="content">
    /// The view model or data context to print, which will be wrapped
    /// in a <see cref="ContentControl"/> and passed to the print window.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing a nullable boolean:
    /// <c>true</c> if the user confirmed the print operation,
    /// <c>false</c> if canceled, or <c>null</c> if closed without result.
    /// </returns>
    public Task<bool?> PrintAsync(object content)
    {
        // Wrap the content in a ContentControl for printing
        var contentControl = new ContentControl
        {
            DataContext = content
        };

        // Create and configure the print window
        var printWindow = new DocumentPrintWindow(contentControl, Resources.Print)
        {
            Title = Resources.Print,
            ShowInTaskbar = false,
            Owner = System.Windows.Application.Current.MainWindow
        };

        // Show the dialog modally and return the result
        bool? result = printWindow.ShowDialog();
        return Task.FromResult(result);
    }

    #endregion
}