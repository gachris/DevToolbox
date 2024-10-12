namespace DevToolbox.Wpf.Data;

internal class HsvColor : NotifyPropertyChanged
{
    #region Fields/Consts

    private int _hue;
    private int _saturation;
    private int _value;

    #endregion

    #region Properties

    public int Hue
    {
        get => _hue;
        set => SetProperty(ref _hue, value);
    }

    public int Saturation
    {
        get => _saturation;
        set => SetProperty(ref _saturation, value);
    }

    public int Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    #endregion

    #region Methods Overrides

    protected override void ValidateProperty(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(Hue):
                if (_hue is < 0 or > 360)
                {
                    AddError(propertyName, "Hue must be between 0 and 360.");
                }
                else
                {
                    ClearErrors(propertyName);
                }
                break;
            case nameof(Saturation):
                if (_saturation is < 0 or > 100)
                {
                    AddError(propertyName, "Saturation must be between 0 and 100.");
                }
                break;
            case nameof(Value):
                if (_value is < 0 or > 100)
                {
                    AddError(propertyName, "Value must be between 0 and 100.");
                }
                break;
            default:
                base.ValidateProperty(propertyName);
                break;
        }
    }

    #endregion
}
