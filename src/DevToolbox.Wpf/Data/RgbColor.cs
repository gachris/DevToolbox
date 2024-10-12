namespace DevToolbox.Wpf.Data;

internal class RgbColor : NotifyPropertyChanged
{
    #region Fields/Consts

    private byte _red;
    private byte _green;
    private byte _blue;

    #endregion

    #region Properties

    public byte Red
    {
        get => _red;
        set => SetProperty(ref _red, value);
    }

    public byte Green
    {
        get => _green;
        set => SetProperty(ref _green, value);
    }

    public byte Blue
    {
        get => _blue;
        set => SetProperty(ref _blue, value);
    }

    #endregion
}
