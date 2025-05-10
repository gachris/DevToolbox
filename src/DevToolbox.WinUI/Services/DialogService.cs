using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevToolbox.Core.Contracts;
using DevToolbox.Core.Properties;
using DevToolbox.Core.Windows;
using DevToolbox.WinUI.Contracts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DevToolbox.WinUI.Services;

/// <summary>
/// Provides functionality to show modal dialog windows and track their association with view models.
/// </summary>
public class DialogService : IDialogService
{
    #region Fields/Consts

    private readonly Dictionary<object, ContentDialog> _dialogs = [];
    private readonly IAppWindowService _appWindowService;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogService"/> class.
    /// </summary>
    /// <param name="appWindowService">
    /// The service providing access to the main application window for dialog parenting.
    /// </param>
    public DialogService(IAppWindowService appWindowService)
    {
        _appWindowService = appWindowService;
    }

    #region IDialogService Implementation

    /// <inheritdoc/>
    public Task<bool?> ShowDialogAsync(object dialogWindow)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<ModalResult> ShowDialogAsync(object? owner, object content, IDialogOptions options)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public async Task<ModalResult> ShowDialogAsync(
        object? owner,
        string title,
        string message,
        PluginButtons pluginButtons = PluginButtons.OK | PluginButtons.Cancel,
        string? okButtonText = null,
        string? cancelButtonText = null,
        string? yesButtonText = null,
        string? noButtonText = null,
        DialogImage dialogImage = DialogImage.None)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 16,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 16)
                    }
                }
            },
            PrimaryButtonText = okButtonText ?? Resources.OK,
            SecondaryButtonText = cancelButtonText ?? Resources.Cancel,
            CloseButtonText = pluginButtons == PluginButtons.Cancel ? Resources.Cancel : null,
            XamlRoot = _appWindowService.MainWindow.Content.XamlRoot
        };

        if (pluginButtons.HasFlag(PluginButtons.OK) && pluginButtons.HasFlag(PluginButtons.Cancel))
        {
            dialog.PrimaryButtonText = okButtonText ?? Resources.OK;
            dialog.SecondaryButtonText = cancelButtonText ?? Resources.Cancel;
        }
        else if (pluginButtons == PluginButtons.OK)
        {
            dialog.PrimaryButtonText = okButtonText ?? Resources.OK;
        }
        else if (pluginButtons == PluginButtons.Cancel)
        {
            dialog.CloseButtonText = cancelButtonText ?? Resources.Cancel;
        }

        // Show dialog and handle result
        var result = await dialog.ShowAsync();

        var modalResult = result switch
        {
            ContentDialogResult.Primary => ModalResult.OK,
            ContentDialogResult.Secondary => ModalResult.Cancel,
            _ => ModalResult.Cancel
        };

        return modalResult;
    }

    /// <inheritdoc/>
    public async Task ShowErrorAsync(Exception exception)
    {
        var dialog = new ContentDialog
        {
            Title = Resources.Unhandled_exception,
            Content = new ScrollViewer
            {
                Content = new TextBlock
                {
                    Text = $"{exception.Message}\n\n{exception.StackTrace}",
                    TextWrapping = TextWrapping.Wrap
                },
                MaxHeight = 560
            },
            PrimaryButtonText = Resources.OK,
            XamlRoot = _appWindowService.MainWindow.Content.XamlRoot
        };

        await dialog.ShowAsync();
    }

    /// <inheritdoc/>
    public bool TryGetDialogByContent(object content, out object? dialogWindow)
    {
        var result = _dialogs.TryGetValue(content, out ContentDialog? window);
        dialogWindow = window;
        return result;
    }

    /// <inheritdoc/>
    public bool TryGetDialogByContentType(Type contentType, out object? dialogWindow)
    {
        foreach (var kvp in _dialogs)
        {
            if (kvp.Key.GetType() == contentType)
            {
                dialogWindow = kvp.Value;
                return true;
            }
        }

        dialogWindow = null!;
        return false;
    }

    #endregion
}
