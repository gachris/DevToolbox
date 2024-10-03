using CommunityToolkit.Mvvm.ComponentModel;
using DevToolbox.Wpf.Windows.Snap;

namespace DevToolbox.Wpf.Demo.ViewModels;

public partial class WindowViewModel : ObservableObject
{
    public bool WindowArranging
    {
        get => SnapSettings.WindowArranging;
        set
        {
            SnapSettings.WindowArranging = value;
            OnPropertyChanged();
        }
    }

    public bool SnapSizing
    {
        get => SnapSettings.SnapSizing;
        set
        {
            SnapSettings.SnapSizing = value;
            OnPropertyChanged();
        }
    }

    public bool DockMoving
    {
        get => SnapSettings.DockMoving;
        set
        {
            SnapSettings.DockMoving = value;
            OnPropertyChanged();
        }
    }

    public bool DragFromMaximize
    {
        get => SnapSettings.DragFromMaximize;
        set
        {
            SnapSettings.DragFromMaximize = value;
            OnPropertyChanged();
        }
    }
}