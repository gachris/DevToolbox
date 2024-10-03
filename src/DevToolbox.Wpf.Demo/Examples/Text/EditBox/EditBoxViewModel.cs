using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevToolbox.Wpf.Demo.ViewModels;

public partial class EditBoxViewModel : ObservableObject
{
    [ObservableProperty]
    private string _displayText;

    [ObservableProperty]
    private string _text;

    [ObservableProperty]
    private bool _isEditing;

    public EditBoxViewModel()
    {
        _displayText = "Display Text";
        _text = "Text";
    }

    [RelayCommand]
    private void BeginEdit()
    {
    }

    [RelayCommand]
    private void EndEdit()
    {
    }

    [RelayCommand]
    private void CancelEdit()
    {
    }
}
