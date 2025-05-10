using System;
using System.Threading.Tasks;
using CommonServiceLocator;
using DevToolbox.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace DevToolbox.WinUI;

/// <summary>
/// Configures global exception handling for unhandled exceptions across AppDomain,
/// XAML resource loading, UI thread, and unobserved Task exceptions.
/// </summary>
public static class GlobalExceptionHandler
{
    #region Methods

    /// <summary>
    /// Sets up handlers for unhandled exceptions from various sources,
    /// and displays errors via <see cref="IDialogService"/>.
    /// </summary>
    public static void SetupExceptionHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            ShowErrorDialog((Exception)e.ExceptionObject);

        Microsoft.UI.Xaml.Application.Current.DebugSettings.XamlResourceReferenceFailed += (s, e) =>
        {
            ShowErrorDialog(new Exception(e.Message));
        };

        Microsoft.UI.Xaml.Application.Current.UnhandledException += (s, e) =>
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
    /// Displays an error dialog for the specified exception using the configured dialog service.
    /// </summary>
    /// <param name="exception">The exception to display in the error dialog.</param>
    private static async void ShowErrorDialog(Exception exception)
    {
        var dialogService = ServiceLocator.Current.GetService<IDialogService>();
        ArgumentNullException.ThrowIfNull(dialogService, nameof(dialogService));
        await dialogService.ShowErrorAsync(exception).ConfigureAwait(false);
    }

    #endregion
}