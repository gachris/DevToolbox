using System;
using System.Windows;
using DevToolbox.Wpf.Media;

namespace DevToolbox.Wpf.Markup;

/// <summary>
/// A <see cref="ResourceDictionary"/> that dynamically updates its merged resources
/// in response to application theming state changes (e.g., core theming override or Fluent theme activation).
/// </summary>
public class ThemeResourceDictionary : ResourceDictionary, IDisposable
{
    private Uri? _source;
    private ResourceDictionary? _currentDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeResourceDictionary"/> class.
    /// Registers for theme-related events to respond to dynamic changes in application theming.
    /// </summary>
    public ThemeResourceDictionary()
    {
        ThemeManager.OverrideCoreThemingChanged += OnOverrideCoreThemingChanged;
        ThemeManager.RequestedThemeChanged += OnRequestedThemeChanged;
    }

    /// <summary>
    /// Gets or sets the URI source for the theme-specific resource dictionary.
    /// When set, this triggers loading or unloading the resource dictionary depending on the current theming state.
    /// </summary>
    public new Uri? Source
    {
        get => _source;
        set
        {
            if (_source != value)
            {
                _source = value;
                ApplySource();
            }
        }
    }

    /// <summary>
    /// Handles the <see cref="ThemeManager.OverrideCoreThemingChanged"/> event.
    /// Updates the resource dictionary source accordingly.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void OnOverrideCoreThemingChanged(object? sender, EventArgs e) => ApplySource();

    /// <summary>
    /// Handles the <see cref="ThemeManager.RequestedThemeChanged"/> event.
    /// Updates the resource dictionary source accordingly.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void OnRequestedThemeChanged(object? sender, EventArgs e) => ApplySource();

    /// <summary>
    /// Applies the appropriate resource dictionary based on the current theming settings.
    /// Removes the current dictionary if core theming is overridden or Fluent theme is enabled;
    /// otherwise, loads the dictionary from the provided <see cref="Source"/>.
    /// </summary>
    private void ApplySource()
    {
        if (_currentDictionary != null)
        {
            MergedDictionaries.Remove(_currentDictionary);
            _currentDictionary = null;
        }

        if (!ThemeManager.OverrideCoreTheming && !ThemeManager.IsFluentThemeEnabled && _source != null)
        {
            _currentDictionary = new ResourceDictionary { Source = _source };
            MergedDictionaries.Add(_currentDictionary);
        }
    }

    /// <summary>
    /// Releases event subscriptions to prevent memory leaks.
    /// Call this method when the dictionary is no longer needed or being disposed.
    /// </summary>
    public void Dispose()
    {
        ThemeManager.OverrideCoreThemingChanged -= OnOverrideCoreThemingChanged;
        ThemeManager.RequestedThemeChanged -= OnRequestedThemeChanged;
    }
}
