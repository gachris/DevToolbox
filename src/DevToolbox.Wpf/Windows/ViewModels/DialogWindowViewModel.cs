﻿using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevToolbox.Core.Windows;
using System.Linq;
using DevToolbox.Wpf.Windows.Commands;
using DevToolbox.Wpf.Windows.Views;
using DevToolbox.Core.Properties;

namespace DevToolbox.Wpf.Windows.ViewModels;

/// <summary>
/// ViewModel for <see cref="DialogWindow"/>, managing its content, buttons, and lifecycle.
/// </summary>
public partial class DialogWindowViewModel : ObservableObject, IDisposable
{
    #region Fields/Consts

    private readonly List<PluginButtonViewModel> _leftButtons = [];
    private readonly List<PluginButtonViewModel> _rightButtons = [];
    private readonly DialogWindow _window;
    private bool _disposedValue; 
    private bool _waitingAnimationBusy;
    private string? _waitingAnimationTitle;
    private string? _waitingAnimationMessage;

    /// <summary>
    /// Gets or sets the title text displayed during the waiting animation.
    /// </summary>
    public string? WaitingAnimationTitle
    {
        get => _waitingAnimationTitle;
        set => SetProperty(ref _waitingAnimationTitle, value);
    }

    /// <summary>
    /// Gets or sets the message text displayed below the title during the waiting animation.
    /// </summary>
    public string? WaitingAnimationMessage
    {
        get => _waitingAnimationMessage;
        set => SetProperty(ref _waitingAnimationMessage, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the waiting animation is active.
    /// </summary>
    public bool WaitingAnimationBusy
    {
        get => _waitingAnimationBusy;
        set => SetProperty(ref _waitingAnimationBusy, value);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether the dialog footer (buttons) should be visible.
    /// </summary>
    public bool IsFooterVisible { get; }

    /// <summary>
    /// Gets the <see cref="BaseDialogView"/> hosted within this dialog.
    /// </summary>
    public BaseDialogView View { get; }

    /// <summary>
    /// Gets the collection of plugin buttons displayed on the left side.
    /// </summary>
    public ReadOnlyCollection<PluginButtonViewModel> LeftButtons { get; }

    /// <summary>
    /// Gets the collection of plugin buttons displayed on the right side.
    /// </summary>
    public ReadOnlyCollection<PluginButtonViewModel> RightButtons { get; }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogWindowViewModel"/> class.
    /// </summary>
    /// <param name="window">The dialog window instance to control.</param>
    /// <param name="view">The view to display inside the dialog.</param>
    /// <param name="dialogOptions">Options dictating which plugin buttons to display.</param>
    public DialogWindowViewModel(DialogWindow window, BaseDialogView view, DialogOptions dialogOptions)
    {
        _window = window;
        View = view;

        LeftButtons = new ReadOnlyCollection<PluginButtonViewModel>(_leftButtons);
        RightButtons = new ReadOnlyCollection<PluginButtonViewModel>(_rightButtons);

        SetupButtons(dialogOptions);

        IsFooterVisible = LeftButtons.Count != 0 || RightButtons.Count != 0;
    }

    #region Methods

    /// <summary>
    /// Configures plugin buttons based on the specified <paramref name="dialogOptions"/>.
    /// </summary>
    /// <param name="dialogOptions">The options containing plugin button definitions.</param>
    private void SetupButtons(DialogOptions dialogOptions)
    {
        if (dialogOptions.PluginButtons?.Any() ?? false)
        {
            var leftButtons = dialogOptions.PluginButtons!
                  .Where(x => x.ButtonPosition == PluginButtonPosition.Left)
                  .OrderBy(x => x.ButtonOrder)
                  .ToList();
            var rightButtons = dialogOptions.PluginButtons!
                .Where(x => x.ButtonPosition == PluginButtonPosition.Right)
                .OrderBy(x => x.ButtonOrder)
                .ToList();

            _leftButtons.AddRange(leftButtons.Select(GetButton));
            _rightButtons.AddRange(rightButtons.Select(GetButton));
        }
    }

    /// <summary>
    /// Creates a <see cref="PluginButtonViewModel"/> for the given <see cref="PluginButton"/>.
    /// </summary>
    /// <param name="pluginButton">The plugin button definition.</param>
    /// <returns>A configured <see cref="PluginButtonViewModel"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="pluginButton"/> has an unsupported <see cref="PluginButtonType"/>.
    /// </exception>
    private static PluginButtonViewModel GetButton(PluginButton pluginButton)
    {
        var content = pluginButton.Content is string text && !string.IsNullOrEmpty(text)
            ? pluginButton.Content
            : pluginButton.ButtonType switch
            {
                PluginButtonType.Yes => Resources.Yes,
                PluginButtonType.No => Resources.No,
                PluginButtonType.OK => Resources.OK,
                PluginButtonType.Cancel => Resources.Cancel,
                _ => throw new ArgumentOutOfRangeException(nameof(pluginButton.ButtonType)),
            };

        var command = pluginButton.ButtonType switch
        {
            PluginButtonType.Yes => DialogWindowCommands.Yes,
            PluginButtonType.No => DialogWindowCommands.No,
            PluginButtonType.OK => DialogWindowCommands.OK,
            PluginButtonType.Cancel => DialogWindowCommands.Cancel,
            _ => throw new ArgumentOutOfRangeException(nameof(pluginButton.ButtonType)),
        };

        var button = new PluginButtonViewModel(content, command, pluginButton.IsDefault, pluginButton.IsCancel);

        return button;
    }

    #endregion

    #region Relay Commands

    [RelayCommand]
    private void Yes(object e)
    {
        _window.ModalResult = ModalResult.Yes;
    }

    [RelayCommand]
    private void No(object e)
    {
        _window.ModalResult = ModalResult.No;
    }

    /// <summary>
    /// Command handler for the Cancel action. Sets the dialog result to Cancel.
    /// </summary>
    /// <param name="e">The command parameter (unused).</param>
    [RelayCommand]
    private void OK(object e)
    {
        _window.ModalResult = ModalResult.OK;
    }

    /// <summary>
    /// Command handler for the OK action. Sets the dialog result to OK.
    /// </summary>
    /// <param name="e">The command parameter (unused).</param>
    [RelayCommand]
    private void Cancel(object e)
    {
        _window.ModalResult = ModalResult.Cancel;
    }

    #endregion

    #region IDisposable Implementation

    /// <summary>
    /// Releases managed and unmanaged resources used by this instance.
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> to release both managed and unmanaged resources;
    /// <c>false</c> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _leftButtons.Clear();
                _rightButtons.Clear();

                if (View is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _disposedValue = true;
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    // /// <summary>
    // /// Finalizer to ensure unmanaged resources are released.
    // /// </summary>
    // ~DialogWindowViewModel()
    // {
    //     Dispose(disposing: false);
    // }

    #endregion
}
