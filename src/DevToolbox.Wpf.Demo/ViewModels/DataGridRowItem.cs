using CommunityToolkit.Mvvm.ComponentModel;

namespace DevToolbox.Wpf.Demo.ViewModels;

public class DataGridRowItem : ObservableObject
{
    #region Fields/Consts

    private string _gameName = null!;
    private string _creator = null!;
    private string _publisher = null!;
    private string _owner = null!;
    private int _points;

    #endregion

    #region Properties

    public string GameName
    {
        get => _gameName;
        set => SetProperty(ref _gameName, value, nameof(GameName));
    }

    public string Creator
    {
        get => _creator;
        set => SetProperty(ref _creator, value, nameof(Creator));
    }

    public string Publisher
    {
        get => _publisher;
        set => SetProperty(ref _publisher, value, nameof(Publisher));
    }

    public string Owner
    {
        get => _owner;
        set => SetProperty(ref _owner, value, nameof(Owner));
    }

    public int Points
    {
        get => _points;
        set => SetProperty(ref _points, value, nameof(Points));
    }

    #endregion
}
