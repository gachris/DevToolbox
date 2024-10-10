using DevToolbox.Wpf.Mvvm;

namespace DevToolbox.Wpf.Controls;

internal class LabColor : NotifyPropertyChanged
{
    #region Fields/Consts

    private int _lightness;
    private int _a;
    private int _b;

    #endregion

    #region Properties

    public int Lightness
    {
        get => _lightness;
        set => SetProperty(ref _lightness, value);
    }

    public int A
    {
        get => _a;
        set => SetProperty(ref _a, value);
    }

    public int B
    {
        get => _b;
        set => SetProperty(ref _b, value);
    }

    #endregion

    #region Methods Overrides

    protected override void ValidateProperty(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(Lightness):
                if (_lightness is < 0 or > 100)
                {
                    AddError(propertyName, "Lightness must be between 0 and 100.");
                }
                else
                {
                    ClearErrors(propertyName);
                }
                break;
            case nameof(A):
                if (_a is < (-128) or > 127)
                {
                    AddError(propertyName, "A must be between -128 and 127.");
                }
                else
                {
                    ClearErrors(propertyName);
                }
                break;
            case nameof(B):
                if (_b is < (-128) or > 127)
                {
                    AddError(propertyName, "B must be between -128 and 127.");
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

    #endregion
}
