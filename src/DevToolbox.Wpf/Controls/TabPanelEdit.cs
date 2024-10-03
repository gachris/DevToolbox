using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A customizable tab panel that supports different view modes for tab arrangement,
/// including scrolling and stretching of tab items.
/// </summary>
public partial class TabPanelEdit : TabPanel
{
    #region Fields/Consts

    private double _rowHeight; // Height of the current row of tabs.
    private double _scaleFactor; // Scale factor for tab sizing.
    private double _totChildWidth = 0.0; // Total width of all child elements.
    private double _itemWidth = 0.0; // Width of each individual item.

    /// <summary>
    /// Dependency property for specifying the view mode of the tab panel.
    /// </summary>
    public static readonly DependencyProperty TabPanelViewModeProperty =
        DependencyProperty.Register("TabPanelViewMode", typeof(TabPanelViewMode), typeof(TabPanelEdit), new FrameworkPropertyMetadata((d, e) => (d as TabPanelEdit)?.OnTabPanelViewModeChanged(e)));

    #endregion

    #region Properties

    /// <summary>
    /// Gets the ItemsControl that owns this TabPanelEdit (e.g. a TabControl).
    /// </summary>
    protected ItemsControl ItemsControl => ItemsControl.GetItemsOwner(this);

    /// <summary>
    /// Gets or sets the view mode of the tab panel, determining how the tabs are arranged.
    /// </summary>
    public TabPanelViewMode TabPanelViewMode
    {
        get => (TabPanelViewMode)GetValue(TabPanelViewModeProperty);
        set => SetValue(TabPanelViewModeProperty, value);
    }

    #endregion

    /// <summary>
    /// Static constructor to override default keyboard navigation settings for the TabPanelEdit.
    /// </summary>
    static TabPanelEdit()
    {
        KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(TabPanelEdit), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
        KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(TabPanelEdit), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
    }

    /// <summary>
    /// Initializes a new instance of the TabPanelEdit class.
    /// </summary>
    public TabPanelEdit()
    {
    }

    #region Methods Override

    /// <summary>
    /// Measures the size required for child elements in the panel, considering the specified view mode.
    /// </summary>
    /// <param name="availableSize">The size available for child elements.</param>
    /// <returns>The size required for the children.</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        var tabControl = ItemsControl as TabControl;

        // Handling scrolling view mode
        if (TabPanelViewMode == TabPanelViewMode.Scroll && (tabControl?.TabStripPlacement == Dock.Top || tabControl?.TabStripPlacement == Dock.Bottom))
        {
            var width = 0.0;
            var rowWidth = 0.0;

            _rowHeight = 0.0;

            // Measure each child and calculate total width and height
            foreach (UIElement element in Children)
            {
                element.Measure(availableSize);
                Size size = GetDesiredSizeLessMargin(element);
                _rowHeight = Math.Max(_rowHeight, size.Height);
                rowWidth = Math.Max(rowWidth, size.Width);
                width += size.Width;
            }

            // Scale down the tabs if they exceed the available width
            if (width > availableSize.Width)
            {
                _scaleFactor = Children[0] is TabItem contentTabItem
                    ? (availableSize.Width - contentTabItem.DesiredSize.Width) / width
                    : availableSize.Width / width;

                width = 0.0;

                foreach (UIElement element in Children)
                {
                    if (element is not TabItem)
                        element.Measure(new Size(element.DesiredSize.Width * _scaleFactor, availableSize.Height));
                    else
                        element.Measure(new Size(element.DesiredSize.Width, availableSize.Height));

                    width += element.DesiredSize.Width;
                }
            }
            else _scaleFactor = 1.0;

            return new Size(width, _rowHeight);
        }
        // Handling stretch view mode
        else if (TabPanelViewMode == TabPanelViewMode.Stretch && (tabControl?.TabStripPlacement == Dock.Top || tabControl?.TabStripPlacement == Dock.Bottom))
        {
            var i = 0;
            var rowHeight = 0.0;
            var minRowWidth = 0.0;
            var maxRowWidth = 0.0;
            var width = 0.0;

            _totChildWidth = 0.0;
            foreach (UIElement element in Children.Cast<UIElement>().OrderBy(x => x.DesiredSize.Width))
            {
                element.Measure(availableSize);
                var size = GetDesiredSizeLessMargin(element);

                if (minRowWidth == 0)
                    minRowWidth = size.Width;

                _totChildWidth += size.Width;

                if (availableSize.Width > _totChildWidth && _itemWidth > size.Width)
                {
                    maxRowWidth = Math.Max(maxRowWidth, size.Width);
                    width += size.Width;
                    i++;
                }
            }

            _itemWidth = (availableSize.Width - width) / (Children.Count - i);

            // Measure elements again considering the new item width
            foreach (UIElement element in Children)
            {
                var size = GetDesiredSizeLessMargin(element);
                if (availableSize.Width < _totChildWidth)
                {
                    if (size.Width >= _itemWidth) element.Measure(new Size(_itemWidth, availableSize.Height));
                }
                else element.Measure(availableSize);
            }

            _totChildWidth = 0.0;

            // Calculate the total height required after measurement
            foreach (UIElement element in Children)
            {
                var size = GetDesiredSizeLessMargin(element);
                _totChildWidth += size.Width;
                rowHeight = Math.Max(rowHeight, size.Height);
            }

            return new Size(_totChildWidth, rowHeight);
        }
        else return base.MeasureOverride(availableSize);
    }

    /// <summary>
    /// Arranges child elements within the specified size, depending on the view mode.
    /// </summary>
    /// <param name="arrangeSize">The size available for the children.</param>
    /// <returns>The arranged size of the children.</returns>
    protected override Size ArrangeOverride(Size arrangeSize)
    {
        var tabControl = ItemsControl as TabControl;

        // Arranging children in scroll view mode
        if (TabPanelViewMode == TabPanelViewMode.Scroll && (tabControl?.TabStripPlacement == Dock.Top || tabControl?.TabStripPlacement == Dock.Bottom))
        {
            var point = new Point();
            foreach (UIElement element in Children)
            {
                var size1 = element.DesiredSize;
                var size2 = GetDesiredSizeLessMargin(element);
                var margin = (Thickness)element.GetValue(FrameworkElement.MarginProperty);
                var width = size2.Width;

                if (element.DesiredSize.Width != size2.Width)
                    width = arrangeSize.Width - point.X;

                element.Arrange(new Rect(point, new Size(Math.Min(width, size2.Width), _rowHeight)));

                var leftRightMargin = Math.Max(0.0, -(margin.Left + margin.Right));
                point.X += size1.Width + (leftRightMargin * _scaleFactor);
            }

            return arrangeSize;
        }
        // Arranging children in stretch view mode
        else if (TabPanelViewMode == TabPanelViewMode.Stretch && (tabControl?.TabStripPlacement == Dock.Top || tabControl?.TabStripPlacement == Dock.Bottom))
        {
            var inflate = new Size();

            if (arrangeSize.Width < _totChildWidth)
                inflate.Width = (_totChildWidth - arrangeSize.Width) / InternalChildren.Count;

            var offset = new Point();
            if (arrangeSize.Width > _totChildWidth)
                offset.X = -(arrangeSize.Width - _totChildWidth) / 2;

            var totalFinalWidth = 0.0;
            foreach (UIElement child in InternalChildren)
            {
                var childFinalSize = GetDesiredSizeLessMargin(child);
                childFinalSize.Width -= inflate.Width;
                childFinalSize.Height = arrangeSize.Height;

                child.Arrange(new Rect(offset, childFinalSize));

                offset.Offset(childFinalSize.Width, 0);
                totalFinalWidth += childFinalSize.Width;
            }

            return new Size(totalFinalWidth, arrangeSize.Height);
        }
        else return base.ArrangeOverride(arrangeSize);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Called when the TabPanelViewMode property changes to invalidate the measure.
    /// </summary>
    /// <param name="e">Event arguments containing old and new values.</param>
    private void OnTabPanelViewModeChanged(DependencyPropertyChangedEventArgs e) => InvalidateMeasure();

    /// <summary>
    /// Gets the desired size of a UI element without its margins.
    /// </summary>
    /// <param name="element">The UI element to measure.</param>
    /// <returns>The size of the element without its margins.</returns>
    private static Size GetDesiredSizeLessMargin(UIElement element)
    {
        var margin = (Thickness)element.GetValue(MarginProperty);
        return new Size
        {
            Height = Math.Max(0.0, element.DesiredSize.Height - (margin.Top + margin.Bottom)),
            Width = Math.Max(0.0, element.DesiredSize.Width - (margin.Left + margin.Right))
        };
    }

    #endregion
}
