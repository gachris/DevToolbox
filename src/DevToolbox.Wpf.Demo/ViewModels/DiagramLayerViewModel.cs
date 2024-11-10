using CommunityToolkit.Mvvm.ComponentModel;

namespace DevToolbox.Wpf.Demo.ViewModels;

public class DiagramLayerViewModel : ObservableObject
{
    #region Fields/Consts

    private object _source = null!;
    private double _width;
    private double _height;

    #endregion

    #region Properties

    public object Source
    {
        get => _source;
        set => SetProperty(ref _source, value, nameof(Source));
    }

    public double Width
    {
        get => _width;
        set => SetProperty(ref _width, value, nameof(Width));
    }

    public double Height
    {
        get => _height;
        set => SetProperty(ref _height, value, nameof(Height));
    }

    #endregion
}