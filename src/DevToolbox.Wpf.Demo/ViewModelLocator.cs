using CommonServiceLocator;
using DevToolbox.Wpf.Demo.ViewModels;

namespace DevToolbox.Wpf.Demo;

public class ViewModelLocator
{
    #region Common

    public static SettingsViewModel SettingsViewModel => ServiceLocator.Current.GetInstance<SettingsViewModel>();

    #endregion
}
