using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevToolbox.Wpf.Helpers;

internal class HexTexBoxHelper
{
    public static bool GetValidate(DependencyObject obj)
    {
        return (bool)obj.GetValue(ValidateProperty);
    }

    public static void SetValidate(DependencyObject obj, bool value)
    {
        obj.SetValue(ValidateProperty, value);
    }

    public static readonly DependencyProperty ValidateProperty =
        DependencyProperty.RegisterAttached("Validate", typeof(bool), typeof(HexTexBoxHelper), new PropertyMetadata(false, OnValidateChanged));

    private static void OnValidateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var textBox = (TextBox)d;
        var validate = (bool)e.NewValue;

        if (validate)
        {
            textBox.MaxLength = 8;
            textBox.LostFocus += TextBox_LostFocus;
            textBox.PreviewTextInput += TextBox_PreviewTextInput;
        }
        else
        {
            textBox.LostFocus -= TextBox_LostFocus;
            textBox.PreviewTextInput -= TextBox_PreviewTextInput;
        }
    }

    private static void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        var textBox = (TextBox)sender;
        var hex = textBox.Text ?? string.Empty;

        if (hex.StartsWith("#"))
        {
            hex = hex.Replace("#", string.Empty);
        }

        if (hex.Length is < 6 and not 3)
        {
            hex = hex.PadLeft(6, '0');
        }

        if (hex.Length is < 8 and not 3)
        {
            hex = hex.PadLeft(8, 'f');
        }

        var color = (Color)ColorConverter.ConvertFromString("#" + hex);

        textBox.Text = color.ToString();
    }

    private static void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        var hexPattern = "^[0-9a-fA-F]+$";

        if (!Regex.IsMatch(e.Text, hexPattern))
        {
            e.Handled = true;
        }
    }
}