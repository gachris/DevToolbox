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
/// Markup extension that provides a <see cref="SolidColorBrush"/> based on the current system accent color
/// and a specified <see cref="UIColorType"/> (e.g., AccentLight2, AccentDark3).
/// Automatically updates the brush when the system accent color changes.
/// </summary>
[MarkupExtensionReturnType(typeof(Brush))]
public class SystemBrushExtension : MarkupExtension, INotifyPropertyChanged
{
    /// <summary>
    /// Occurs when the system accent color changes. Used to notify all instances of this extension.
    /// </summary>
    public static event Action? SystemAccentChanged;

    /// <summary>
    /// Occurs when a property value changes, enabling data binding updates.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemBrushExtension"/> class.
    /// </summary>
    public SystemBrushExtension() { }

    static SystemBrushExtension()
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
    /// The default is <see cref="UIColorType.Accent"/>.
    /// </summary>
    public UIColorType ColorType { get; set; } = UIColorType.Accent;

    /// <summary>
    /// Gets the <see cref="Brush"/> corresponding to the current system accent color and selected <see cref="ColorType"/>.
    /// </summary>
    public Brush Value => new SolidColorBrush(AccentColorHelper.GetColor(ColorType));

    /// <summary>
    /// Returns an object that is set on the property where the markup extension is applied.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>A dynamically updating binding to the accent color brush.</returns>
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
    /// Called when the system accent color changes to raise the <see cref="PropertyChanged"/> event for <see cref="Value"/>.
    /// </summary>
    private void OnSystemAccentChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
    }
}
