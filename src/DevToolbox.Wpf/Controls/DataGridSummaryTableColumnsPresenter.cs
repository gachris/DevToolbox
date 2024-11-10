using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DevToolbox.Wpf.Extensions;
using DevToolbox.Wpf.Utils;

namespace DevToolbox.Wpf.Controls;

internal class DataGridSummaryTableColumnsPresenter : VirtualizingStackPanel
{
    #region Fields/Consts

    private ScrollBar? _horizontalScrollBar;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the owner of the DataGridRowsPresenter, cast as a GridControl.
    /// </summary>
    private GridControl? Owner => this?.FindVisualAncestor<GridControl>();

    /// <summary>
    /// Gets the list of realized (visible) child elements in the VirtualizingStackPanel.
    /// </summary>
    private IList? GetRealizedChildren => typeof(VirtualizingStackPanel).InvokeMember(
        "RealizedChildren",
        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty,
        null, this, null) as IList;

    /// <summary>
    /// Gets the vertical ScrollBar used in the DataGrid.
    /// </summary>
    private ScrollBar? HorizontalScrollBar
    {
        get
        {
            if (_horizontalScrollBar == null)
            {
                _horizontalScrollBar = Owner?.FindVisualChildByName<ScrollBar>("PART_HorizontalScrollBar");
                _horizontalScrollBar?.UpdateLayout();
            }
            return _horizontalScrollBar;
        }
    }

    #endregion

    #region Methods Overrides

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        if (Owner == null || Owner.FrozenColumnCount <= 0)
        {
            return base.ArrangeOverride(arrangeSize);
        }

        var rcChild = new Rect(arrangeSize);
        var frozenColumnCount = Owner.FrozenColumnCount;
        var nextFrozenColumnStart = 0.0;
        var nextNonFrozenColumnStart = 0.0;

        // Determine horizontal offset and physical offset for arranging children
        double physicalOffset = 0.0;
        var originPoint = new Point(0, 0);

        if (this is IScrollInfo scrollInfo)
        {
            var horizontalOffset = scrollInfo.HorizontalOffset;
            physicalOffset = -ComputePhysicalFromLogicalOffset(horizontalOffset, true);
            rcChild.Y = -scrollInfo.VerticalOffset;
        }

        var sbOffset = HorizontalScrollBar?.TransformToAncestor(Owner)?.Transform(originPoint).X ?? 0;
        var rowsPanelOffset = TransformToAncestor(Owner)?.Transform(originPoint).X ?? 0;
        var viewportStartX = sbOffset - rowsPanelOffset;
        nextNonFrozenColumnStart = viewportStartX - physicalOffset;

        // Arrange children
        var children = GetRealizedChildren ?? new List<UIElement>();
        for (int i = 0; i < children.Count; ++i)
        {
            if (children[i] is not UIElement container)
                continue;

            var childSize = container.DesiredSize;
            rcChild.Height = arrangeSize.Height;
            rcChild.Width = childSize.Width;

            if (i < frozenColumnCount)
            {
                // Arrange frozen columns
                rcChild.X = nextFrozenColumnStart;
                nextFrozenColumnStart += childSize.Width;
            }
            else
            {
                // Arrange non-frozen columns
                if (DoubleUtil.LessThan(nextNonFrozenColumnStart, nextFrozenColumnStart))
                {
                    var columnChoppedWidth = nextFrozenColumnStart - nextNonFrozenColumnStart;
                    if (DoubleUtil.AreClose(columnChoppedWidth, 0.0))
                    {
                        nextNonFrozenColumnStart = nextFrozenColumnStart + childSize.Width;
                    }
                    else
                    {
                        var clipWidth = childSize.Width - columnChoppedWidth;
                        nextNonFrozenColumnStart = nextFrozenColumnStart + clipWidth;
                    }
                    rcChild.X = nextFrozenColumnStart;
                }
                else
                {
                    rcChild.X = nextNonFrozenColumnStart;
                    nextNonFrozenColumnStart += childSize.Width;
                }
            }

            container.Arrange(rcChild);
        }

        return arrangeSize;
    }

    /// <summary>
    /// Called when the viewport offset changes, typically due to scrolling.
    /// This method invalidates the arrangement if the vertical offset has changed.
    /// </summary>
    /// <param name="oldViewportOffset">The old viewport offset.</param>
    /// <param name="newViewportOffset">The new viewport offset.</param>
    protected override void OnViewportOffsetChanged(Vector oldViewportOffset, Vector newViewportOffset)
    {
        base.OnViewportOffsetChanged(oldViewportOffset, newViewportOffset);

        if (!DoubleUtil.AreClose(oldViewportOffset.Y, newViewportOffset.Y))
        {
            InvalidateArrange();
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Computes the physical offset from the logical offset.
    /// This is used to convert logical scrolling positions to physical pixel offsets.
    /// </summary>
    /// <param name="logicalOffset">The logical offset (e.g., the scroll position).</param>
    /// <param name="fHorizontal">Indicates whether the offset is for horizontal scrolling.</param>
    /// <returns>The computed physical offset in pixels.</returns>
    private double ComputePhysicalFromLogicalOffset(double logicalOffset, bool fHorizontal)
    {
        var physicalOffset = 0.0;
        var children = GetRealizedChildren ?? new List<UIElement>();

        for (var i = 0; i < Math.Min(logicalOffset, children.Count); i++)
        {
            if (children[i] is UIElement child)
            {
                physicalOffset -= fHorizontal ? child.DesiredSize.Width : child.DesiredSize.Height;
            }
        }

        return physicalOffset;
    }

    #endregion
}