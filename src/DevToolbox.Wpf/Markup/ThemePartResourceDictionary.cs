using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using DevToolbox.Wpf.Media;

namespace DevToolbox.Wpf.Markup;

/// <summary>
/// Represents a resource dictionary that supports theming in the application.
/// It manages theme part resources and enables or disables them based on the current application theme.
/// </summary>
public class ThemePartResourceDictionary : ResourceDictionary
{
    #region Fields/Consts

    private static readonly List<ThemePartResourceDictionary> _resourceDictionaries = new();
    private bool _isEnabled;
    private Uri? _mutableSource;
    private object? _key;
    private ResourceDictionary? _disabledSource;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the list of all <see cref="ThemePartResourceDictionary"/> instances.
    /// </summary>
    internal static List<ThemePartResourceDictionary> ResourceDictionaries => _resourceDictionaries;

    /// <summary>
    /// Gets or sets a value indicating whether this resource dictionary is enabled.
    /// When set, triggers actions to add or remove the associated resources.
    /// </summary>
    protected bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            try
            {
                if (_isEnabled == value)
                {
                    return;
                }

                _isEnabled = value;
                OnIsEnabledChanged();
            }
            catch
            {
                _isEnabled = !_isEnabled; // Fallback in case of an error.
            }
        }
    }

    /// <summary>
    /// Gets or sets the source URI for the disabled resource dictionary.
    /// </summary>
    public Uri? DisabledSource
    {
        get => _mutableSource;
        set
        {
            if (_mutableSource == value)
            {
                return;
            }

            _mutableSource = value;
        }
    }

    /// <summary>
    /// Gets or sets the key associated with this resource dictionary.
    /// Throws an exception if trying to set a new key when one already exists.
    /// </summary>
    public object? Key
    {
        get => _key;
        set
        {
            if (_key == value)
            {
                return;
            }

            if (_key != null)
            {
                throw new ArgumentException(nameof(Key));
            }

            _key = value;
            OnKeyChanged();
        }
    }

    #endregion

    static ThemePartResourceDictionary()
    {
        ThemeManager.ApplicationThemeCoreChanged += ThemeManager_ApplicationThemeCoreChanged;
    }

    #region Methods

    /// <summary>
    /// Called when the enabled state of the resource dictionary changes.
    /// Adds or removes the resource dictionary from the application's merged dictionaries.
    /// </summary>
    private void OnIsEnabledChanged()
    {
        if (!IsEnabled)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Resources.MergedDictionaries.Remove(_disabledSource);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    Application.Current.Resources.MergedDictionaries.Remove(_disabledSource)), DispatcherPriority.Normal);
            }

            return;
        }

        var key = Key as ThemePartKeyExtension;

        // Skip loading if this is DevToolbox and override is enabled
        if (key != null &&
            key.ThemeName == ThemeManager.ApplicationThemeNameCore &&
            key.AssemblyName?.StartsWith("DevToolbox", StringComparison.OrdinalIgnoreCase) == true &&
            ThemeManager.OverrideCoreTheming)
        {
            return;
        }

        if (key?.ThemeName == ThemeManager.ApplicationThemeNameCore)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                _disabledSource = new ResourceDictionary() { Source = DisabledSource };
                Application.Current.Resources.MergedDictionaries.Add(_disabledSource);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    _disabledSource = new ResourceDictionary() { Source = DisabledSource };
                    Application.Current.Resources.MergedDictionaries.Add(_disabledSource);
                }, DispatcherPriority.Normal);
            }
        }
    }

    /// <summary>
    /// Called when the key of the resource dictionary changes.
    /// Updates the disabled source URI and enables the dictionary if it matches the current theme.
    /// </summary>
    private void OnKeyChanged()
    {
        if (DisabledSource == null && Key is ThemePartKeyExtension key)
        {
            DisabledSource = key.Uri;

            if (key.ThemeName == ThemeManager.ApplicationThemeNameCore)
            {
                IsEnabled = true;
            }
        }

        // See if there’s already a dictionary with the same ThemePartKeyExtension
        var existingIndex = _resourceDictionaries
            .FindIndex(x =>
                x.Key is ThemePartKeyExtension existingKey
                && existingKey.Equals(Key));

        if (existingIndex >= 0)
        {
            // Replace the old one in-place
            _resourceDictionaries[existingIndex] = this;
        }
        else
        {
            // No match, just add
            _resourceDictionaries.Add(this);
        }
    }

    #endregion

    #region Events Subscriptions

    /// <summary>
    /// Handles the <see cref="ThemeManager.ApplicationThemeChanged"/> event.
    /// Disables all resource dictionaries and enables those that match the new theme.
    /// </summary>
    private static void ThemeManager_ApplicationThemeCoreChanged(object? sender, EventArgs e)
    {
        ResourceDictionaries.ForEach(x => x.IsEnabled = false);

        var themes = ResourceDictionaries.Where(x => x.Key is ThemePartKeyExtension key && key.ThemeName == ThemeManager.ApplicationThemeNameCore).ToArray();

        foreach (var partResourceDictionary in themes)
        {
            partResourceDictionary.IsEnabled = true;
        }
    }

    #endregion
}
