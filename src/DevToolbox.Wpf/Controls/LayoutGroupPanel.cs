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

public class LayoutGroupPanel : Grid, ILayoutSerializable
{
    #region Fields/Consts

    private readonly List<LayoutItemsControl> _documentControls = [];

    private LayoutGroupItemsControl? _owner;

    private static readonly DependencyPropertyKey OrientationPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(Orientation), typeof(Orientation), typeof(LayoutGroupPanel), new FrameworkPropertyMetadata(default(Orientation)));

    public static readonly DependencyProperty OrientationProperty = OrientationPropertyKey.DependencyProperty;

    public static readonly DependencyProperty ColumnWidthProperty =
        DependencyProperty.RegisterAttached("ColumnWidth", typeof(GridLength), typeof(LayoutGroupPanel), new PropertyMetadata(new GridLength(1, GridUnitType.Star), OnColumnWidthChanged));

    public static readonly DependencyProperty MinColumnWidthProperty =
        DependencyProperty.RegisterAttached("MinColumnWidth", typeof(double), typeof(LayoutGroupPanel), new PropertyMetadata(100d, OnMinColumnWidthChanged));

    public static readonly DependencyProperty MaxColumnWidthProperty =
        DependencyProperty.RegisterAttached("MaxColumnWidth", typeof(double), typeof(LayoutGroupPanel), new PropertyMetadata(double.MaxValue, OnMaxColumnWidthChanged));

    public static readonly DependencyProperty RowHeightProperty =
    DependencyProperty.RegisterAttached("RowHeight", typeof(GridLength), typeof(LayoutGroupPanel), new PropertyMetadata(new GridLength(1, GridUnitType.Star), OnRowHeightChanged));

    public static readonly DependencyProperty MinRowHeightProperty =
        DependencyProperty.RegisterAttached("MinRowHeight", typeof(double), typeof(LayoutGroupPanel), new PropertyMetadata(100d, OnMinRowHeightChanged));

    public static readonly DependencyProperty MaxRowHeightProperty =
        DependencyProperty.RegisterAttached("MaxRowHeight", typeof(double), typeof(LayoutGroupPanel), new PropertyMetadata(double.MaxValue, OnMaxRowHeightChanged));

    #endregion

    #region Propeties

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        internal set => SetValue(OrientationPropertyKey, value);
    }

    public bool CanAddVertical =>
        _documentControls.Count(x => !x.IsHidden) == 1
        || (_documentControls.Count(x => !x.IsHidden) > 1 && Orientation == Orientation.Vertical);

    public bool CanAddHorizontal =>
        _documentControls.Count(x => !x.IsHidden) == 1
        || (_documentControls.Count(x => !x.IsHidden) > 1 && Orientation == Orientation.Horizontal);

    #endregion

    #region Methods Override

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        AttachToOwner();
    }

    #endregion

    #region Methods

    public static void SetColumnWidth(DependencyObject element, GridLength value)
    {
        element.SetValue(ColumnWidthProperty, value);
    }

    public static GridLength GetColumnWidth(DependencyObject element)
    {
        return (GridLength)element.GetValue(ColumnWidthProperty);
    }

    public static void SetMinColumnWidth(DependencyObject element, double value)
    {
        element.SetValue(MinColumnWidthProperty, value);
    }

    public static double GetMinColumnWidth(DependencyObject element)
    {
        return (double)element.GetValue(MinColumnWidthProperty);
    }

    public static void SetMaxColumnWidth(DependencyObject element, double value)
    {
        element.SetValue(MaxColumnWidthProperty, value);
    }

    public static double GetMaxColumnWidth(DependencyObject element)
    {
        return (double)element.GetValue(MaxColumnWidthProperty);
    }

    public static void SetRowHeight(DependencyObject element, GridLength value)
    {
        element.SetValue(RowHeightProperty, value);
    }

    public static GridLength GetRowHeight(DependencyObject element)
    {
        return (GridLength)element.GetValue(RowHeightProperty);
    }

    public static void SetMinRowHeight(DependencyObject element, double value)
    {
        element.SetValue(MinRowHeightProperty, value);
    }

    public static double GetMinRowHeight(DependencyObject element)
    {
        return (double)element.GetValue(MinRowHeightProperty);
    }

    public static void SetMaxRowHeight(DependencyObject element, double value)
    {
        element.SetValue(MaxRowHeightProperty, value);
    }

    public static double GetMaxRowHeight(DependencyObject element)
    {
        return (double)element.GetValue(MaxRowHeightProperty);
    }

    private static void OnColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        UpdateColumnDefinition(d, column => column.Width = (GridLength)e.NewValue);
    }

    private static void OnMinColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        UpdateColumnDefinition(d, column => column.MinWidth = (double)e.NewValue);
    }

    private static void OnMaxColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        UpdateColumnDefinition(d, column => column.MaxWidth = (double)e.NewValue);
    }

    private static void OnRowHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        UpdateRowDefinition(d, row => row.Height = (GridLength)e.NewValue);
    }

    private static void OnMinRowHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        UpdateRowDefinition(d, row => row.MinHeight = (double)e.NewValue);
    }

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

    internal void Add(LayoutItemsControl element)
    {
        _documentControls.Add(element);
        ArrangeLayout();
    }

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

    internal void Remove(LayoutItemsControl element)
    {
        _documentControls.Remove(element);
        ArrangeLayout();
    }

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

    private ContentPresenter GenerateContainer(object item)
    {
        return new() { Content = item };
    }

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
