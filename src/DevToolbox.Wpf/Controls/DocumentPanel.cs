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

public class DocumentPanel : Grid, ILayoutSerializable
{
    #region Fields/Consts

    private readonly IList<DocumentControl> _documentControls = new List<DocumentControl>();

    private DocumentList? _owner;

    private static readonly DependencyPropertyKey OrientationPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(Orientation), typeof(Orientation), typeof(DocumentPanel), new FrameworkPropertyMetadata(default(Orientation)));

    public static readonly DependencyProperty OrientationProperty = OrientationPropertyKey.DependencyProperty;

    public static readonly DependencyProperty ColumnWidthProperty =
        DependencyProperty.RegisterAttached("ColumnWidth", typeof(GridLength), typeof(DocumentPanel), new PropertyMetadata(new GridLength(1, GridUnitType.Star), OnColumnWidthChanged));

    public static readonly DependencyProperty MinColumnWidthProperty =
        DependencyProperty.RegisterAttached("MinColumnWidth", typeof(double), typeof(DocumentPanel), new PropertyMetadata(100d, OnMinColumnWidthChanged));

    public static readonly DependencyProperty MaxColumnWidthProperty =
        DependencyProperty.RegisterAttached("MaxColumnWidth", typeof(double), typeof(DocumentPanel), new PropertyMetadata(double.MaxValue, OnMaxColumnWidthChanged));

    public static readonly DependencyProperty RowHeightProperty =
    DependencyProperty.RegisterAttached("RowHeight", typeof(GridLength), typeof(DocumentPanel), new PropertyMetadata(new GridLength(1, GridUnitType.Star), OnRowHeightChanged));

    public static readonly DependencyProperty MinRowHeightProperty =
        DependencyProperty.RegisterAttached("MinRowHeight", typeof(double), typeof(DocumentPanel), new PropertyMetadata(100d, OnMinRowHeightChanged));

    public static readonly DependencyProperty MaxRowHeightProperty =
        DependencyProperty.RegisterAttached("MaxRowHeight", typeof(double), typeof(DocumentPanel), new PropertyMetadata(double.MaxValue, OnMaxRowHeightChanged));

    #endregion

    #region Propeties

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        internal set => SetValue(OrientationPropertyKey, value);
    }

    #endregion

    #region Methods Override

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        AttachToOwner();
    }

    #endregion

    #region Methods

    public static void SetColumnWidth(DependencyObject element, GridLength value) => element.SetValue(ColumnWidthProperty, value);

    public static GridLength GetColumnWidth(DependencyObject element) => (GridLength)element.GetValue(ColumnWidthProperty);

    private static void OnColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => UpdateColumnDefinition(d, column => column.Width = (GridLength)e.NewValue);

    public static void SetMinColumnWidth(DependencyObject element, double value) => element.SetValue(MinColumnWidthProperty, value);

    public static double GetMinColumnWidth(DependencyObject element) => (double)element.GetValue(MinColumnWidthProperty);

    private static void OnMinColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => UpdateColumnDefinition(d, column => column.MinWidth = (double)e.NewValue);

    public static void SetMaxColumnWidth(DependencyObject element, double value) => element.SetValue(MaxColumnWidthProperty, value);

    public static double GetMaxColumnWidth(DependencyObject element) => (double)element.GetValue(MaxColumnWidthProperty);

    private static void OnMaxColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => UpdateColumnDefinition(d, column => column.MaxWidth = (double)e.NewValue);

    private static void UpdateColumnDefinition(DependencyObject child, Action<ColumnDefinition> updateAction)
    {
        if (VisualTreeHelper.GetParent(child) is not DocumentPanel grid) return;
        var column = GetColumn((UIElement)child);
        if (column >= grid.ColumnDefinitions.Count) return;
        grid.Dispatcher.BeginInvoke(new Action(() => updateAction(grid.ColumnDefinitions[column])));
    }

    public static void SetRowHeight(DependencyObject element, GridLength value) => element.SetValue(RowHeightProperty, value);

    public static GridLength GetRowHeight(DependencyObject element) => (GridLength)element.GetValue(RowHeightProperty);

    private static void OnRowHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => UpdateRowDefinition(d, row => row.Height = (GridLength)e.NewValue);

    public static void SetMinRowHeight(DependencyObject element, double value) => element.SetValue(MinRowHeightProperty, value);

    public static double GetMinRowHeight(DependencyObject element) => (double)element.GetValue(MinRowHeightProperty);

    private static void OnMinRowHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => UpdateRowDefinition(d, row => row.MinHeight = (double)e.NewValue);

    public static void SetMaxRowHeight(DependencyObject element, double value) => element.SetValue(MaxRowHeightProperty, value);

    public static double GetMaxRowHeight(DependencyObject element) => (double)element.GetValue(MaxRowHeightProperty);

    private static void OnMaxRowHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => UpdateRowDefinition(d, row => row.MaxHeight = (double)e.NewValue);

    private static void UpdateRowDefinition(DependencyObject child, Action<RowDefinition> updateAction)
    {
        if (VisualTreeHelper.GetParent(child) is not DocumentPanel grid) return;
        var row = GetRow((UIElement)child);
        if (row >= grid.ColumnDefinitions.Count) return;
        grid.Dispatcher.BeginInvoke(new Action(() => updateAction(grid.RowDefinitions[row])));
    }

    internal void ArrangeLayout()
    {
        Clear(this);

        var children = _documentControls.Where(x => !x.IsHidden).Select(GenerateContainer).ToArray();
        for (var i = 0; ; i++)
        {
            if (children.Length <= i) break;

            var child = children[i];

            if (Orientation == Orientation.Horizontal)
            {
                var column = new ColumnDefinition
                {
                    MinWidth = GetMinColumnWidth(child),
                    Width = GetColumnWidth(child),
                    MaxWidth = GetMaxColumnWidth(child)
                };

                ColumnDefinitions.Add(column);

                SetColumn(child, i);
            }
            else
            {
                var row = new RowDefinition
                {
                    MinHeight = GetMinRowHeight(child),
                    Height = GetRowHeight(child),
                    MaxHeight = GetMaxRowHeight(child)
                };

                RowDefinitions.Add(row);

                SetRow(child, i);
            }

            Children.Add(child);

            if (i == children.Length - 1) break;

            var splitter = new GridSplitter()
            {
                ResizeBehavior = GridResizeBehavior.CurrentAndNext,
                Background = Brushes.Transparent
            };

            if (Orientation == Orientation.Horizontal)
            {
                splitter.Width = 5;
                splitter.VerticalAlignment = VerticalAlignment.Stretch;
                splitter.HorizontalAlignment = HorizontalAlignment.Right;

                SetColumn(splitter, i);
            }
            else
            {
                splitter.Height = 5;
                splitter.VerticalAlignment = VerticalAlignment.Bottom;
                splitter.HorizontalAlignment = HorizontalAlignment.Stretch;

                SetRow(splitter, i);
            }

            Children.Add(splitter);
        }

        InvalidateArrange();
    }

    internal void Add(DocumentControl element)
    {
        _documentControls.Add(element);

        ArrangeLayout();
    }

    internal void Add(DocumentControl element, DocumentControl relativeElement, Dock relativeDock)
    {
        if (_documentControls.Count > 1)
        {
            var orientation = relativeDock is Dock.Left or Dock.Right ? Orientation.Horizontal : Orientation.Vertical;
            if (orientation != Orientation) throw new NotSupportedException("Orientation cannot change while items is more than one!");
        }
        else Orientation = relativeDock is Dock.Left or Dock.Right ? Orientation.Horizontal : Orientation.Vertical;

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

    internal void Remove(DocumentControl element)
    {
        _documentControls.Remove(element);

        ArrangeLayout();
    }

    private void Clear(Grid grid)
    {
        foreach (UIElement child in grid.Children)
        {
            if (child is Grid chidGrid) Clear(chidGrid);
        }

        grid.Children.Clear();
        grid.ColumnDefinitions.Clear();
        grid.RowDefinitions.Clear();
    }

    private ContentPresenter GenerateContainer(object item) => new ContentPresenter { Content = item };

    private void AttachToOwner()
    {
        if (_owner != null) _owner.ItemContainerGenerator.ItemsChanged -= ItemContainerGenerator_ItemsChanged;

        _owner = TemplatedParent as DocumentList;

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

    public void Serialize(XmlDocument doc, XmlNode parentNode)
    {
    }

    public void Deserialize(DockManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
    {
    }

    #endregion
}