using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DevToolbox.Wpf.Demo.ViewModels;

public abstract class DockIBaseItemViewModel : ObservableObject
{
    #region Fields/Consts

    private string _header = null!;
    private ImageSource? _icon;

    #endregion

    #region Properties

    public string Header
    {
        get => _header;
        set => SetProperty(ref _header, value, nameof(Header));
    }

    public ImageSource? Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value, nameof(Icon));
    }

    #endregion
}