using System;
using System.Threading.Tasks;
using DevToolbox.Core.Contracts;
using DevToolbox.WinUI.Contracts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Printing;
using Windows.Graphics.Printing;

namespace DevToolbox.WinUI.Services;

/// <summary>
/// Implements <see cref="IPrintDialogService"/> to display print dialogs
/// and handle printing of XAML content.
/// </summary>
public class PrintDialogService : IPrintDialogService
{
    #region Fields/Consts

    private PrintManager? _printManager;
    private PrintDocument? _printDocument;
    private IPrintDocumentSource? _printDocumentSource;
    private readonly IAppWindowService _appWindowService;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="PrintDialogService"/> class.
    /// </summary>
    /// <param name="appWindowService">
    /// Service providing access to the main application window for dialog parenting.
    /// </param>
    public PrintDialogService(IAppWindowService appWindowService)
    {
        _appWindowService = appWindowService;
    }

    #region IPrintDialogService Implementation

    /// <inheritdoc/>
    public async Task<bool?> PrintAsync(object content)
    {
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(_appWindowService.MainWindow);
        _printManager = PrintManagerInterop.GetForWindow(hWnd);
        _printManager.PrintTaskRequested += PrintManager_PrintTaskRequested;

        _printDocument = new PrintDocument();
        _printDocumentSource = _printDocument.DocumentSource;

        _printDocument.Paginate += PaginateHandler;
        _printDocument.GetPreviewPage += GetPreviewPageHandler;
        _printDocument.AddPages += AddPagesHandler;

        return await PrintManagerInterop.ShowPrintUIForWindowAsync(hWnd);

        void GetPreviewPageHandler(object sender, GetPreviewPageEventArgs e)
        {
            _printDocument?.SetPreviewPage(e.PageNumber, CreatePrintablePage());
        }

        void AddPagesHandler(object sender, AddPagesEventArgs e)
        {
            _printDocument?.AddPage(CreatePrintablePage());
            _printDocument?.AddPagesComplete();
        }

        FrameworkElement CreatePrintablePage()
        {
            var stackPanel = new ContentControl
            {
                Content = content
            };

            return stackPanel;
        }
    }

    #endregion

    #region Events Subscriptions

    /// <summary>
    /// Handles the PrintTaskRequested event to set the print document source.
    /// </summary>
    /// <param name="sender">The <see cref="PrintManager"/> sending the request.</param>
    /// <param name="args">Print task request arguments containing the request object.</param>
    private void PrintManager_PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
    {
        args.Request.CreatePrintTask("Print", taskSourceRequested =>
        {
            taskSourceRequested.SetSource(_printDocumentSource);
        });
    }

    /// <summary>
    /// Handles pagination by indicating the total number of preview pages.
    /// </summary>
    private void PaginateHandler(object sender, PaginateEventArgs e)
    {
        _printDocument?.SetPreviewPageCount(1, PreviewPageCountType.Final);
    }

    #endregion
}