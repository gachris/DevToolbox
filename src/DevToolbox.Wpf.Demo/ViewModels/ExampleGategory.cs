using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DevToolbox.Wpf.Demo.ViewModels;

public partial class ExampleGategory : ObservableObject
{
    [ObservableProperty]
    private string? _header;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isExpanded;

    public ObservableCollection<ExampleSubGategory>? SubCategories { get; set; }
}
