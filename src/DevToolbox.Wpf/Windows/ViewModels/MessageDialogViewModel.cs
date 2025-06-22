using DevToolbox.Core.Windows;
using DevToolbox.Wpf.Controls;

namespace DevToolbox.Wpf.Windows.ViewModels;

/// <summary>
/// ViewModel for displaying a message dialog with a title, message content, and optional image.
/// </summary>
public partial class MessageDialogViewModel : DialogViewModel
{
    #region Properties

    /// <summary>
    /// Gets the title text displayed in the dialog header.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the message content displayed in the dialog body.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the image code to display alongside the message, or <c>null</c> if none is specified.
    /// </summary>
    public string? ImageCode { get; }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageDialogViewModel"/> class.
    /// </summary>
    /// <param name="title">The title text to display.</param>
    /// <param name="message">The message content to display.</param>
    /// <param name="dialogImage">The an image to display, or <c>null</c>.</param>
    public MessageDialogViewModel(string title, string message, DialogImage dialogImage)
    {
        Title = title;
        Message = message;

        ImageCode = dialogImage switch
        {
            DialogImage.Info => Icons.Info,
            DialogImage.Warning => Icons.Warning,
            DialogImage.Error => Icons.ErrorBadge,
            _ => null,
        };
    }
}