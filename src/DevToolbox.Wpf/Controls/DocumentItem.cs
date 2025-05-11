using System.Windows;

namespace DevToolbox.Wpf.Controls;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class DocumentItem : TabItemEdit
{
    #region Fields/Consts

    private static readonly DependencyPropertyKey IsDockablePropertyKey = 
        DependencyProperty.RegisterReadOnly(nameof(IsDockable), typeof(bool), typeof(DocumentItem), new PropertyMetadata(false));

    public static readonly DependencyProperty IsDockableProperty = IsDockablePropertyKey.DependencyProperty;

    #endregion

    #region Properties

    public bool IsDockable
    {
        get => (bool)GetValue(IsDockableProperty);
        internal set => SetValue(IsDockablePropertyKey, value);
    }

    #endregion

    static DocumentItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DocumentItem), new FrameworkPropertyMetadata(typeof(DocumentItem)));
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
