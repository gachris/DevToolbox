using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;

namespace DevToolbox.Wpf.Windows;

public partial class DocumentPrintWindow : WindowEx
{
    #region Properties

    public UIElement Element { get; }

    public string Description { get; }

    #endregion

    public DocumentPrintWindow(UIElement element, string description)
    {
        InitializeComponent();
        Element = element;
        Description = description;
    }

    #region Events Subscription

    private void FixedDocument_Loaded(object sender, RoutedEventArgs e)
    {
        var fixedDocument = (FixedDocument)sender;

        var grid = new Grid();
        grid.Children.Add(Element);

        var fixedPage = new FixedPage();
        fixedPage.Children.Add(grid);

        var widthBinding = new Binding("ActualWidth")
        {
            Source = fixedPage
        };
        var heightBinding = new Binding("ActualHeight")
        {
            Source = fixedPage
        };
        grid.SetBinding(WidthProperty, widthBinding);
        grid.SetBinding(HeightProperty, heightBinding);

        var pageContent = new PageContent();
        (pageContent as IAddChild).AddChild(fixedPage);

        fixedDocument.Pages.Add(pageContent);
    }

    #endregion
}