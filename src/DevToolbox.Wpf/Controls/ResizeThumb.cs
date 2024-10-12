using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using DevToolbox.Wpf.Extensions;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a draggable control for resizing a <see cref="DesignLayer"/> element
/// within a <see cref="Canvas"/>.
/// </summary>
public class ResizeThumb : Thumb
{
    #region SizeAdorner

    /// <summary>
    /// Represents an adorner that displays resizing controls for a <see cref="DesignLayer"/>.
    /// </summary>
    private class SizeAdorner : Adorner
    {
        #region Fields/Consts

        private static readonly ComponentResourceKey SizeControlStyleKey = new ComponentResourceKey(typeof(ResizeThumb), "SizeControlStyle");

        private readonly Control _control;
        private readonly VisualCollection _visualCollection;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of visual children in the adorner.
        /// </summary>
        protected override int VisualChildrenCount => _visualCollection.Count;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SizeAdorner"/> class.
        /// </summary>
        /// <param name="adornerElement">The element to adorn.</param>
        public SizeAdorner(FrameworkElement adornerElement) : base(adornerElement)
        {
            _control = new();
            _control.SetResourceReference(StyleProperty, SizeControlStyleKey);
            _control.DataContext = adornerElement;
            _visualCollection = new(this);
            _visualCollection.Add(_control);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Retrieves the visual child at the specified index.
        /// </summary>
        /// <param name="index">The index of the child.</param>
        /// <returns>The visual child at the specified index.</returns>
        protected override Visual GetVisualChild(int index) => _visualCollection[index];

        /// <summary>
        /// Arranges the adorner's visual children.
        /// </summary>
        /// <param name="arrangeBounds">The size that the adorner should use to arrange its children.</param>
        /// <returns>The final size of the adorner.</returns>
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            _control.Arrange(new Rect(new Point(0.0, 0.0), arrangeBounds));
            return arrangeBounds;
        }

        #endregion
    }

    #endregion

    private Canvas? _canvas;
    private SizeAdorner? _sizeAdorner;
    private Point _transformOrigin;
    private double _angle;
    private RotateTransform? _rotateTransform;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResizeThumb"/> class.
    /// Sets up event handlers for drag operations.
    /// </summary>
    public ResizeThumb()
    {
        DragStarted += OnResizeDragStarted;
        DragDelta += new DragDeltaEventHandler(ResizeThumb_DragDelta);
        DragCompleted += OnResizeDragCompleted;
    }

    /// <summary>
    /// Event handler for the <see cref="Thumb.DragStarted"/> event.
    /// Initializes resizing parameters when the drag operation starts.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnResizeDragStarted(object sender, DragStartedEventArgs e)
    {
        if (DataContext is DesignLayer designerItem)
        {
            _canvas = (Canvas)VisualTreeHelper.GetParent(designerItem);

            if (_canvas != null)
            {
                _transformOrigin = designerItem.RenderTransformOrigin;

                _rotateTransform = designerItem.RenderTransform as RotateTransform;

                _angle = _rotateTransform != null ? _rotateTransform.Angle * Math.PI / 180.0 : 0.0d;

                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_canvas);
                if (adornerLayer != null)
                {
                    _sizeAdorner = new SizeAdorner(designerItem);
                    adornerLayer.Add(_sizeAdorner);
                }
            }
        }
    }

    /// <summary>
    /// Event handler for the <see cref="Thumb.DragDelta"/> event.
    /// Updates the size of the <see cref="DesignLayer"/> during the drag operation.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (DataContext is not DesignLayer designerItem) return;

        var designer = designerItem.InternalParent;

        if (designer != null && designerItem.IsSelected)
        {
            double dragDeltaVertical, dragDeltaHorizontal, scale;

            var selectedDesignerItems = designer.SelectedItems.Select(x => designer.ContainerFromItem(x) as DesignLayer).OfType<DesignLayer>();

            CalculateDragLimits(selectedDesignerItems, out double minLeft, out double minTop,
                                out double minDeltaHorizontal, out double minDeltaVertical);

            foreach (DesignLayer item in selectedDesignerItems)
            {
                if (item != null && item.ParentID == Guid.Empty)
                {
                    switch (VerticalAlignment)
                    {
                        case VerticalAlignment.Bottom:
                            dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                            scale = (item.ActualHeight - dragDeltaVertical) / item.ActualHeight;
                            DragBottom(scale, item, designer);
                            break;
                        case VerticalAlignment.Top:
                            double top = Canvas.GetTop(item);
                            dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
                            scale = (item.ActualHeight - dragDeltaVertical) / item.ActualHeight;
                            DragTop(scale, item, designer);
                            break;
                        default:
                            break;
                    }

                    switch (base.HorizontalAlignment)
                    {
                        case HorizontalAlignment.Left:
                            double left = Canvas.GetLeft(item);
                            dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                            scale = (item.ActualWidth - dragDeltaHorizontal) / item.ActualWidth;
                            DragLeft(scale, item, designer);
                            break;
                        case HorizontalAlignment.Right:
                            dragDeltaHorizontal = Math.Min(-e.HorizontalChange, minDeltaHorizontal);
                            scale = (item.ActualWidth - dragDeltaHorizontal) / item.ActualWidth;
                            DragRight(scale, item, designer);
                            break;
                        default:
                            break;
                    }
                }
            }
            e.Handled = true;
        }
    }

    /// <summary>
    /// Event handler for the <see cref="Thumb.DragCompleted"/> event.
    /// Cleans up the adorner when the drag operation completes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnResizeDragCompleted(object sender, DragCompletedEventArgs e)
    {
        if (_sizeAdorner != null)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_canvas);
            adornerLayer?.Remove(_sizeAdorner);

            _sizeAdorner = null;
        }
    }

    #region Helper methods

    /// <summary>
    /// Resizes the specified <see cref="DesignLayer"/> from the left.
    /// </summary>
    /// <param name="scale">The scaling factor for resizing.</param>
    /// <param name="item">The <see cref="DesignLayer"/> to resize.</param>
    /// <param name="selectionService">The parent canvas managing the design layers.</param>
    private void DragLeft(double scale, DesignLayer item, DesignCanvas selectionService)
    {
        IEnumerable<DesignLayer> groupItems = selectionService.GetGroupMembers(item).Cast<DesignLayer>();

        double groupLeft = Canvas.GetLeft(item) + item.Width;
        foreach (DesignLayer groupItem in groupItems)
        {
            double groupItemLeft = Canvas.GetLeft(groupItem);
            double delta = (groupLeft - groupItemLeft) * (scale - 1);
            Canvas.SetLeft(groupItem, groupItemLeft - delta);
            groupItem.Width = groupItem.ActualWidth * scale;
        }
    }

    /// <summary>
    /// Resizes the specified <see cref="DesignLayer"/> from the top.
    /// </summary>
    /// <param name="scale">The scaling factor for resizing.</param>
    /// <param name="item">The <see cref="DesignLayer"/> to resize.</param>
    /// <param name="selectionService">The parent canvas managing the design layers.</param>
    private void DragTop(double scale, DesignLayer item, DesignCanvas selectionService)
    {
        IEnumerable<DesignLayer> groupItems = selectionService.GetGroupMembers(item).Cast<DesignLayer>();
        double groupBottom = Canvas.GetTop(item) + item.Height;
        foreach (DesignLayer groupItem in groupItems)
        {
            double groupItemTop = Canvas.GetTop(groupItem);
            double delta = (groupBottom - groupItemTop) * (scale - 1);
            Canvas.SetTop(groupItem, groupItemTop - delta);
            groupItem.Height = groupItem.ActualHeight * scale;
        }
    }

    /// <summary>
    /// Resizes the specified <see cref="DesignLayer"/> from the right.
    /// </summary>
    /// <param name="scale">The scaling factor for resizing.</param>
    /// <param name="item">The <see cref="DesignLayer"/> to resize.</param>
    /// <param name="selectionService">The parent canvas managing the design layers.</param>
    private void DragRight(double scale, DesignLayer item, DesignCanvas selectionService)
    {
        IEnumerable<DesignLayer> groupItems = selectionService.GetGroupMembers(item).Cast<DesignLayer>();

        double groupLeft = Canvas.GetLeft(item);
        foreach (DesignLayer groupItem in groupItems)
        {
            double groupItemLeft = Canvas.GetLeft(groupItem);
            double delta = (groupItemLeft - groupLeft) * (scale - 1);

            Canvas.SetLeft(groupItem, groupItemLeft + delta);
            groupItem.Width = groupItem.ActualWidth * scale;
        }
    }

    /// <summary>
    /// Resizes the specified <see cref="DesignLayer"/> from the bottom.
    /// </summary>
    /// <param name="scale">The scaling factor for resizing.</param>
    /// <param name="item">The <see cref="DesignLayer"/> to resize.</param>
    /// <param name="selectionService">The parent canvas managing the design layers.</param>
    private void DragBottom(double scale, DesignLayer item, DesignCanvas selectionService)
    {
        IEnumerable<DesignLayer> groupItems = selectionService.GetGroupMembers(item).Cast<DesignLayer>();
        double groupTop = Canvas.GetTop(item);
        foreach (DesignLayer groupItem in groupItems)
        {
            double groupItemTop = Canvas.GetTop(groupItem);
            double delta = (groupItemTop - groupTop) * (scale - 1);

            Canvas.SetTop(groupItem, groupItemTop + delta);
            groupItem.Height = groupItem.ActualHeight * scale;
        }
    }

    /// <summary>
    /// Calculates the limits for resizing the selected <see cref="DesignLayer"/> items.
    /// </summary>
    /// <param name="selectedItems">The selected <see cref="DesignLayer"/> items.</param>
    /// <param name="minLeft">The minimum left position.</param>
    /// <param name="minTop">The minimum top position.</param>
    /// <param name="minDeltaHorizontal">The minimum horizontal delta for resizing.</param>
    /// <param name="minDeltaVertical">The minimum vertical delta for resizing.</param>
    private void CalculateDragLimits(IEnumerable<DesignLayer> selectedItems, out double minLeft, out double minTop, out double minDeltaHorizontal, out double minDeltaVertical)
    {
        minLeft = double.MaxValue;
        minTop = double.MaxValue;
        minDeltaHorizontal = double.MaxValue;
        minDeltaVertical = double.MaxValue;

        // Drag limits are set by these parameters: canvas top, canvas left, minHeight, minWidth
        // Calculate min value for each parameter for each item
        foreach (DesignLayer item in selectedItems)
        {
            double left = Canvas.GetLeft(item);
            double top = Canvas.GetTop(item);

            minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
            minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);

            minDeltaVertical = Math.Min(minDeltaVertical, item.ActualHeight - item.MinHeight);
            minDeltaHorizontal = Math.Min(minDeltaHorizontal, item.ActualWidth - item.MinWidth);
        }
    }

    #endregion
}
