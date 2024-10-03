using System.Threading.Tasks;

namespace DevToolbox.Wpf.Demo;

public interface ILocalSettingsService
{
    Task<T?> ReadSettingAsync<T>(string key);

    Task SaveSettingAsync<T>(string key, T value);
}
