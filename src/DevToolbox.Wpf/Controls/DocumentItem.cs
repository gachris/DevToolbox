using System.Windows;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A tab item representing a document in the <see cref="DockManager"/>'s document area.
/// Can optionally be converted into a dockable pane.
/// </summary>
public class DocumentItem : TabItemEdit
{
    #region Fields/Consts

    /// <summary>
    /// Defines the read-only <see cref="IsDockable"/> dependency property key.
    /// </summary>
    private static readonly DependencyPropertyKey IsDockablePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsDockable),
            typeof(bool),
            typeof(DocumentItem),
            new PropertyMetadata(false));

    /// <summary>
    /// Identifies the <see cref="IsDockable"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsDockableProperty = IsDockablePropertyKey.DependencyProperty;

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether this document can be undocked into its own pane.
    /// </summary>
    public bool IsDockable
    {
        get => (bool)GetValue(IsDockableProperty);
        internal set => SetValue(IsDockablePropertyKey, value);
    }

    #endregion

    static DocumentItem()
    {
        /// <summary>
        /// Overrides the default style key to use <see cref="DocumentItem"/>'s control template.
        /// </summary>
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DocumentItem),
            new FrameworkPropertyMetadata(typeof(DocumentItem)));
    }
}
