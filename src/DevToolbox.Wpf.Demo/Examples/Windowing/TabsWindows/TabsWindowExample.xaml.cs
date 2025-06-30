using DevToolbox.Wpf.Demo.ViewModels;
using DevToolbox.Wpf.Windows;
using DevToolbox.Wpf.Windows.Effects;

namespace DevToolbox.Wpf.Demo.Examples.Windowing.TabsWindows;

public partial class TabsWindowExample : TabsWindow
{
    public TabsWindowExample()
    {
        InitializeComponent();

        WindowBehavior.SetWindowEffect(this, new Tabbed());
    }

    protected override TabsWindow CreateDefaultTearOffWindow(object item)
    {
        return new TabsWindowExample
        {
            DataContext = new TabsWindowExampleViewModel()
            {
                Items = { (TabItemContent)item }
            }
        };
    }
}