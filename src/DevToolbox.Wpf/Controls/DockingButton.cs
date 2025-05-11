using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DevToolbox.Wpf.Controls;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class DockingButton : Button
{
    #region Fields/Consts

    private static readonly RoutedUICommand _showHideOverlay = new(nameof(ShowHideOverlayCommand), nameof(ShowHideOverlayCommand), typeof(DockingButton));

    private DockingButtonGroupItem? _parentGroupItem;

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(ImageSource), typeof(DockingButton), new FrameworkPropertyMetadata(default));

    private static readonly DependencyPropertyKey DockPropertyKey = 
        DependencyProperty.RegisterReadOnly(nameof(Dock), typeof(Dock), typeof(DockingButton), new FrameworkPropertyMetadata(default(Dock)));

    public static readonly DependencyProperty DockProperty = DockPropertyKey.DependencyProperty;

    #endregion

    #region Properties

    public static RoutedUICommand ShowHideOverlayCommand => _showHideOverlay;

    public Dock Dock
    {
        get => (Dock)GetValue(DockProperty);
        private set => SetValue(DockPropertyKey, value);
    }

    public ImageSource Icon
    {
        get => (ImageSource)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public int AssociatedItemIdex { get; internal set; }

    public DockableControl? AssociatedContainer { get; internal set; }

    public DockingButtonGroupItem? ParentGroupItem
    {
        get => _parentGroupItem;
        internal set
        {
            _parentGroupItem = value;
            Dock = _parentGroupItem?.Dock ?? (Dock)(-1);
        }
    }

    #endregion

    static DockingButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DockingButton), new FrameworkPropertyMetadata(typeof(DockingButton)));
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member