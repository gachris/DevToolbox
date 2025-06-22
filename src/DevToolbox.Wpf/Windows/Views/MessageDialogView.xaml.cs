using System;
using DevToolbox.Core.Windows;
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
    /// <param name="dialogImage">
    /// An image to display alongside the message,
    /// or <c>null</c> if no image is provided.
    /// </param>
    public MessageDialogView(
        string title,
        string message,
        DialogImage dialogImage)
        : base(new MessageDialogViewModel(title, message, dialogImage))
    {
        InitializeComponent();
    }
}