using System;
using System.Threading.Tasks;
using DevToolbox.Core.Windows;

namespace DevToolbox.Core.Contracts;

/// <summary>
/// Provides methods for displaying and managing modal dialog windows.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows the specified dialog window as a modal dialog asynchronously.
    /// </summary>
    /// <param name="dialogWindow">
    /// The dialog window instance to display.
    /// </param>
    /// <returns>
    /// A task that completes with a nullable boolean indicating the user's response:
    /// <c>true</c> if OK was selected, <c>false</c> if Cancel was selected,
    /// or <c>null</c> if the dialog was closed without a selection.
    /// </returns>
    Task<bool?> ShowDialogAsync(object dialogWindow);

    /// <summary>
    /// Shows a modal dialog hosting the specified content asynchronously.
    /// </summary>
    /// <param name="owner">
    /// The window that owns the dialog. If <c>null</c>, the main application window
    /// is used if visible; otherwise, the dialog is centered on the screen.
    /// </param>
    /// <param name="content">
    /// The content to display within the dialog.
    /// </param>
    /// <param name="options">
    /// Options that control the dialog's appearance and behavior.
    /// </param>
    /// <returns>
    /// A task that completes with a <see cref="ModalResult"/> indicating how the dialog was closed.
    /// </returns>
    Task<ModalResult> ShowDialogAsync(
        object? owner,
        object content,
        IDialogOptions options);

    /// <summary>
    /// Shows a message dialog asynchronously with customizable buttons and icon.
    /// </summary>
    /// <param name="owner">
    /// The window that owns the dialog. If <c>null</c>, default window selection is applied.
    /// </param>
    /// <param name="title">
    /// The text to display in the dialog's title bar.
    /// </param>
    /// <param name="message">
    /// The message content to display in the dialog.
    /// </param>
    /// <param name="pluginButtons">
    /// Flags specifying which buttons to include (e.g., OK, Cancel, Yes, No).
    /// </param>
    /// <param name="okButtonText">
    /// Custom text for the OK button, or <c>null</c> to use the default text.
    /// </param>
    /// <param name="cancelButtonText">
    /// Custom text for the Cancel button, or <c>null</c> to use the default text.
    /// </param>
    /// <param name="yesButtonText">
    /// Custom text for the Yes button, or <c>null</c> to use the default text.
    /// </param>
    /// <param name="noButtonText">
    /// Custom text for the No button, or <c>null</c> to use the default text.
    /// </param>
    /// <param name="dialogImage">
    /// The icon to display in the dialog.
    /// </param>
    /// <returns>
    /// A task that completes with a <see cref="ModalResult"/> indicating the user's selection.
    /// </returns>
    Task<ModalResult> ShowDialogAsync(
        object? owner,
        string title,
        string message,
        PluginButtons pluginButtons = PluginButtons.OK | PluginButtons.Cancel,
        string? okButtonText = null,
        string? cancelButtonText = null,
        string? yesButtonText = null,
        string? noButtonText = null,
        DialogImage dialogImage = DialogImage.None);

    /// <summary>
    /// Shows an error dialog asynchronously for the specified exception.
    /// </summary>
    /// <param name="exception">
    /// The exception whose details are displayed in the error dialog.
    /// </param>
    /// <returns>
    /// A task that completes when the error dialog is closed.
    /// </returns>
    Task ShowErrorAsync(Exception exception);

    /// <summary>
    /// Attempts to retrieve the open dialog window associated with the given content.
    /// </summary>
    /// <param name="content">
    /// The content whose dialog window is being sought.
    /// </param>
    /// <param name="dialogWindow">
    /// When this method returns, contains the dialog window if found; otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if a matching dialog window was found; otherwise, <c>false</c>.
    /// </returns>
    bool TryGetDialogByContent(object content, out object? dialogWindow);

    /// <summary>
    /// Attempts to retrieve an open dialog window by the view model's type.
    /// </summary>
    /// <param name="viewModelType">
    /// The type of the view model whose dialog window is being sought.
    /// </param>
    /// <param name="dialogWindow">
    /// When this method returns, contains the dialog window if found; otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if a matching dialog window was found; otherwise, <c>false</c>.
    /// </returns>
    bool TryGetDialogByContentType(Type viewModelType, out object? dialogWindow);
}