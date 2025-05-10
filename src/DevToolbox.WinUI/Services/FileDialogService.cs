using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevToolbox.Core.Contracts;
using DevToolbox.Core.Data;
using DevToolbox.WinUI.Contracts;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace DevToolbox.WinUI.Services;

/// <summary>
/// Provides file open and save dialog functionality using WinUI's storage pickers,
/// applying the application's main window as the parent.
/// </summary>
public class FileDialogService : IFileDialogService
{
    #region Fields/Consts

    private readonly IAppWindowService _appWindowService;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDialogService"/> class.
    /// </summary>
    /// <param name="appWindowService">
    /// The service that provides the main application window for dialog parenting.
    /// </param>
    public FileDialogService(IAppWindowService appWindowService)
    {
        _appWindowService = appWindowService;
    }

    #region IPrintDialogService IFileDialogService

    /// <inheritdoc/>
    public async Task<FileDialogResult> OpenFileDialogAsync(string filter)
    {
        var filePicker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };

        var hwnd = WindowNative.GetWindowHandle(_appWindowService.MainWindow);
        InitializeWithWindow.Initialize(filePicker, hwnd);

        var extensions = filter.Split('|');
        for (var i = 1; i < extensions.Length; i += 2)
        {
            var extensionGroup = extensions[i].Split(';');
            foreach (var extension in extensionGroup)
            {
                // Ensure the extension is not empty (to handle cases like trailing semicolons)
                if (!string.IsNullOrWhiteSpace(extension))
                {
                    // Ensure the extension starts with a '.' and remove any wildcards
                    var cleanedExtension = extension.Replace("*", "").Trim();
                    if (!cleanedExtension.StartsWith("."))
                    {
                        cleanedExtension = "." + cleanedExtension;
                    }
                    filePicker.FileTypeFilter.Add(cleanedExtension);
                }
            }
        }

        var file = await filePicker.PickSingleFileAsync();

        if (file != null)
        {
            return new FileDialogResult(true, file.Path);
        }
        else
        {
            return new FileDialogResult(false, string.Empty);
        }
    }

    /// <inheritdoc/>
    public async Task<FileDialogResult> SaveFileDialogAsync(string filter, string fileName)
    {
        var filePicker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = fileName
        };

        var hwnd = WindowNative.GetWindowHandle(_appWindowService.MainWindow);
        InitializeWithWindow.Initialize(filePicker, hwnd);

        var extensions = filter.Split('|');
        for (var i = 0; i < extensions.Length - 1; i += 2)
        {
            var description = extensions[i];
            var extensionGroup = extensions[i + 1].Split(';');
            var cleanedExtensions = new List<string>();

            foreach (var extension in extensionGroup)
            {
                var cleanedExtension = extension.Replace("*", "").Trim();
                if (!cleanedExtension.StartsWith("."))
                {
                    cleanedExtension = "." + cleanedExtension;
                }
                cleanedExtensions.Add(cleanedExtension);
            }

            if (cleanedExtensions.Count > 0)
            {
                filePicker.FileTypeChoices.Add(description, cleanedExtensions);
            }
        }

        var file = await filePicker.PickSaveFileAsync();

        if (file != null)
        {
            return new FileDialogResult(true, file.Path);
        }
        else
        {
            return new FileDialogResult(false, string.Empty);
        }
    }

    #endregion
}