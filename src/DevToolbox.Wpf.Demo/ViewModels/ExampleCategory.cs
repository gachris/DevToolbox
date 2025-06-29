using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DevToolbox.Wpf.Demo.ViewModels;

public partial class ExampleCategory : ObservableObject
{
    [ObservableProperty]
    private string? _header;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isExpanded;

    public ObservableCollection<ExampleSubCategory>? SubCategories { get; set; }
}
