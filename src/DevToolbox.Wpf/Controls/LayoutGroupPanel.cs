using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Xml;
using DevToolbox.Wpf.Serialization;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A Grid-derived panel that manages and arranges multiple <see cref="LayoutItemsControl"/> instances
/// into a resizable layout, supporting both horizontal and vertical orientations.
/// </summary>
public class LayoutGroupPanel : Grid, ILayoutSerializable
{
    #region Fields/Consts

    private readonly List<LayoutItemsControl> _documentControls = new();
    private LayoutGroupItemsControl? _owner;

    /// <summary>
    /// Read-only backing key for the <see cref="Orientation"/> dependency property.
    /// </summary>
    private static readonly DependencyPropertyKey OrientationPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(Orientation),
            typeof(Orientation),
            typeof(LayoutGroupPanel),
            new FrameworkPropertyMetadata(default(Orientation)));

    /// <summary>
    /// Identifies the <see cref="Orientation"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty OrientationProperty = OrientationPropertyKey.DependencyProperty;

    /// <summary>
    /// Identifies the attached ColumnWidth dependency property for child elements.
    /// </summary>
    public static readonly DependencyProperty ColumnWidthProperty =
        DependencyProperty.RegisterAttached(
            "ColumnWidth",
            typeof(GridLength),
            typeof(LayoutGroupPanel),
            new PropertyMetadata(new GridLength(1, GridUnitType.Star), OnColumnWidthChanged));

    /// <summary>
    /// Identifies the attached MinColumnWidth dependency property for child elements.
    /// </summary>
    public static readonly DependencyProperty MinColumnWidthProperty =
        DependencyProperty.RegisterAttached(
            "MinColumnWidth",
            typeof(double),
            typeof(LayoutGroupPanel),
            new PropertyMetadata(100d, OnMinColumnWidthChanged));

    /// <summary>
    /// Identifies the attached MaxColumnWidth dependency property for child elements.
    /// </summary>
    public static readonly DependencyProperty MaxColumnWidthProperty =
        DependencyProperty.RegisterAttached(
            "MaxColumnWidth",
            typeof(double),
            typeof(LayoutGroupPanel),
            new PropertyMetadata(double.MaxValue, OnMaxColumnWidthChanged));

    /// <summary>
    /// Identifies the attached RowHeight dependency property for child elements.
    /// </summary>
    public static readonly DependencyProperty RowHeightProperty =
        DependencyProperty.RegisterAttached(
            "RowHeight",
            typeof(GridLength),
            typeof(LayoutGroupPanel),
            new PropertyMetadata(new GridLength(1, GridUnitType.Star), OnRowHeightChanged));

    /// <summary>
    /// Identifies the attached MinRowHeight dependency property for child elements.
    /// </summary>
    public static readonly DependencyProperty MinRowHeightProperty =
        DependencyProperty.RegisterAttached(
            "MinRowHeight",
            typeof(double),
            typeof(LayoutGroupPanel),
            new PropertyMetadata(100d, OnMinRowHeightChanged));

    /// <summary>
    /// Identifies the attached MaxRowHeight dependency property for child elements.
    /// </summary>
    public static readonly DependencyProperty MaxRowHeightProperty =
        DependencyProperty.RegisterAttached(
            "MaxRowHeight",
            typeof(double),
            typeof(LayoutGroupPanel),
            new PropertyMetadata(double.MaxValue, OnMaxRowHeightChanged));

    #endregion

    #region Propeties

    /// <summary>
    /// Gets the current orientation of the panel, determined by recent split additions.
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        internal set => SetValue(OrientationPropertyKey, value);
    }

    /// <summary>
    /// Gets a value indicating whether a new vertical split can be added.
    /// </summary>
    public bool CanAddVertical =>
        _documentControls.Count(x => !x.IsHidden) == 1
        || (_documentControls.Count(x => !x.IsHidden) > 1 && Orientation == Orientation.Vertical);

    /// <summary>
    /// Gets a value indicating whether a new horizontal split can be added.
    /// </summary>
    public bool CanAddHorizontal =>
        _documentControls.Count(x => !x.IsHidden) == 1
        || (_documentControls.Count(x => !x.IsHidden) > 1 && Orientation == Orientation.Horizontal);

    #endregion

    #region Methods Override

    /// <inheritdoc/>
    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        AttachToOwner();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Sets the column width for a child element.
    /// </summary>
    /// <param name="element">The child dependency object.</param>
    /// <param name="value">The <see cref="GridLength"/> to set.</param>
    public static void SetColumnWidth(DependencyObject element, GridLength value)
    {
        element.SetValue(ColumnWidthProperty, value);
    }

    /// <summary>
    /// Gets the column width for a child element.
    /// </summary>
    /// <param name="element">The child dependency object.</param>
    /// <returns>The current <see cref="GridLength"/>.</returns>
    public static GridLength GetColumnWidth(DependencyObject element)
    {
        return (GridLength)element.GetValue(ColumnWidthProperty);
    }

    /// <summary>
    /// Sets the minimum column width for a child element.
    /// </summary>
    /// <param name="element">The child dependency object.</param>
    /// <param name="value">The minimum width in device-independent units.</param>
    public static void SetMinColumnWidth(DependencyObject element, double value)
    {
        element.SetValue(MinColumnWidthProperty, value);
    }

    /// <summary>
    /// Gets the minimum column width for a child element.
    /// </summary>
    /// <param name="element">The child dependency object.</param>
    /// <returns>The minimum width in device-independent units.</returns>
    public static double GetMinColumnWidth(DependencyObject element)
    {
        return (double)element.GetValue(MinColumnWidthProperty);
    }

    /// <summary>
    /// Sets the maximum column width for a child element.
    /// </summary>
    /// <param name="element">The child dependency object.</param>
    /// <param name="value">The maximum width in device-independent units.</param>
    public static void SetMaxColumnWidth(DependencyObject element, double value)
    {
        element.SetValue(MaxColumnWidthProperty, value);
    }

    /// <summary>
    /// Gets the maximum column width for a child element.
    /// </summary>
    /// <param name="element">The child dependency object.</param>
    /// <returns>The maximum width in device-independent units.</returns>
    public static double GetMaxColumnWidth(DependencyObject element)
    {
        return (double)element.GetValue(MaxColumnWidthProperty);
    }

    /// <summary>
    /// Sets the row height for a child element.
    /// </summary>
    /// <param name="element">The child dependency object.</param>
    /// <param name="value">The <see cref="GridLength"/> to set.</param>
    public static void SetRowHeight(DependencyObject element, GridLength value)
    {
        element.SetValue(RowHeightProperty, value);
    }

    /// <summary>
    /// Gets the row height for a child element.
    /// </summary>
    /// <param name="element">The child dependency object.</param>
    /// <returns>The current <see cref="GridLength"/>.</returns>
    public static GridLength GetRowHeight(DependencyObject element)
    {
        return (GridLength)element.GetValue(RowHeightProperty);
    }

    /// <summary>
    /// Sets the minimum row height for a child element.
    /// </summary>
    /// <param name="element">The child dependency object.</param>
    /// <param name="value">The minimum height in device-independent units.</param>
    public static void SetMinRowHeight(DependencyObject element, double value)
    {
        element.SetValue(MinRowHeightProperty, value);
    }

    /// <summary>
    /// Gets the minimum row height for a child element.
    /// </summary>
    /// <param name="element">The child dependency object.</param>
    /// <returns>The minimum height in device-independent units.</returns>
    public static double GetMinRowHeight(DependencyObject element)
    {
        return (double)element.GetValue(MinRowHeightProperty);
    }

    /// <summary>
    /// Sets the maximum row height for a child element.
    /// </summary>
    /// <param name="element">The child dependency object.</param>
    /// <param name="value">The maximum height in device-independent units.</param>
    public static void SetMaxRowHeight(DependencyObject element, double value)
    {
        element.SetValue(MaxRowHeightProperty, value);
    }

    /// <summary>
    /// Gets the maximum row height for a child element.
    /// </summary>
    /// <param name="element">The child dependency object.</param>
    /// <returns>The maximum height in device-independent units.</returns>
    public static double GetMaxRowHeight(DependencyObject element)
    {
        return (double)element.GetValue(MaxRowHeightProperty);
    }

    /// <summary>
    /// Called when <see cref="ColumnWidthProperty"/> changes on a child element.
    /// Updates the corresponding column definition width.
    /// </summary>
    private static void OnColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        UpdateColumnDefinition(d, col => col.Width = (GridLength)e.NewValue);
    }

    /// <summary>
    /// Called when <see cref="MinColumnWidthProperty"/> changes on a child element.
    /// Updates the corresponding column definition minimum width.
    /// </summary>
    private static void OnMinColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        UpdateColumnDefinition(d, col => col.MinWidth = (double)e.NewValue);
    }

    /// <summary>
    /// Called when <see cref="MaxColumnWidthProperty"/> changes on a child element.
    /// Updates the corresponding column definition maximum width.
    /// </summary>
    private static void OnMaxColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        UpdateColumnDefinition(d, col => col.MaxWidth = (double)e.NewValue);
    }

    /// <summary>
    /// Called when <see cref="RowHeightProperty"/> changes on a child element.
    /// Updates the corresponding row definition height.
    /// </summary>
    private static void OnRowHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        UpdateRowDefinition(d, row => row.Height = (GridLength)e.NewValue);
    }

    /// <summary>
    /// Called when <see cref="MinRowHeightProperty"/> changes on a child element.
    /// Updates the corresponding row definition minimum height.
    /// </summary>
    private static void OnMinRowHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        UpdateRowDefinition(d, row => row.MinHeight = (double)e.NewValue);
    }

    /// <summary>
    /// Called when <see cref="MaxRowHeightProperty"/> changes on a child element.
    /// Updates the corresponding row definition maximum height.
    /// </summary>
    private static void OnMaxRowHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        UpdateRowDefinition(d, row => row.MaxHeight = (double)e.NewValue);
    }

    private static void UpdateColumnDefinition(DependencyObject child, Action<ColumnDefinition> updateAction)
    {
        if (VisualTreeHelper.GetParent(child) is not LayoutGroupPanel grid)
            return;

        var column = GetColumn((UIElement)child);
        if (column >= grid.ColumnDefinitions.Count)
            return;

        grid.Dispatcher.BeginInvoke(new Action(() => updateAction(grid.ColumnDefinitions[column])));
    }

    private static void UpdateRowDefinition(DependencyObject child, Action<RowDefinition> updateAction)
    {
        if (VisualTreeHelper.GetParent(child) is not LayoutGroupPanel grid)
            return;

        var row = GetRow((UIElement)child);
        if (row >= grid.RowDefinitions.Count)
            return;

        grid.Dispatcher.BeginInvoke(new Action(() => updateAction(grid.RowDefinitions[row])));
    }

    /// <summary>
    /// Rebuilds and arranges child <see cref="LayoutItemsControl"/> elements according to current orientation
    /// and splitter logic.
    /// </summary>
    internal void ArrangeLayout()
    {
        // 1) Clear out everything up-front
        ColumnDefinitions.Clear();
        RowDefinitions.Clear();
        Children.Clear();

        var visible = _documentControls
            .Where(c => !c.IsHidden)
            .ToArray();

        for (int i = 0; i < visible.Length; i++)
        {
            var child = visible[i];

            if (Orientation == Orientation.Horizontal)
            {
                // ——— 2) Add the child’s column ———
                ColumnDefinitions.Add(new ColumnDefinition
                {
                    MinWidth = GetMinColumnWidth(child),
                    Width = GetColumnWidth(child),
                    MaxWidth = GetMaxColumnWidth(child)
                });
                int childCol = ColumnDefinitions.Count - 1;
                SetColumn(child, childCol);
                Children.Add(child);

                // ——— 3) If it’s not the last item, add a splitter column + splitter ———
                if (i < visible.Length - 1)
                {
                    // make the splitter’s column auto-sized (so it's exactly as wide as the splitter)
                    ColumnDefinitions.Add(new ColumnDefinition
                    {
                        Width = GridLength.Auto
                    });
                    int splitterCol = ColumnDefinitions.Count - 1;

                    var splitter = new GridSplitter
                    {
                        // explicitly tell it you’re resizing columns
                        ResizeDirection = GridResizeDirection.Columns,
                        // resize both neighbors (left & right)
                        ResizeBehavior = GridResizeBehavior.PreviousAndNext,

                        // fill the entire cell so you can grab it anywhere
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,

                        // visible “thumb” width
                        Width = 5,
                        Background = Brushes.Transparent
                    };

                    SetColumn(splitter, splitterCol);
                    Children.Add(splitter);
                }
            }
            else
            {
                // same thing but for vertical (rows)
                RowDefinitions.Add(new RowDefinition
                {
                    MinHeight = GetMinRowHeight(child),
                    Height = GetRowHeight(child),
                    MaxHeight = GetMaxRowHeight(child)
                });
                int childRow = RowDefinitions.Count - 1;
                SetRow(child, childRow);
                Children.Add(child);

                if (i < visible.Length - 1)
                {
                    RowDefinitions.Add(new RowDefinition
                    {
                        Height = GridLength.Auto
                    });
                    int splitterRow = RowDefinitions.Count - 1;

                    var splitter = new GridSplitter
                    {
                        ResizeDirection = GridResizeDirection.Rows,
                        ResizeBehavior = GridResizeBehavior.PreviousAndNext,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Height = 5,
                        Background = Brushes.Transparent
                    };

                    SetRow(splitter, splitterRow);
                    Children.Add(splitter);
                }
            }
        }

        InvalidateArrange();
    }

    /// <summary>
    /// Inserts a new <see cref="LayoutItemsControl"/> at the end of the collection.
    /// </summary>
    /// <param name="element">The control to add.</param>
    internal void Add(LayoutItemsControl element)
    {
        _documentControls.Add(element);
        ArrangeLayout();
    }

    /// <summary>
    /// Inserts a new <see cref="LayoutItemsControl"/> adjacent to a relative element.
    /// </summary>
    /// <param name="element">The control to add.</param>
    /// <param name="relativeElement">Existing control to insert next to.</param>
    /// <param name="relativeDock">Dock position relative to <paramref name="relativeElement"/>.</param>
    /// <exception cref="NotSupportedException">
    /// Thrown if orientation change is not supported when more than one item exists.
    /// </exception>
    internal void Add(LayoutItemsControl element, LayoutItemsControl relativeElement, Dock relativeDock)
    {
        var orientation = relativeDock is Dock.Left or Dock.Right ? Orientation.Horizontal : Orientation.Vertical;
        if ((orientation is Orientation.Vertical && !CanAddVertical)
            || (orientation is Orientation.Horizontal && !CanAddHorizontal))
            throw new NotSupportedException("Orientation cannot change while items is more than one!");
        else Orientation = orientation;

        var relativeIndex = _documentControls.IndexOf(relativeElement);
        if (relativeIndex != -1)
        {
            switch (relativeDock)
            {
                case Dock.Left:
                case Dock.Top:
                    _documentControls.Insert(relativeIndex, element);
                    break;
                case Dock.Right:
                case Dock.Bottom:
                    _documentControls.Insert(relativeIndex + 1, element);
                    break;
                default:
                    break;
            }

            ArrangeLayout();
        }
    }

    /// <summary>
    /// Removes a <see cref="LayoutItemsControl"/> from the panel.
    /// </summary>
    /// <param name="element">The control to remove.</param>
    internal void Remove(LayoutItemsControl element)
    {
        _documentControls.Remove(element);
        ArrangeLayout();
    }

    /// <summary>
    /// Recursively clears all child grids and definitions.
    /// </summary>
    /// <param name="grid">The grid to clear.</param>
    private static void Clear(Grid grid)
    {
        foreach (UIElement child in grid.Children)
        {
            if (child is Grid childGrid)
                Clear(childGrid);
        }

        grid.Children.Clear();
        grid.ColumnDefinitions.Clear();
        grid.RowDefinitions.Clear();
    }

    /// <summary>
    /// Attaches to the templated parent <see cref="LayoutGroupItemsControl"/> and hooks event handlers.
    /// </summary>
    private void AttachToOwner()
    {
        if (_owner != null) _owner.ItemContainerGenerator.ItemsChanged -= ItemContainerGenerator_ItemsChanged;

        _owner = TemplatedParent as LayoutGroupItemsControl;

        if (_owner != null)
        {
            _owner.ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
            if (_owner.Items.Count > 0) _owner.Items.Refresh();
        }
    }

    /// <summary>
    /// Handles items changed events from the owner, regenerates containers and arranges layout.
    /// </summary>
    private void ItemContainerGenerator_ItemsChanged(object sender, ItemsChangedEventArgs e)
    {
        if (_owner?.ItemContainerGenerator is not IItemContainerGenerator generator) return;

        if (e.Action is NotifyCollectionChangedAction.Add)
        {
            var pos = generator.GeneratorPositionFromIndex(-1);
            using (generator.StartAt(pos, GeneratorDirection.Forward))
            {
                for (var i = 0; i < _owner.Items.Count; i++)
                {
                    var element = generator.GenerateNext(out bool isNewlyRealized);
                    if (isNewlyRealized) generator.PrepareItemContainer(element);
                }
            }
        }
        else if (e.Action is NotifyCollectionChangedAction.Reset)
        {
            var pos = generator.GeneratorPositionFromIndex(-1);
            using (generator.StartAt(pos, GeneratorDirection.Forward))
            {
                for (var i = 0; i < _owner.Items.Count; i++)
                {
                    var element = generator.GenerateNext(out bool isNewlyRealized);
                    if (isNewlyRealized) generator.PrepareItemContainer(element);
                }
            }
        }

        ArrangeLayout();
    }

    /// <inheritdoc/>
    public void Serialize(XmlDocument doc, XmlNode parentNode)
    {
    }

    /// <inheritdoc/>
    public void Deserialize(LayoutManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
    {
    }

    #endregion
}
