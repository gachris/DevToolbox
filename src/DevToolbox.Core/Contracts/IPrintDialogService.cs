using System.Threading.Tasks;

namespace DevToolbox.Core.Contracts;

/// <summary>
/// Provides a method to display a print dialog for specified content.
/// </summary>
public interface IPrintDialogService
{
    /// <summary>
    /// Shows a print dialog asynchronously for the given content.
    /// </summary>
    /// <param name="content">
    /// The object representing the content to print (e.g., a document or UI element).
    /// </param>
    /// <returns>
    /// A task that completes with a nullable boolean indicating the print result:
    /// <c>true</c> if the user confirmed printing, <c>false</c> if the user canceled,
    /// or <c>null</c> if the dialog was closed without a decision.
    /// </returns>
    Task<bool?> PrintAsync(object content);
}