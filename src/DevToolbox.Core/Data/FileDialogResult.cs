namespace DevToolbox.Core.Data;

/// <summary>
/// Represents the outcome of a file dialog operation, including the user's choice and the selected file path.
/// </summary>
public class FileDialogResult
{
    /// <summary>
    /// Gets a value indicating whether the dialog was confirmed (<c>true</c>), canceled (<c>false</c>),
    /// or closed without a decision (<c>null</c>).
    /// </summary>
    public bool? Result { get; }

    /// <summary>
    /// Gets the full path of the file selected by the user, or the default file name if none was selected.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDialogResult"/> class with the specified result and file name.
    /// </summary>
    /// <param name="result">
    /// The dialog result: <c>true</c> if confirmed, <c>false</c> if canceled, or <c>null</c> if closed without a decision.
    /// </param>
    /// <param name="fileName">
    /// The file path selected by the user, or a default file name.
    /// </param>
    public FileDialogResult(bool? result, string fileName)
    {
        Result = result;
        FileName = fileName;
    }
}