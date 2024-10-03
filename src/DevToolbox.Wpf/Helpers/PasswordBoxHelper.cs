using System.Windows;
using System.Windows.Controls;

namespace DevToolbox.Wpf.Helpers;

/// <summary>
/// Provides attached properties to monitor the length of the password entered
/// in a PasswordBox and makes it accessible through dependency properties.
/// </summary>
public class PasswordBoxHelper
{
    #region Fields/Consts

    /// <summary>
    /// A key for the read-only attached property that stores the length of the password.
    /// </summary>
    private static readonly DependencyPropertyKey PasswordLengthPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly(
            "PasswordLength",
            typeof(int),
            typeof(PasswordBoxHelper),
            new PropertyMetadata(0));

    /// <summary>
    /// The attached property that holds the length of the password.
    /// </summary>
    public static readonly DependencyProperty PasswordLengthProperty = PasswordLengthPropertyKey.DependencyProperty;

    /// <summary>
    /// An attached property that indicates whether the PasswordBox should monitor its password length.
    /// </summary>
    private static readonly DependencyProperty MonitorPasswordLengthProperty =
        DependencyProperty.RegisterAttached(
            "MonitorPasswordLength",
            typeof(bool),
            typeof(PasswordBoxHelper),
            new PropertyMetadata(false, OnMonitorPasswordLengthChanged));

    #endregion

    #region Methods

    /// <summary>
    /// Gets the length of the password from the attached property.
    /// </summary>
    /// <param name="obj">The target PasswordBox to get the property value from.</param>
    /// <returns>The length of the password entered in the PasswordBox.</returns>
    public static int GetPasswordLength(DependencyObject obj) => (int)obj.GetValue(PasswordLengthProperty);

    /// <summary>
    /// Sets the length of the password in the attached property.
    /// </summary>
    /// <param name="obj">The target PasswordBox to set the property value on.</param>
    /// <param name="value">The length of the password to set.</param>
    private static void SetPasswordLength(DependencyObject obj, int value) => obj.SetValue(PasswordLengthPropertyKey, value);

    /// <summary>
    /// Gets the value of the MonitorPasswordLength attached property.
    /// </summary>
    /// <param name="dependencyObject">The target object to get the property value from.</param>
    /// <returns>The current value of the MonitorPasswordLength property.</returns>
    public static bool GetMonitorPasswordLength(DependencyObject dependencyObject) => (bool)dependencyObject.GetValue(MonitorPasswordLengthProperty);

    /// <summary>
    /// Sets the value of the MonitorPasswordLength attached property.
    /// </summary>
    /// <param name="dependencyObject">The target object to set the property value on.</param>
    /// <param name="value">True to monitor the password length; otherwise, false.</param>
    public static void SetMonitorPasswordLength(DependencyObject dependencyObject, bool value) => dependencyObject.SetValue(MonitorPasswordLengthProperty, value);

    /// <summary>
    /// Called when the MonitorPasswordLength property changes. 
    /// Subscribes or unsubscribes to the PasswordChanged event based on the property's value.
    /// </summary>
    /// <param name="d">The dependency object that the property is attached to.</param>
    /// <param name="e">The event data containing the old and new property values.</param>
    private static void OnMonitorPasswordLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox passwordBox) return;

        if ((bool)e.NewValue)
            passwordBox.PasswordChanged += PasswordChanged;
        else
            passwordBox.PasswordChanged -= PasswordChanged;
    }

    /// <summary>
    /// Handles the PasswordChanged event of the PasswordBox. 
    /// Updates the PasswordLength attached property whenever the password changes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event arguments.</param>
    private static void PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox) return;
        SetPasswordLength(passwordBox, passwordBox.Password.Length);
    }

    #endregion
}
