using System.Windows.Controls;
using System.Windows;
using DevToolbox.Wpf.Windows;

namespace DevToolbox.Wpf.Demo.Windows;

public partial class NotificationWindow : SidePanelWindow
{
    public NotificationWindow()
    {
        InitializeComponent();
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        // Retrieve the input values
        string name = NameTextBox.Text;
        string language = (LanguageComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "None";
        bool isSubscribed = SubscribeCheckBox.IsChecked ?? false;

        // Display the result
        ResultTextBlock.Text = $"Hello, {name}! You selected {language}.\nSubscribed: {isSubscribed}";
    }
}