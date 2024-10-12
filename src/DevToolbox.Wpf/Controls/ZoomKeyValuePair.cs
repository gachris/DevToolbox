using DevToolbox.Wpf.Mvvm;

namespace DevToolbox.Wpf;

internal class ZoomKeyValuePair : NotifyPropertyChanged
{
    public string? _key;
    public double _value;

    public string? Key
    {
        get => _key;
        set => SetProperty(ref _key, value, nameof(Key));
    }

    public double Value
    {
        get => _value;
        set => SetProperty(ref _value, value, nameof(Value));
    }

    public ZoomKeyValuePair()
    {
    }

    public ZoomKeyValuePair(string key, double value)
    {
        Key = key;
        Value = value;
    }
}