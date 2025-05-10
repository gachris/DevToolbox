using System;
using DevToolbox.Wpf.Windows.ViewModels;

namespace DevToolbox.Wpf.Windows.Views;

/// <summary>
/// A view for displaying error dialogs with a message and optional stack trace.
/// </summary>
public partial class ErrorDialogView : BaseDialogView, IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorDialogView"/> class
    /// with the specified error message and optional stack trace.
    /// </summary>
    /// <param name="message">
    /// The error message to display in the dialog.
    /// </param>
    /// <param name="stackTrace">
    /// The stack trace associated with the error, or <c>null</c> if not available.
    /// </param>
    public ErrorDialogView(string message, string? stackTrace)
        : base(new ErrorDialogViewModel(message, stackTrace))
    {
        InitializeComponent();
    }
}