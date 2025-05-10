using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using DevToolbox.Core.Windows;
using DevToolbox.Core.Properties;
using DevToolbox.Wpf.Windows;
using DevToolbox.Wpf.Windows.ViewModels;
using DevToolbox.Wpf.Windows.Views;
using DevToolbox.Core.Contracts;

namespace DevToolbox.Wpf.Services;

/// <summary>
/// Provides functionality to show modal dialog windows and track their association with view models.
/// </summary>
public class DialogService : IDialogService
{
    #region Fields/Consts

    /// <summary>
    /// Holds the mapping between dialog view models and their corresponding dialog windows.
    /// </summary>
    private readonly Dictionary<object, Window> _dialogs = [];

    #endregion

    #region IDialogService Implementation

    /// <inheritdoc/>
    public Task<bool?> ShowDialogAsync(object dialogWindow)
    {
        var dialog = (Window)dialogWindow;
        var viewModel = dialog.DataContext;

        if (viewModel is not null)
        {
            _dialogs.Add(viewModel, dialog);
        }

        dialog.Owner ??= (Application.Current.MainWindow.IsVisible ? Application.Current.MainWindow : null);
        dialog.WindowStartupLocation = dialog.Owner is null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner;

        var modalResult = dialog.ShowDialog();

        if (viewModel is not null)
        {
            _dialogs.Remove(viewModel);
        }

        return Task.FromResult(modalResult);
    }

    /// <inheritdoc/>
    public Task<ModalResult> ShowDialogAsync(
        object? owner,
        object content,
        IDialogOptions options)
    {
        if (content is BaseDialogView dialogView)
        {
            var viewModel = dialogView.DataContext;

            using var dialog = new DialogWindow(dialogView, (DialogOptions)options);

            if (viewModel is not null)
            {
                _dialogs.Add(viewModel, dialog);
            }

            dialog.Owner = (Window?)owner ?? (Application.Current.MainWindow.IsVisible ? Application.Current.MainWindow : null);
            dialog.WindowStartupLocation = dialog.Owner is null
                ? WindowStartupLocation.CenterScreen 
                : WindowStartupLocation.CenterOwner;

            var modalResult = dialog.ShowModal();

            if (viewModel is not null)
            {
                _dialogs.Remove(viewModel);
            }

            return Task.FromResult(modalResult);
        }
        else if (content is DialogViewModel dialogViewModel)
        {
            using var dialog = new DialogWindow(new DialogView(dialogViewModel), (DialogOptions)options);

            _dialogs.Add(content, dialog);

            dialog.Owner = (Window?)owner ?? (Application.Current.MainWindow.IsVisible ? Application.Current.MainWindow : null);
            dialog.WindowStartupLocation = dialog.Owner is null
                ? WindowStartupLocation.CenterScreen
                : WindowStartupLocation.CenterOwner;

            var modalResult = dialog.ShowModal();

            _dialogs.Remove(dialogViewModel);

            return Task.FromResult(modalResult);
        }
        else
        {
            throw new InvalidOperationException("The content cannot be null.");
        }
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
        var imageSource = dialogImage switch
        {
            DialogImage.Info => "pack://application:,,,/DevToolbox.Wpf;component/Assets/info-100.png",
            DialogImage.Warning => "pack://application:,,,/DevToolbox.Wpf;component/Assets/warning-100.png",
            DialogImage.Error => "pack://application:,,,/DevToolbox.Wpf;component/Assets/error-100.png",
            _ => null,
        };

        var dialogOptions = new DialogOptions()
        {
            Width = 580,
            MinHeight = 100,
            SizeToContent = SizeToContent.Height,
            IsTitleBarVisible = false
        };

        if (pluginButtons.HasFlag(PluginButtons.Yes))
        {
            dialogOptions.PluginButtons.Add(new PluginButton()
            {
                ButtonOrder = 10,
                ButtonPosition = PluginButtonPosition.Right,
                ButtonType = PluginButtonType.Yes,
                Content = yesButtonText ?? Resources.Yes
            });
        }

        if (pluginButtons.HasFlag(PluginButtons.No))
        {
            dialogOptions.PluginButtons.Add(new PluginButton()
            {
                ButtonOrder = 20,
                ButtonPosition = PluginButtonPosition.Right,
                ButtonType = PluginButtonType.No,
                Content = noButtonText ?? Resources.No
            });
        }

        if (pluginButtons.HasFlag(PluginButtons.OK))
        {
            dialogOptions.PluginButtons.Add(new PluginButton()
            {
                IsDefault = !pluginButtons.HasFlag(PluginButtons.Cancel),
                ButtonOrder = 30,
                ButtonPosition = PluginButtonPosition.Right,
                ButtonType = PluginButtonType.OK,
                Content = okButtonText ?? Resources.OK
            });
        }

        if (pluginButtons.HasFlag(PluginButtons.Cancel))
        {
            dialogOptions.PluginButtons.Add(new PluginButton()
            {
                IsCancel = true,
                IsDefault = true,
                ButtonOrder = 40,
                ButtonPosition = PluginButtonPosition.Right,
                ButtonType = PluginButtonType.Cancel,
                Content = cancelButtonText ?? Resources.Cancel
            });
        }

        var view = new MessageDialogView(
            title,
            message,
            imageSource is not null ? new Uri(imageSource, UriKind.RelativeOrAbsolute) : null);

        var modalResult = await ShowDialogAsync((Window?)owner, view, dialogOptions);

        return modalResult;
    }

    /// <inheritdoc/>
    public async Task ShowErrorAsync(Exception exception)
    {
        var options = new DialogOptions()
        {
            Width = 458,
            MinHeight = 185,
            MaxHeight = 560,
            SizeToContent = SizeToContent.Height,
            WindowTitle = Resources.Unhandled_exception,
            PluginButtons =
            {
                new()
                {
                    IsDefault = true,
                    ButtonOrder = 10,
                    ButtonPosition = PluginButtonPosition.Right,
                    ButtonType = PluginButtonType.OK,
                }
            }
        };

        var view = new ErrorDialogView(exception.Message, exception.StackTrace);
        await ShowDialogAsync(Application.Current.MainWindow, view, options);
    }

    /// <inheritdoc/>
    public bool TryGetDialogByContent(object content, out object? dialogWindow)
    {
        var result = _dialogs.TryGetValue(content, out Window? window);
        dialogWindow = window;
        return result;
    }

    /// <inheritdoc/>
    public bool TryGetDialogByContentType(Type contentType, out object dialogWindow)
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
