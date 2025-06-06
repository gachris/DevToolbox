﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DevToolbox.Core.Contracts;
using DevToolbox.Core.Extensions;

namespace DevToolbox.Core.Services;

/// <summary>
/// Provides functionality for reading and persisting local application settings to a JSON file.
/// Uses <see cref="IFileService"/> for file I/O and <see cref="LocalSettingsOptions"/> for configuration.
/// </summary>
public class LocalSettingsService : ILocalSettingsService
{
    #region Fields/Consts

    private const string _defaultApplicationDataFolder = $"DevToolbox/ApplicationData";
    private const string _defaultLocalSettingsFile = "LocalSettings.json";

    private readonly IFileService _fileService;
    private readonly LocalSettingsOptions? _options;

    private readonly string _localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly string _applicationDataFolder;
    private readonly string _localSettingsFile;

    private Dictionary<string, object> _settings;

    private bool _isInitialized;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalSettingsService"/> class.
    /// </summary>
    /// <param name="fileService">The file service used for reading and writing files.</param>
    /// <param name="options">The settings options specifying folders and filenames.</param>
    public LocalSettingsService(IFileService fileService, LocalSettingsOptions? options = null)
    {
        _fileService = fileService;
        _options = options;

        _applicationDataFolder = Path.Combine(_localApplicationData, _options?.ApplicationDataFolder ?? _defaultApplicationDataFolder);
        _localSettingsFile = _options?.LocalSettingsFile ?? _defaultLocalSettingsFile;

        _settings = [];
    }

    #region Methods

    /// <inheritdoc/>
    public async Task<T?> ReadSettingAsync<T>(string key)
    {
        await InitializeAsync();

        return _settings != null && _settings.TryGetValue(key, out var obj) ? await Json.ToObjectAsync<T>((string)obj) : default;
    }

    /// <inheritdoc/>
    public async Task SaveSettingAsync<T>(string key, T value)
    {
        await InitializeAsync();

        _settings[key] = await Json.StringifyAsync(value);

        await Task.Run(() => _fileService.Save(_applicationDataFolder, _localSettingsFile, _settings));
    }

    /// <summary>
    /// Loads settings from the settings file into memory if not already initialized.
    /// </summary>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    private async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            _settings = await Task.Run(() => _fileService.Read<Dictionary<string, object>>(_applicationDataFolder, _localSettingsFile)) ?? [];

            _isInitialized = true;
        }
    }

    #endregion
}
