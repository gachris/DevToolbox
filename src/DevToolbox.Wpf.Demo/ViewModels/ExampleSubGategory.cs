using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevToolbox.Wpf.Demo.JsonHelpers;

namespace DevToolbox.Wpf.Demo.ViewModels;

public partial class ExampleSubGategory : ObservableObject
{
    private string? _icon;

    [ObservableProperty]
    private string? _header;


    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private string? _namespace;

    [JsonConverter(typeof(IconConverter))]
    public string? Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }

    public ObservableCollection<Example>? Examples { get; set; }

    [RelayCommand]
    private void Select()
    {
        IsSelected = true;
    }
}