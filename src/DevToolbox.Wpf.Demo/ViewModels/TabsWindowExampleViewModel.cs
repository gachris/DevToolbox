using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DevToolbox.Wpf.Demo.ViewModels;

public class TabsWindowExampleViewModel : ObservableObject
{
    public ObservableCollection<TabItemContent> Items { get; } = [];
}