using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DevToolbox.Wpf.Demo.ViewModels;

public class DocuemntControlViewModel : ObservableObject
{
    #region Properties

    public ObservableCollection<DockIBaseItemViewModel> Items { get; } = [];

    #endregion
}
