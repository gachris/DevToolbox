using System.Windows;
using System.Windows.Controls;
using DevToolbox.Wpf.Extensions;

namespace DevToolbox.Wpf.Helpers;

/// <summary>
/// Provides attached properties and behavior for <see cref="TextBox"/> controls.
/// </summary>
public static class TextBoxHelper
{
    /// <summary>
    /// Identifies the <c>HideClearButton</c> attached dependency property.
    /// When set to true, hides the internal clear/delete button in a <see cref="TextBox"/>.
    /// </summary>
    public static readonly DependencyProperty HideClearButtonProperty =
        DependencyProperty.RegisterAttached(
            "HideClearButton",
            typeof(bool),
            typeof(TextBoxHelper),
            new PropertyMetadata(false, OnHideClearButtonChanged));

    /// <summary>
    /// Gets the value of the <c>HideClearButton</c> attached property for a specified <see cref="DependencyObject"/>.
    /// </summary>
    /// <param name="obj">The dependency object.</param>
    /// <returns><c>true</c> if the clear button should be hidden; otherwise, <c>false</c>.</returns>
    public static bool GetHideClearButton(DependencyObject obj) =>
        (bool)obj.GetValue(HideClearButtonProperty);

    /// <summary>
    /// Sets the value of the <c>HideClearButton</c> attached property for a specified <see cref="DependencyObject"/>.
    /// </summary>
    /// <param name="obj">The dependency object.</param>
    /// <param name="value">True to hide the clear button; false to show it.</param>
    public static void SetHideClearButton(DependencyObject obj, bool value) =>
        obj.SetValue(HideClearButtonProperty, value);

    /// <summary>
    /// Called when the <c>HideClearButton</c> property changes.
    /// Hooks or unhooks the <see cref="FrameworkElement.Loaded"/> event to modify button visibility.
    /// </summary>
    /// <param name="d">The dependency object where the property changed.</param>
    /// <param name="e">Event data for the property change.</param>
    private static void OnHideClearButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            if ((bool)e.NewValue)
            {
                textBox.Loaded += TextBox_Loaded;
            }
            else
            {
                textBox.Loaded -= TextBox_Loaded;
            }
        }
    }

    /// <summary>
    /// Handles the <see cref="FrameworkElement.Loaded"/> event to locate and hide the clear/delete button.
    /// </summary>
    /// <param name="sender">The <see cref="TextBox"/> control.</param>
    /// <param name="e">Event data.</param>
    private static void TextBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            var deleteButton = textBox.FindName<Button>("ClearButton");
            deleteButton ??= textBox.FindName<Button>("DeleteButton");
            deleteButton ??= textBox.FindElementOfType<Button>();

            if (deleteButton != null)
            {
                deleteButton.Visibility = Visibility.Collapsed;
                deleteButton.IsHitTestVisible = false; // optional: prevent interaction
            }
        }
    }
}
