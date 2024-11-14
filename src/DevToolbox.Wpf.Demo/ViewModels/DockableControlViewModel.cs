using System.Windows.Controls;

namespace DevToolbox.Wpf.Demo.ViewModels;

public class DockableControlViewModel : DocuemntControlViewModel
{
    #region Fields/Consts

    private Dock _dock;

    #endregion

    #region Properties

    public Dock Dock
    {
        get => _dock;
        set => SetProperty(ref _dock, value, nameof(Dock));
    }

    #endregion
}