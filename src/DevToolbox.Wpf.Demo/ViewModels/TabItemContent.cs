using CommunityToolkit.Mvvm.ComponentModel;

namespace DevToolbox.Wpf.Demo.ViewModels;

public class TabItemContent : ObservableObject
{
    public object Header { get; set; }

    public object? Content { get; set; }

    public bool IsSelected { get; set; }

    public TabItemContent()
    {
        Header = "New Tab";
    }
}
