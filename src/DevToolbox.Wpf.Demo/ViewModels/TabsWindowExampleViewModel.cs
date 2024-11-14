using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DevToolbox.Wpf.Demo.ViewModels;

public class TabsWindowExampleViewModel : ObservableObject
{
    public ObservableCollection<TabItemContent> Items { get; }

    public TabsWindowExampleViewModel()
    {
        Items =
        [
            new TabItemContent() { Header = "Header 1", Content = "Content 1" },
            new TabItemContent() { Header = "Header 2", Content = "Content 2" },
            new TabItemContent() { Header = "Header 3", Content = "Content 3" }
        ];
    }
}