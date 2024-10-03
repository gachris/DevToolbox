using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Highlighting;

namespace DevToolbox.Wpf.Demo.Views;

public partial class XamlEditorView : UserControl
{
    public static readonly DependencyProperty SourceCodeProperty =
        DependencyProperty.Register("SourceCode", typeof(string), typeof(XamlEditorView), new PropertyMetadata(null, OnSourceCodeChanged));

    public string SourceCode
    {
        get { return (string)GetValue(SourceCodeProperty); }
        set { SetValue(SourceCodeProperty, value); }
    }

    public XamlEditorView()
    {
        InitializeComponent();

        var highlighting = textEditor.SyntaxHighlighting;
        highlighting.GetNamedColor("Comment").Foreground = new SimpleHighlightingBrush(Color.FromRgb(107, 142, 35));
        highlighting.GetNamedColor("CData").Foreground = new SimpleHighlightingBrush(Colors.Black);
        highlighting.GetNamedColor("DocType").Foreground = new SimpleHighlightingBrush(Colors.Black);
        highlighting.GetNamedColor("XmlDeclaration").Foreground = new SimpleHighlightingBrush(Colors.DarkGoldenrod);
        highlighting.GetNamedColor("XmlTag").Foreground = new SimpleHighlightingBrush(Color.FromRgb(95, 130, 232));
        highlighting.GetNamedColor("AttributeName").Foreground = new SimpleHighlightingBrush(Color.FromRgb(135, 206, 250));
        highlighting.GetNamedColor("AttributeValue").Foreground = new SimpleHighlightingBrush(Color.FromRgb(255, 160, 122));
        highlighting.GetNamedColor("Entity").Foreground = new SimpleHighlightingBrush(Colors.Blue);
        highlighting.GetNamedColor("BrokenEntity").Foreground = new SimpleHighlightingBrush(Colors.Brown);

        foreach (var color in highlighting.NamedHighlightingColors)
        {
            color.FontWeight = null;
        }
        textEditor.SyntaxHighlighting = null;
        textEditor.SyntaxHighlighting = highlighting;
    }

    private static void OnSourceCodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var codeView = (XamlEditorView)d;
        codeView.OnSourceCodeChanged((string)e.OldValue, (string)e.NewValue);
    }

    private void OnSourceCodeChanged(string oldValue, string newValue)
    {
        textEditor.Text = newValue;
    }
}
