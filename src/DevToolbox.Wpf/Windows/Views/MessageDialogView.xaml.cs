using System;
using DevToolbox.Wpf.Windows.ViewModels;

namespace DevToolbox.Wpf.Windows.Views;

/// <summary>
/// A view for displaying informational dialogs with a title, message, and optional image.
/// </summary>
public partial class MessageDialogView : BaseDialogView, IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageDialogView"/> class
    /// with the specified title, message, and optional image source.
    /// </summary>
    /// <param name="title">
    /// The title text to display in the dialog header.
    /// </param>
    /// <param name="message">
    /// The message content to display in the dialog body.
    /// </param>
    /// <param name="imageSource">
    /// An optional URI of an image to display alongside the message,
    /// or <c>null</c> if no image is provided.
    /// </param>
    public MessageDialogView(
        string title,
        string message,
        Uri? imageSource)
        : base(new MessageDialogViewModel(title, message, imageSource))
    {
        InitializeComponent();
    }
}