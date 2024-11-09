using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DevToolbox.Wpf.Data;
using DevToolbox.Wpf.Extensions;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Provides data that should be useful to templates displaying
/// a preview.
/// </summary>
internal class ScrollingPreviewData : NotifyPropertyChanged
{
    #region Fields/Consts

    private double _offset;
    private double _viewport;
    private double _extent;
    private object? _firstItem;
    private object? _lastItem;

    #endregion

    #region Properties

    /// <summary>
    /// The ScrollBar's offset.
    /// </summary>
    public double Offset
    {
        get => _offset;
        internal set => SetProperty(ref _offset, value);
    }

    /// <summary>
    /// The size of the current viewport.
    /// </summary>
    public double Viewport
    {
        get => _viewport;
        internal set => SetProperty(ref _viewport, value);
    }

    /// <summary>
    /// The entire scrollable range.
    /// </summary>
    public double Extent
    {
        get => _extent;
        internal set => SetProperty(ref _extent, value);
    }

    /// <summary>
    ///     The first visible item in the viewport.
    /// </summary>
    public object? FirstItem
    {
        get => _firstItem;
        private set => SetProperty(ref _firstItem, value);
    }

    /// <summary>
    /// The last visible item in the viewport.
    /// </summary>
    public object? LastItem
    {
        get => _lastItem;
        private set => SetProperty(ref _lastItem, value);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Updates Offset, Viewport, and Extent.
    /// </summary>
    public void UpdateScrollingValues(ScrollBar scrollBar)
    {
        Offset = scrollBar.Value;
        Viewport = scrollBar.ViewportSize;
        Extent = scrollBar.Maximum - scrollBar.Minimum;
        var parentGrid = scrollBar.FindVisualAncestor<ItemsControl>();
        if (parentGrid != null)
        {
            UpdateItem(parentGrid, true);
        }
    }

    /// <summary>
    /// Updates FirstItem and LastItem based on the
    /// Offset and Viewport properties.
    /// </summary>
    /// <param name="itemsControl">The ItemsControl that contains the data items.</param>
    /// <param name="vertical">vertical </param>
    public void UpdateItem(ItemsControl itemsControl, bool vertical)
    {
        if (itemsControl is null)
        {
            return;
        }

        var numItems = itemsControl.Items.Count;
        if (numItems > 0)
        {
            if (VirtualizingPanel.GetIsVirtualizing(itemsControl))
            {
                var firstIndex = (int)_offset;
                var lastIndex = (int)_offset + (int)_viewport - 1;

                FirstItem = firstIndex >= 0 && firstIndex < numItems ? itemsControl.Items[firstIndex] : null;
                LastItem = lastIndex >= 0 && lastIndex < numItems ? itemsControl.Items[lastIndex] : null;
            }
            else
            {
                ScrollContentPresenter? scp = null;
                var foundFirstItem = false;
                var bestLastItemIndex = -1;
                object? firstVisibleItem = null;
                object? lastVisibleItem = null;

                for (var i = 0; i < numItems; i++)
                {
                    if (itemsControl.ItemContainerGenerator.ContainerFromIndex(i) is UIElement child)
                    {
                        if (scp == null)
                        {
                            scp = child.FindVisualAncestor<ScrollContentPresenter>();
                            if (scp == null)
                            {
                                return;
                            }
                        }

                        var t = child.TransformToAncestor(scp);
                        var p = t.Transform(foundFirstItem ? new Point(child.RenderSize.Width, child.RenderSize.Height) : new Point());

                        if (!foundFirstItem && (vertical ? p.Y : p.X) >= 0.0)
                        {
                            firstVisibleItem = itemsControl.Items[i];
                            bestLastItemIndex = i;
                            foundFirstItem = true;
                        }
                        else if (foundFirstItem && (vertical ? p.Y : p.X) < scp.ActualHeight)
                        {
                            bestLastItemIndex = i;
                        }
                    }
                }

                if (bestLastItemIndex >= 0)
                {
                    lastVisibleItem = itemsControl.Items[bestLastItemIndex];
                }

                FirstItem = firstVisibleItem;
                LastItem = lastVisibleItem;
            }
        }
    }

    #endregion
}