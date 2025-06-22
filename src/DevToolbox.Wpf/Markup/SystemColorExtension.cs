using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using DevToolbox.Wpf.Helpers;
using Microsoft.Win32;
#if NET8_0_OR_GREATER && WINDOWS10_0_17763_0_OR_GREATER
using Windows.UI.ViewManagement;
#else
using DevToolbox.Wpf.Media;
#endif

namespace DevToolbox.Wpf.Markup;

/// <summary>
/// Markup extension that returns a <see cref="Color"/> based on the system accent color
/// and selected <see cref="UIColorType"/> (e.g., AccentLight2, AccentDark3).
/// Updates dynamically when the system accent color changes.
/// </summary>
[MarkupExtensionReturnType(typeof(Color))]
public class SystemColorExtension : MarkupExtension, INotifyPropertyChanged
{
    /// <summary>
    /// Event triggered when the system accent color changes.
    /// </summary>
    public static event Action? SystemAccentChanged;

    /// <summary>
    /// Occurs when the <see cref="Value"/> property changes, enabling data binding to update.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemColorExtension"/> class.
    /// </summary>
    public SystemColorExtension() { }

    static SystemColorExtension()
    {
        SystemEvents.UserPreferenceChanged += (s, e) =>
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                SystemAccentChanged?.Invoke();
            }
        };
    }

    /// <summary>
    /// Gets or sets the color type to retrieve (e.g., Accent, AccentDark1).
    /// Default is <see cref="UIColorType.Accent"/>.
    /// </summary>
    public UIColorType ColorType { get; set; } = UIColorType.Accent;

    /// <summary>
    /// Gets the <see cref="Color"/> value based on the selected <see cref="ColorType"/>.
    /// </summary>
    public Color Value => AccentColorHelper.GetColor(ColorType);

    /// <summary>
    /// Returns the object that is set on the property where the markup extension is applied.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>A dynamic binding to the <see cref="Value"/> color property.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        SystemAccentChanged += OnSystemAccentChanged;

        var binding = new Binding(nameof(Value))
        {
            Source = this,
            Mode = BindingMode.OneWay
        };

        return binding.ProvideValue(serviceProvider);
    }

    /// <summary>
    /// Handles system accent color changes and notifies listeners of the updated <see cref="Value"/>.
    /// </summary>
    private void OnSystemAccentChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
    }
}
