using System;
using System.Threading.Tasks;
using System.Windows;
using CommonServiceLocator;
using DevToolbox.Core.Contracts;

namespace DevToolbox.Wpf;

/// <summary>
/// Handles global exceptions for the application by subscribing to AppDomain, Dispatcher, and TaskScheduler events
/// and displaying error dialogs when unhandled exceptions occur.
/// </summary>
public static class GlobalExceptionHandler
{
    #region Methods

    /// <summary>
    /// Subscribes to global exception events (AppDomain, Dispatcher, and TaskScheduler)
    /// to ensure all unhandled exceptions are caught and displayed to the user.
    /// </summary>
    public static void SetupExceptionHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            ShowErrorDialog((Exception)e.ExceptionObject);

        Application.Current.DispatcherUnhandledException += (s, e) =>
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

    /// <summary>
    /// Retrieves the dialog service from the service locator and uses it to show an error dialog
    /// containing the exception message and stack trace.
    /// </summary>
    /// <param name="exception">The exception to display.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the <see cref="IDialogService"/> cannot be resolved from the service locator.
    /// </exception>
    private static async void ShowErrorDialog(Exception exception)
    {
        try
        {
            if (ServiceLocator.Current.GetService(typeof(IDialogService)) is not IDialogService dialogService)
                throw new ArgumentNullException(nameof(dialogService), "Could not resolve IDialogService from ServiceLocator.");
            await dialogService.ShowErrorAsync(exception);
        }
        catch
        {
            MessageBox.Show(exception.Message);
        }
    }

    #endregion
}