using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace DevToolbox.Wpf.Demo;

public class LocalSettingsService : ILocalSettingsService
{
    #region Fields/Consts

    private const string _defaultApplicationDataFolder = "iCloud/ApplicationData";
    private const string _defaultLocalSettingsFile = "LocalSettings.json";

    private readonly IFileService _fileService;
    private readonly LocalSettingsOptions _options;

    private readonly string _localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly string _applicationDataFolder;
    private readonly string _localsettingsFile;

    private Dictionary<string, object> _settings;

    private bool _isInitialized;

    #endregion

    public LocalSettingsService(IFileService fileService, IOptions<LocalSettingsOptions> options)
    {
        _fileService = fileService;
        _options = options.Value;

        _applicationDataFolder = Path.Combine(_localApplicationData, _options.ApplicationDataFolder ?? _defaultApplicationDataFolder);
        _localsettingsFile = _options.LocalSettingsFile ?? _defaultLocalSettingsFile;

        _settings = [];
    }

    #region ILocalSettingsService Implementation

    public async Task<T?> ReadSettingAsync<T>(string key)
    {
        await InitializeAsync();

        return _settings != null && _settings.TryGetValue(key, out var obj) ? await Json.ToObjectAsync<T>((string)obj) : default;
    }

    public async Task SaveSettingAsync<T>(string key, T value)
    {
        await InitializeAsync();

        _settings[key] = await Json.StringifyAsync(value);

        await Task.Run(() => _fileService.Save(_applicationDataFolder, _localsettingsFile, _settings));
    }

    #endregion

    #region Methods

    private async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            _settings = await Task.Run(() => _fileService.Read<Dictionary<string, object>>(_applicationDataFolder, _localsettingsFile)) ?? [];

            _isInitialized = true;
        }
    }

    #endregion
}
