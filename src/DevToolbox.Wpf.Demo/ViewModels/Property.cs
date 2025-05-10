using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DevToolbox.Wpf.Demo.Json;

namespace DevToolbox.Wpf.Demo.ViewModels;

public partial class Property : ObservableObject
{
    private ControlType _controlType;

    [ObservableProperty]
    private string? _namespace;

    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private string? _displayName;

    [ObservableProperty]
    private string? _type;

    [JsonConverter(typeof(ControlTypeConverter))]
    public ControlType ControlType
    {
        get => _controlType;
        set => SetProperty(ref _controlType, value);
    }

    [ObservableProperty]
    private object? _value;
}
