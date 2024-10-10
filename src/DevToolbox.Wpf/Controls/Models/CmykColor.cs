using DevToolbox.Wpf.Mvvm;

namespace DevToolbox.Wpf.Controls;

internal class CmykColor : NotifyPropertyChanged
{
    #region Fields/Consts

    private int _cyan;
    private int _magenta;
    private int _yellow;
    private int _key;

    #endregion

    #region Properties

    public int Cyan
    {
        get => _cyan;
        set => SetProperty(ref _cyan, value);
    }

    public int Magenta
    {
        get => _magenta;
        set => SetProperty(ref _magenta, value);
    }

    public int Yellow
    {
        get => _yellow;
        set => SetProperty(ref _yellow, value);
    }

    public int Key
    {
        get => _key;
        set => SetProperty(ref _key, value);
    }

    #endregion

    #region Methods Overrides

    protected override void ValidateProperty(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(Cyan):
            case nameof(Magenta):
            case nameof(Yellow):
            case nameof(Key):
                if (IsCmykOutOfRange(_cyan) || IsCmykOutOfRange(_magenta) || IsCmykOutOfRange(_yellow) || IsCmykOutOfRange(_key))
                {
                    AddError(propertyName, "CMYK values must be between 0 and 100.");
                }
                else
                {
                    ClearErrors(propertyName);
                }
                break;
            default:
                base.ValidateProperty(propertyName);
                break;
        }
    }

    private static bool IsCmykOutOfRange(int value)
    {
        return value is < 0 or > 100;
    }

    #endregion
}
