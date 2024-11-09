using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DevToolbox.Wpf.Controls.Utils;
using DevToolbox.Wpf.Extensions;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Extends <see cref="DataGridRowsPresenter"/> with support for frozen rows.
/// Manages the layout of both frozen and scrollable rows, keeping frozen rows anchored
/// at the top of the viewport while allowing non-frozen rows to scroll normally within a <see cref="GridControl"/>.
/// </summary>
internal class GridControlRowsPresenter : DataGridRowsPresenter
{
    #region Fields/Consts

    private ScrollBar? _verticalScrollBar;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the owner of the DataGridRowsPresenter, cast as a GridControl.
    /// </summary>
    private GridControl? Owner => TemplatedParent is FrameworkElement itemsPresenter ? itemsPresenter.TemplatedParent as GridControl : null;

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
    private ScrollBar? VerticalScrollBar
    {
        get
        {
            if (_verticalScrollBar == null)
            {
                _verticalScrollBar = Owner?.FindVisualChildByName<ScrollBar>("PART_VerticalScrollBar");
                _verticalScrollBar?.UpdateLayout();
            }
            return _verticalScrollBar;
        }
    }

    #endregion

    #region Methods Overrides

    /// <summary>
    /// Arranges the child elements within the DataGridRowsPresenter.
    /// This method handles the layout for both frozen and non-frozen rows.
    /// </summary>
    /// <param name="arrangeSize">The size that the DataGridRowsPresenter should use to arrange its children.</param>
    /// <returns>The actual size used to arrange the children.</returns>
    protected override Size ArrangeOverride(Size arrangeSize)
    {
        if (Owner == null || Owner.FrozenRowCount <= 0)
        {
            return base.ArrangeOverride(arrangeSize);
        }

        var rcChild = new Rect(arrangeSize);
        var frozenRowCount = Owner.FrozenRowCount;
        var nextFrozenRowStart = 0.0;
        var nextNonFrozenRowStart = 0.0;
        var dataGridVerticalScrollStartY = 0.0;

        // Determine vertical offset and physical offset for arranging children
        double physicalOffset = 0.0;
        var originPoint = new Point(0, 0);

        if (this is IScrollInfo scrollInfo)
        {
            var verticalOffset = scrollInfo.VerticalOffset;
            physicalOffset = -ComputePhysicalFromLogicalOffset(verticalOffset, false);
            rcChild.X = -scrollInfo.HorizontalOffset;
        }

        var sbOffset = VerticalScrollBar?.TransformToAncestor(Owner).Transform(originPoint).Y ?? 0;
        var rowsPanelOffset = TransformToAncestor(Owner).Transform(originPoint).Y;
        var viewportStartY = sbOffset - rowsPanelOffset;
        nextNonFrozenRowStart = viewportStartY - physicalOffset;

        // Arrange children
        var children = GetRealizedChildren ?? new List<UIElement>();
        for (int i = 0; i < children.Count; ++i)
        {
            if (children[i] is not UIElement container)
                continue;

            var childSize = container.DesiredSize;
            rcChild.Height = childSize.Height;
            rcChild.Width = Math.Max(arrangeSize.Width, childSize.Width);

            if (i < frozenRowCount)
            {
                // Arrange frozen rows
                rcChild.Y = nextFrozenRowStart;
                nextFrozenRowStart += childSize.Height;
                dataGridVerticalScrollStartY += childSize.Height;
            }
            else
            {
                // Arrange non-frozen rows
                if (DoubleUtil.LessThan(nextNonFrozenRowStart, nextFrozenRowStart))
                {
                    var rowChoppedHeight = nextFrozenRowStart - nextNonFrozenRowStart;
                    if (DoubleUtil.AreClose(rowChoppedHeight, 0.0))
                    {
                        nextNonFrozenRowStart = nextFrozenRowStart + childSize.Height;
                    }
                    else
                    {
                        var clipHeight = childSize.Height - rowChoppedHeight;
                        nextNonFrozenRowStart = nextFrozenRowStart + clipHeight;
                    }
                    rcChild.Y = nextFrozenRowStart;
                }
                else
                {
                    rcChild.Y = nextNonFrozenRowStart;
                    nextNonFrozenRowStart += childSize.Height;
                }
            }

            container.Arrange(rcChild);
        }

        // Update the vertical offset for non-frozen rows
        Owner.NonFrozenRowsViewportVerticalOffset = dataGridVerticalScrollStartY;

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