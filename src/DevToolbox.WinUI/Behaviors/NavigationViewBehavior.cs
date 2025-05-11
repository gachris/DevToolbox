using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace DevToolbox.WinUI.Behaviors;

/// <summary>
/// A behavior for <see cref="NavigationView"/> that synchronizes the selected menu item
/// and provides support for binding selected value and handling the SettingsItem.
/// </summary>
public class NavigationViewBehavior : Behavior<NavigationView>
{
    #region Dependency Properties

    /// <summary>
    /// Identifies the <see cref="IsSettingsSelected"/> attached dependency property.
    /// </summary>
    public static readonly DependencyProperty IsSettingsSelectedProperty =
        DependencyProperty.RegisterAttached(
            nameof(IsSettingsSelected),
            typeof(bool),
            typeof(NavigationViewBehavior),
            new PropertyMetadata(false, OnIsSettingsSelectedChanged));

    /// <summary>
    /// Identifies the <see cref="SelectedValue"/> attached dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectedValueProperty =
        DependencyProperty.RegisterAttached(
            nameof(SelectedValue),
            typeof(object),
            typeof(NavigationViewBehavior),
            new PropertyMetadata(null, OnSelectedValueChanged));

    /// <summary>
    /// Identifies the <see cref="SelectedValuePath"/> attached dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectedValuePathProperty =
        DependencyProperty.RegisterAttached(
            nameof(SelectedValuePath),
            typeof(string),
            typeof(NavigationViewBehavior),
            new PropertyMetadata(null, OnSelectedValuePathChanged));

    #endregion

    #region Fields

    private bool _skipSelectedValueChanged;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets a value indicating whether the NavigationView
    /// SettingsItem is selected.
    /// </summary>
    public bool IsSettingsSelected
    {
        get => (bool)GetValue(IsSettingsSelectedProperty);
        set => SetValue(IsSettingsSelectedProperty, value);
    }

    /// <summary>
    /// Gets or sets the bound value of the selected item. Supports
    /// value-path binding via <see cref="SelectedValuePath"/>.
    /// </summary>
    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the name of the property on menu items used to match
    /// the <see cref="SelectedValue"/> when updating selection.
    /// </summary>
    public string SelectedValuePath
    {
        get => (string)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }

    #endregion

    #region Overrides

    /// <summary>
    /// Attaches behavior to the associated <see cref="NavigationView"/>, subscribing to selection events.
    /// </summary>
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        UpdateSelectedValue();
    }

    /// <summary>
    /// Detaches behavior from the associated <see cref="NavigationView"/>, unsubscribing events.
    /// </summary>
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
    }

    #endregion

    #region Dependency Property Callbacks

    private static void OnIsSettingsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((NavigationViewBehavior)d).OnIsSettingsSelectedChanged(e);
    }

    private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((NavigationViewBehavior)d).OnSelectedValueChanged(e);
    }

    private static void OnSelectedValuePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((NavigationViewBehavior)d).OnSelectedValuePathChanged(e);
    }

    #endregion

    #region Callback Handlers

    private void OnIsSettingsSelectedChanged(DependencyPropertyChangedEventArgs e)
    {
        if (AssociatedObject is null)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            AssociatedObject.SelectedItem = AssociatedObject.SettingsItem;
        }
    }

    private void OnSelectedValueChanged(DependencyPropertyChangedEventArgs e)
    {
        if (_skipSelectedValueChanged)
            return;

        UpdateSelectedItem();
    }

    private void OnSelectedValuePathChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateSelectedValue();
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Reads the current <see cref="NavigationView.SelectedItem"/> and updates <see cref="SelectedValue"/>
    /// based on the <see cref="SelectedValuePath"/> if specified.
    /// </summary>
    private void UpdateSelectedValue()
    {
        if (AssociatedObject is null)
        {
            return;
        }

        var selected = AssociatedObject.SelectedItem;
        if (!string.IsNullOrEmpty(SelectedValuePath))
        {
            var property = selected?.GetType().GetProperty(SelectedValuePath);
            SelectedValue = property?.GetValue(selected);
        }
        else
        {
            SelectedValue = selected;
        }
    }

    /// <summary>
    /// Updates the <see cref="NavigationView.SelectedItem"/> to match the current <see cref="SelectedValue"/>
    /// by comparing values or properties defined in <see cref="SelectedValuePath"/>.
    /// </summary>
    private void UpdateSelectedItem()
    {
        if (AssociatedObject is null || IsSettingsSelected)
            return;

        if (!string.IsNullOrEmpty(SelectedValuePath))
        {
            foreach (var item in AssociatedObject.MenuItems)
            {
                var prop = item.GetType().GetProperty(SelectedValuePath);
                var value = prop?.GetValue(item);
                if (Equals(value, SelectedValue))
                {
                    AssociatedObject.SelectedItem = item;
                    return;
                }
            }
            AssociatedObject.SelectedItem = null;
        }
        else
        {
            AssociatedObject.SelectedItem = SelectedValue;
        }
    }

    #endregion

    #region Event Handlers

    private void AssociatedObject_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        IsSettingsSelected = args.IsSettingsSelected;
        _skipSelectedValueChanged = true;
        UpdateSelectedValue();
        _skipSelectedValueChanged = false;
    }

    #endregion
}