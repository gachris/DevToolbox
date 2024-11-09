namespace DevToolbox.Wpf.Demo.Helpers;

public class GlobalExceptionHandler
{
    #region Fields/Consts

    //private static readonly DialogOptions ErrorDialogOptions = new()
    //{
    //    Width = 458,
    //    MinHeight = 185,
    //    MaxHeight = 560,
    //    SizeToContent = SizeToContent.Height,
    //    WindowTitle = Resources.Unhandled_exception,
    //    PluginButtons = new PluginButton[]
    //    {
    //        new()
    //        {
    //            IsDefault = true,
    //            ButtonOrder = 10,
    //            ButtonPosition = PluginButtonPosition.Right,
    //            ButtonType = PluginButtonType.OK,
    //        }
    //    }
    //};

    #endregion

    #region Methods

    public static void SetupExceptionHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            ShowErrorDialog((Exception)e.ExceptionObject);

        System.Windows.Application.Current.DispatcherUnhandledException += (s, e) =>
        {
            ShowErrorDialog(e.Exception);
            e.Handled = true;
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            ShowErrorDialog(e.Exception);
            e.SetObserved();
        };
    }

    private static void ShowErrorDialog(Exception exception)
    {
        System.Windows.Forms.MessageBox.Show(exception.Message);
        //var dialogService = ServiceLocator.Current.GetService<IDialogService>();

        //ArgumentNullException.ThrowIfNull(dialogService, nameof(dialogService));

        //var view = new ErrorDialogView(exception.Message, exception.StackTrace);
        //dialogService.ShowDialog(null, view, ErrorDialogOptions);
    }

    #endregion
}