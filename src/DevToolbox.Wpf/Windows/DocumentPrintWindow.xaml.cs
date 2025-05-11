using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;

namespace DevToolbox.Wpf.Windows;

/// <summary>
/// Provides a window for displaying and printing a UIElement as a fixed document.
/// </summary>
public partial class DocumentPrintWindow : WindowEx
{
    #region Properties

    /// <summary>
    /// Gets the UI element that will be rendered and printed in the document.
    /// </summary>
    public UIElement Element { get; }

    /// <summary>
    /// Gets the textual description of the document or content being printed.
    /// </summary>
    public string Description { get; }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentPrintWindow"/> class
    /// with the specified element to print and its description.
    /// </summary>
    /// <param name="element">The UI element to render in the fixed document.</param>
    /// <param name="description">A description of the document content.</param>
    public DocumentPrintWindow(UIElement element, string description)
    {
        InitializeComponent();
        Element = element;
        Description = description;
    }

    #region Events Subscription

    /// <summary>
    /// Handles the FixedDocument.Loaded event.
    /// Creates a fixed page containing the provided element and adds it to the document.
    /// </summary>
    /// <param name="sender">The <see cref="FixedDocument"/> that was loaded.</param>
    /// <param name="e">The event data associated with the loaded event.</param>
    private void FixedDocument_Loaded(object sender, RoutedEventArgs e)
    {
        var fixedDocument = (FixedDocument)sender;

        // Wrap the element in a Grid to enable sizing bindings
        var grid = new Grid();
        grid.Children.Add(Element);

        // Create a page and add the grid
        var fixedPage = new FixedPage();
        fixedPage.Children.Add(grid);

        // Bind the grid's size to the page's actual dimensions
        var widthBinding = new Binding("ActualWidth") { Source = fixedPage };
        var heightBinding = new Binding("ActualHeight") { Source = fixedPage };
        grid.SetBinding(WidthProperty, widthBinding);
        grid.SetBinding(HeightProperty, heightBinding);

        // Add the page to the document
        var pageContent = new PageContent();
        ((IAddChild)pageContent).AddChild(fixedPage);
        fixedDocument.Pages.Add(pageContent);
    }

    #endregion
}