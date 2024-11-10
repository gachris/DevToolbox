using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DevToolbox.Wpf.Demo.ViewModels;

public class NetBrowserViewModel : ObservableObject
{
    private static bool _isFirst = false;

    static NetBrowserViewModel()
    {
        _isFirst = true;
    }

    public NetBrowserViewModel()
    {
        if (_isFirst)
        {
            _isFirst = false;
            Items = new ObservableCollection<TabItemContent>()
            {
                new TabItemContent() { Header = "Header_1", Content = "Content_1" },
                new TabItemContent() { Header = "Header_2", Content = "Content_2" },
                new TabItemContent() { Header = "Header_3", Content = "Content_3" }
            };
        }
        else
        {
            Items = new ObservableCollection<TabItemContent>();
        }
    }

    public ObservableCollection<TabItemContent> _items;
    public ObservableCollection<TabItemContent> Items
    {
        get { return _items; }
        set { SetProperty(ref _items, value, nameof(Items)); }
    }
}
