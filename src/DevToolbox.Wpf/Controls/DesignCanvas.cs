using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using DevToolbox.Wpf.Extensions;
using Microsoft.Win32;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a canvas that allows for design operations, including zooming,
/// panning, and item manipulation within a WPF application.
/// </summary>
[TemplatePart(Name = PART_ZoomAndPanControl, Type = typeof(ZoomAndPanControl))]
public partial class DesignCanvas : MultiSelector
{
    #region Fields/Consts

    /// <summary>
    /// Constant for the part name of the Zoom and Pan control.
    /// </summary>
    protected const string PART_ZoomAndPanControl = nameof(PART_ZoomAndPanControl);

    /// <summary>
    /// Scoped cliked item
    /// </summary>
    private static DesignLayer? _clickedItem;

    /// <summary>
    /// Scoped focused areas
    /// </summary>
    private static readonly Dictionary<string, DesignCanvas?> _focusedAreas;

    private readonly LassoAdorner _lassoAdorner;
    private readonly List<DragItemInfo> _dragItems = [];

    private bool _draggStarted;
    private bool _isDragging;
    private bool _isInfoInit;
    private bool _isMouseDown;

    private AdornerLayer _adornerLayer = default!;
    private RectangleGeometry? _clipGeometry;

    /// <summary>
    ///     An event that is raised before a new item is created so that
    ///     developers can participate in the construction of the new item.
    /// </summary>
    public event EventHandler<AddingNewItemEventArgs>? AddingNewItem;

    /// <summary>
    ///     An event that is raised when a new item is created so that
    ///     developers can initialize the item with custom default values.
    /// </summary>
    public event InitializingNewItemEventHandler? InitializingNewItem;


    // Dependency Properties
    /// <summary>
    /// Dependency property to get the starting position of the canvas.
    /// </summary>
    private static readonly DependencyPropertyKey StartPositionPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(StartPosition), typeof(Point), typeof(DesignCanvas), new FrameworkPropertyMetadata(new Point(double.NegativeInfinity, double.NegativeInfinity)));

    /// <summary>
    /// Dependency property to get the current position of the canvas.
    /// </summary>
    private static readonly DependencyPropertyKey CurrentPositionPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(CurrentPosition), typeof(Point), typeof(DesignCanvas), new FrameworkPropertyMetadata(new Point(double.NegativeInfinity, double.NegativeInfinity)));

    /// <summary>
    /// Dependency property to get the Zoom and Pan control associated with the canvas.
    /// </summary>
    private static readonly DependencyPropertyKey ZoomAndPanControlPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ZoomAndPanControl), typeof(ZoomAndPanControl), typeof(DesignCanvas), new FrameworkPropertyMetadata(default));

    /// <summary>
    /// Dependency property to set the style for the drag zoom rectangle.
    /// </summary>
    public static readonly DependencyProperty DragZoomRectangleStyleProperty =
        ZoomAndPanControl.DragZoomRectangleStyleProperty.AddOwner(typeof(DesignCanvas));

    /// <summary>
    /// Dependency property to get or set the outer background color of the canvas.
    /// </summary>
    public static readonly DependencyProperty OuterBackgroundProperty =
        ZoomAndPanControl.OuterBackgroundProperty.AddOwner(typeof(DesignCanvas));

    /// <summary>
    /// Dependency property to get or set the width of the view panel.
    /// </summary>
    public static readonly DependencyProperty ViewPanelWidthProperty =
        DependencyProperty.Register(nameof(ViewPanelWidth), typeof(double), typeof(DesignCanvas), new FrameworkPropertyMetadata(1024D));

    /// <summary>
    /// Dependency property to get or set the height of the view panel.
    /// </summary>
    public static readonly DependencyProperty ViewPanelHeightProperty =
        DependencyProperty.Register(nameof(ViewPanelHeight), typeof(double), typeof(DesignCanvas), new FrameworkPropertyMetadata(1024D));

    /// <summary>
    /// Dependency property to get or set the initial position for zooming and panning.
    /// </summary>
    public static readonly DependencyProperty ZoomAndPanInitialPositionProperty =
        ZoomAndPanControl.ZoomAndPanInitialPositionProperty.AddOwner(typeof(DesignCanvas));

    /// <summary>
    /// Dependency property to get or set the minimum zoom type.
    /// </summary>
    public static readonly DependencyProperty MinimumZoomTypeProperty =
        ZoomAndPanControl.MinimumZoomTypeProperty.AddOwner(typeof(DesignCanvas));

    /// <summary>
    /// Dependency property to get or set the mouse position within the canvas.
    /// </summary>
    public static readonly DependencyProperty MousePositionProperty =
        ZoomAndPanControl.MousePositionProperty.AddOwner(typeof(DesignCanvas), new FrameworkPropertyMetadata(new Point(double.NegativeInfinity, double.NegativeInfinity)));

    /// <summary>
    /// Dependency property to get or set a value indicating whether animations should be used.
    /// </summary>
    public static readonly DependencyProperty UseAnimationsProperty =
        ZoomAndPanControl.UseAnimationsProperty.AddOwner(typeof(DesignCanvas));

    /// <summary>
    /// Dependency property to get or set the current viewport zoom level.
    /// </summary>
    public static readonly DependencyProperty ViewportZoomProperty =
        ZoomAndPanControl.ViewportZoomProperty.AddOwner(typeof(DesignCanvas));

    /// <summary>
    /// Dependency property to get or set the type of geometry for the lasso selection.
    /// </summary>
    public static readonly DependencyProperty LasoGeometryTypeProperty =
        DependencyProperty.Register(nameof(LasoGeometryType), typeof(LasoGeometryType), typeof(DesignCanvas), new PropertyMetadata(LasoGeometryType.Rectangle));

    /// <summary>
    /// Dependency property to get or set the type of selection method used.
    /// </summary>
    public static readonly DependencyProperty SelectionTypeProperty =
        DependencyProperty.Register(nameof(SelectionType), typeof(SelectionType), typeof(DesignCanvas), new PropertyMetadata(SelectionType.Lasso));

    /// <summary>
    /// Dependency property to get or set a value indicating whether to use scoped selection.
    /// </summary>
    public static readonly DependencyProperty UseScopedSelectionProperty =
        DependencyProperty.Register(nameof(UseScopedSelection), typeof(bool), typeof(DesignCanvas));

    /// <summary>
    /// Dependency property to get or set the scope for selection within the canvas.
    /// </summary>
    public static readonly DependencyProperty ScopeProperty =
        DependencyProperty.Register(nameof(Scope), typeof(string), typeof(DesignCanvas), new PropertyMetadata(nameof(Scope), OnScopeChanged));

    /// <summary>
    /// Dependency property to get or set the template for the lasso control.
    /// </summary>
    public static readonly DependencyProperty LassoTemplateProperty =
        DependencyProperty.Register(nameof(LassoTemplate), typeof(ControlTemplate), typeof(DesignCanvas), new PropertyMetadata(OnLassoTemplateChanged));

    /// <summary>
    /// Dependency property to get or set the geometry for the lasso selection.
    /// </summary>
    public static readonly DependencyProperty LassoGeometryProperty =
        DependencyProperty.Register(nameof(LassoGeometry), typeof(Geometry), typeof(DesignCanvas), new PropertyMetadata(OnLassoGeometryChanged));

    /// <summary>
    /// Dependency property to get or set the arrangement type for the lasso selection.
    /// </summary>
    public static readonly DependencyProperty LassoArrangeTypeProperty =
        DependencyProperty.Register(nameof(LassoArrangeType), typeof(LassoArrangeType), typeof(DesignCanvas), new PropertyMetadata(OnLassoArrangeTypeChanged));

    /// <summary>
    /// Dependency property to get the starting position of the canvas.
    /// </summary>
    public static readonly DependencyProperty StartPositionProperty = StartPositionPropertyKey.DependencyProperty;

    /// <summary>
    /// Dependency property to get the current position of the canvas.
    /// </summary>
    public static readonly DependencyProperty CurrentPositionProperty = CurrentPositionPropertyKey.DependencyProperty;

    /// <summary>
    /// Dependency property to get the Zoom and Pan control associated with the canvas.
    /// </summary>
    public static readonly DependencyProperty ZoomAndPanControlProperty = ZoomAndPanControlPropertyKey.DependencyProperty;

    // Routed commands for various canvas operations
    /// <summary>
    /// Command to select all items on the canvas.
    /// </summary>
    public static readonly RoutedCommand SelectAllCommand = new(nameof(SelectAllCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to group selected items together.
    /// </summary>
    public static readonly RoutedCommand GroupCommand = new(nameof(GroupCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to ungroup a selected group of items.
    /// </summary>
    public static readonly RoutedCommand UngroupCommand = new(nameof(UngroupCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to bring a selected item forward in the z-order.
    /// </summary>
    public static readonly RoutedCommand BringForwardCommand = new(nameof(BringForwardCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to bring a selected item to the front of the z-order.
    /// </summary>
    public static readonly RoutedCommand BringToFrontCommand = new(nameof(BringToFrontCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to send a selected item backward in the z-order.
    /// </summary>
    public static readonly RoutedCommand SendBackwardCommand = new(nameof(SendBackwardCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to send a selected item to the back of the z-order.
    /// </summary>
    public static readonly RoutedCommand SendToBackCommand = new(nameof(SendToBackCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to align selected items to the top edge.
    /// </summary>
    public static readonly RoutedCommand AlignTopCommand = new(nameof(AlignTopCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to align selected items to their vertical centers.
    /// </summary>
    public static readonly RoutedCommand AlignVerticalCentersCommand = new(nameof(AlignVerticalCentersCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to align selected items to the bottom edge.
    /// </summary>
    public static readonly RoutedCommand AlignBottomCommand = new(nameof(AlignBottomCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to align selected items to the left edge.
    /// </summary>
    public static readonly RoutedCommand AlignLeftCommand = new(nameof(AlignLeftCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to align selected items to their horizontal centers.
    /// </summary>
    public static readonly RoutedCommand AlignHorizontalCentersCommand = new(nameof(AlignHorizontalCentersCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to align selected items to the right edge.
    /// </summary>
    public static readonly RoutedCommand AlignRightCommand = new(nameof(AlignRightCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to distribute selected items evenly horizontally.
    /// </summary>
    public static readonly RoutedCommand DistributeHorizontalCommand = new(nameof(DistributeHorizontalCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to distribute selected items evenly vertically.
    /// </summary> 
    public static readonly RoutedCommand DistributeVerticalCommand = new(nameof(DistributeVerticalCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to fit all items within the visible area of the canvas.
    /// </summary>
    public static readonly RoutedCommand FitCommand = new(nameof(FitCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to fill the canvas with the selected item(s).
    /// </summary>
    public static readonly RoutedCommand FillCommand = new(nameof(FillCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to zoom in on the canvas.
    /// </summary>
    public static readonly RoutedCommand ZoomInCommand = new(nameof(ZoomInCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to zoom out on the canvas.
    /// </summary>
    public static readonly RoutedCommand ZoomOutCommand = new(nameof(ZoomOutCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to undo the last zoom action on the canvas.
    /// </summary>
    public static readonly RoutedCommand UndoZoomCommand = new(nameof(UndoZoomCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to redo the last undone zoom action on the canvas.
    /// </summary>
    public static readonly RoutedCommand RedoZoomCommand = new(nameof(RedoZoomCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to set the zoom level of the canvas to a specified percentage.
    /// </summary>
    public static readonly RoutedCommand ZoomPercentCommand = new(nameof(ZoomPercentCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to zoom the canvas based on a ratio from the minimum zoom level.
    /// </summary>
    public static readonly RoutedCommand ZoomRatioFromMinimumCommand = new(nameof(ZoomRatioFromMinimumCommand), typeof(DesignCanvas));

    /// <summary>
    /// Command to save the current state of the canvas.
    /// </summary>
    public static readonly RoutedUICommand SaveCommand = ApplicationCommands.Save;

    /// <summary>
    /// Command to print the current canvas view.
    /// </summary>
    public static readonly RoutedUICommand PrintCommand = ApplicationCommands.Print;

    /// <summary>
    /// Command to cut the selected item(s) from the canvas.
    /// </summary>
    public static readonly RoutedUICommand CutCommand = ApplicationCommands.Cut;

    /// <summary>
    /// Command to copy the selected item(s) to the clipboard.
    /// </summary>
    public static readonly RoutedUICommand CopyCommand = ApplicationCommands.Copy;

    /// <summary>
    /// Command to paste item(s) from the clipboard into the canvas.
    /// </summary>
    public static readonly RoutedUICommand PasteCommand = ApplicationCommands.Paste;

    /// <summary>
    /// Command to delete the selected item(s) from the canvas.
    /// </summary>
    public static readonly RoutedUICommand DeleteCommand = ApplicationCommands.Delete;

    /// <summary>
    /// Command to create a new item on the canvas.
    /// </summary>
    public static readonly RoutedUICommand NewCommand = ApplicationCommands.New;

    /// <summary>
    /// Command to open an existing item in the canvas.
    /// </summary>
    public static readonly RoutedUICommand OpenCommand = ApplicationCommands.Open;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the clamped minimum zoom value for the ZoomAndPanControl.
    /// </summary>
    public double MinimumZoomClamped => (double)(ZoomAndPanControl?.GetValue(ZoomAndPanControl.MinimumZoomClampedProperty) ?? default(double));

    /// <summary>
    /// Gets the editable items in the canvas.
    /// </summary>
    private IEditableCollectionView EditableItems => Items;

    /// <summary>
    /// Checks if editing is allowed on the canvas.
    /// </summary>
    private bool CanEdit => EditableItems.CanRemove || !((IList)Items).IsReadOnly;

    /// <summary>
    /// Checks if the Control key is currently pressed.
    /// </summary>
    private static bool IsControlKeyPressed => (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

    /// <summary>
    /// Determines if selection can be performed based on the current conditions.
    /// </summary>
    private bool CanPerformSelection => !UseScopedSelection || !IsControlKeyPressed || FocusedArea == null || FocusedArea == this || FocusedArea.SelectedItems.Count == 0;

    /// <summary>
    /// Gets or sets the focused area for scoped selections.
    /// </summary>
    private DesignCanvas? FocusedArea
    {
        get => _focusedAreas[Scope];
        set => _focusedAreas[Scope] = value;
    }

    /// <summary>
    /// Gets the ZoomAndPanControl instance associated with the canvas.
    /// </summary>
    public ZoomAndPanControl ZoomAndPanControl
    {
        get => (ZoomAndPanControl)GetValue(ZoomAndPanControlProperty);
        private set => SetValue(ZoomAndPanControlPropertyKey, value);
    }

    /// <summary>
    /// Gets or sets the style for the drag zoom rectangle.
    /// </summary>
    public Style DragZoomRectangleStyle
    {
        get => (Style)GetValue(DragZoomRectangleStyleProperty);
        set => SetValue(DragZoomRectangleStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the background brush of the outer area of the canvas.
    /// </summary>
    public Brush OuterBackground
    {
        get => (Brush)GetValue(OuterBackgroundProperty);
        set => SetValue(OuterBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the view panel.
    /// </summary>
    public double ViewPanelWidth
    {
        get => (double)GetValue(ViewPanelWidthProperty);
        set => SetValue(ViewPanelWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the view panel.
    /// </summary>
    public double ViewPanelHeight
    {
        get => (double)GetValue(ViewPanelHeightProperty);
        set => SetValue(ViewPanelHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the initial position type for zoom and pan animations.
    /// </summary>
    public ZoomAndPanInitialPositionType ZoomAndPanInitialPosition
    {
        get => (ZoomAndPanInitialPositionType)GetValue(ZoomAndPanInitialPositionProperty);
        set => SetValue(ZoomAndPanInitialPositionProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum zoom type for the viewport zoom.
    /// </summary>
    public MinimumZoomType MinimumZoomType
    {
        get => (MinimumZoomType)GetValue(MinimumZoomTypeProperty);
        set => SetValue(MinimumZoomTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets the mouse position in the canvas.
    /// </summary>
    public Point MousePosition
    {
        get => (Point)GetValue(MousePositionProperty);
        set => SetValue(MousePositionProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether animations are enabled.
    /// </summary>
    public bool UseAnimations
    {
        get => (bool)GetValue(UseAnimationsProperty);
        set => SetValue(UseAnimationsProperty, value);
    }

    /// <summary>
    /// Gets or sets the current zoom factor of the content in the canvas.
    /// </summary>
    public double ViewportZoom
    {
        get => (double)GetValue(ViewportZoomProperty);
        set => SetValue(ViewportZoomProperty, value);
    }

    /// <summary>
    /// Gets or sets the geometry type for the lasso selection.
    /// </summary>
    public LasoGeometryType LasoGeometryType
    {
        get => (LasoGeometryType)GetValue(LasoGeometryTypeProperty);
        set => SetValue(LasoGeometryTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets the type of selection to be performed.
    /// </summary>
    public SelectionType SelectionType
    {
        get => (SelectionType)GetValue(SelectionTypeProperty);
        set => SetValue(SelectionTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether scoped selection is used.
    /// </summary>
    public bool UseScopedSelection
    {
        get => (bool)GetValue(UseScopedSelectionProperty);
        set => SetValue(UseScopedSelectionProperty, value);
    }

    /// <summary>
    /// Gets or sets the scope for selection on the canvas.
    /// </summary>
    public string Scope
    {
        get => (string)GetValue(ScopeProperty);
        set => SetValue(ScopeProperty, value);
    }

    /// <summary>
    /// Gets or sets the control template for the lasso selection.
    /// </summary>
    public ControlTemplate LassoTemplate
    {
        get => (ControlTemplate)GetValue(LassoTemplateProperty);
        set => SetValue(LassoTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the geometry for the lasso selection.
    /// </summary>
    public Geometry LassoGeometry
    {
        get => (Geometry)GetValue(LassoGeometryProperty);
        set => SetValue(LassoGeometryProperty, value);
    }

    /// <summary>
    /// Gets or sets the arrangement type for lasso selection.
    /// </summary>
    public LassoArrangeType LassoArrangeType
    {
        get => (LassoArrangeType)GetValue(LassoArrangeTypeProperty);
        set => SetValue(LassoArrangeTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets the starting position of the canvas for dragging.
    /// </summary>
    public Point StartPosition
    {
        get => (Point)GetValue(StartPositionProperty);
        private set => SetValue(StartPositionPropertyKey, value);
    }

    /// <summary>
    /// Gets or sets the current position of the canvas for dragging.
    /// </summary>
    public Point CurrentPosition
    {
        get => (Point)GetValue(CurrentPositionProperty);
        private set => SetValue(CurrentPositionPropertyKey, value);
    }

    #endregion

    static DesignCanvas()
    {
        Type typeFromHandle = typeof(DesignCanvas);

        _focusedAreas = new Dictionary<string, DesignCanvas?>()
        {
            { "Scope", null }
        };

        ClipToBoundsProperty.OverrideMetadata(typeof(DesignCanvas), new FrameworkPropertyMetadata(true));
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignCanvas), new FrameworkPropertyMetadata(typeof(DesignCanvas)));

        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(GroupCommand, OnGroupExecuted, OnGroupCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(UngroupCommand, OnUngroupExecuted, UngroupCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(BringForwardCommand, OnBringForwardExecuted, OnBringForwardCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(BringToFrontCommand, OnBringToFrontExecuted, OnBringToFrontCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(SendBackwardCommand, OnSendBackwardExecuted, OnSendBackwardCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(SendToBackCommand, OnSendToBackExecuted, OnSendToBackCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(AlignTopCommand, OnAlignTopExecuted, OnAlignTopCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(AlignVerticalCentersCommand, OnAlignVerticalCentersExecuted, OnAlignVerticalCentersCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(AlignBottomCommand, OnAlignBottomExecuted, OnAlignBottomCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(AlignLeftCommand, OnAlignLeftExecuted, OnAlignLeftCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(AlignHorizontalCentersCommand, OnAlignHorizontalCentersExecuted, OnAlignHorizontalCentersCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(AlignRightCommand, OnAlignRightExecuted, OnAlignRightCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(DistributeHorizontalCommand, OnDistributeHorizontalExecuted, OnDistributeHorizontalCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(DistributeVerticalCommand, OnDistributeVerticalExecuted, OnDistributeVerticalCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(SaveCommand, OnSaveExecuted, OnSaveCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(PrintCommand, OnPrintExecuted, OnPrintCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(CutCommand, OnCutExecuted, OnCutCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(CopyCommand, OnCopyExecuted, OnCopyCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(PasteCommand, OnPasteExecuted, OnPasteCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(DeleteCommand, OnDeleteExecuted, OnDeleteCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(NewCommand, OnNewExecuted, OnNewCanExecute));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(OpenCommand, OnOpenExecuted, OnOpenCanExecute));

        CommandManager.RegisterClassInputBinding(typeFromHandle, new InputBinding(SelectAllCommand, new KeyGesture(Key.A, ModifierKeys.Control)));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(SelectAllCommand, OnSelectAllExecuted, OnSelectAllCanExecute));

        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(FillCommand,
            (sender, e) => OnZoomAndPanControlCommandExecute(ZoomAndPanControl.FillCommand, sender, e),
            (sender, e) => OnZoomAndPanControlCommandCanExecute(ZoomAndPanControl.FillCommand, sender, e)));

        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(FitCommand,
            (sender, e) => OnZoomAndPanControlCommandExecute(ZoomAndPanControl.FitCommand, sender, e),
            (sender, e) => OnZoomAndPanControlCommandCanExecute(ZoomAndPanControl.FitCommand, sender, e)));

        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(ZoomPercentCommand,
            (sender, e) => OnZoomAndPanControlCommandExecute(ZoomAndPanControl.ZoomPercentCommand, sender, e),
            (sender, e) => OnZoomAndPanControlCommandCanExecute(ZoomAndPanControl.ZoomPercentCommand, sender, e)));

        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(ZoomRatioFromMinimumCommand,
            (sender, e) => OnZoomAndPanControlCommandExecute(ZoomAndPanControl.ZoomRatioFromMinimumCommand, sender, e),
            (sender, e) => OnZoomAndPanControlCommandCanExecute(ZoomAndPanControl.ZoomRatioFromMinimumCommand, sender, e)));

        CommandManager.RegisterClassInputBinding(typeFromHandle, new InputBinding(ZoomOutCommand, new KeyGesture(Key.OemMinus, ModifierKeys.Shift)));
        CommandManager.RegisterClassInputBinding(typeFromHandle, new InputBinding(ZoomOutCommand, new KeyGesture(Key.Subtract)));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(ZoomOutCommand,
            (sender, e) => OnZoomAndPanControlCommandExecute(ZoomAndPanControl.ZoomOutCommand, sender, e),
            (sender, e) => OnZoomAndPanControlCommandCanExecute(ZoomAndPanControl.ZoomOutCommand, sender, e)));

        CommandManager.RegisterClassInputBinding(typeFromHandle, new InputBinding(ZoomInCommand, new KeyGesture(Key.OemPlus, ModifierKeys.Shift)));
        CommandManager.RegisterClassInputBinding(typeFromHandle, new InputBinding(ZoomInCommand, new KeyGesture(Key.Add)));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(ZoomInCommand,
            (sender, e) => OnZoomAndPanControlCommandExecute(ZoomAndPanControl.ZoomInCommand, sender, e),
            (sender, e) => OnZoomAndPanControlCommandCanExecute(ZoomAndPanControl.ZoomInCommand, sender, e)));

        CommandManager.RegisterClassInputBinding(typeFromHandle, new InputBinding(UndoZoomCommand, new KeyGesture(Key.Z, ModifierKeys.Control)));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(UndoZoomCommand,
            (sender, e) => OnZoomAndPanControlCommandExecute(ZoomAndPanControl.UndoZoomCommand, sender, e),
            (sender, e) => OnZoomAndPanControlCommandCanExecute(ZoomAndPanControl.UndoZoomCommand, sender, e)));

        CommandManager.RegisterClassInputBinding(typeFromHandle, new InputBinding(UndoZoomCommand, new KeyGesture(Key.Y, ModifierKeys.Control)));
        CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(UndoZoomCommand,
            (sender, e) => OnZoomAndPanControlCommandExecute(ZoomAndPanControl.RedoZoomCommand, sender, e),
            (sender, e) => OnZoomAndPanControlCommandCanExecute(ZoomAndPanControl.RedoZoomCommand, sender, e)));

        void OnZoomAndPanControlCommandExecute(RoutedCommand routedCommand, object sender, ExecutedRoutedEventArgs e) =>
            routedCommand.Execute(e.Parameter, (sender as DesignCanvas)?.ZoomAndPanControl);

        void OnZoomAndPanControlCommandCanExecute(RoutedCommand routedCommand, object sender, CanExecuteRoutedEventArgs e) =>
            e.CanExecute = sender is DesignCanvas designerView && routedCommand.CanExecute(e.Parameter, designerView.ZoomAndPanControl);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DesignCanvas"/> class.
    /// </summary>
    public DesignCanvas()
    {
        _lassoAdorner = new LassoAdorner(this);
    }

    #region Methods Override

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (Template.FindName(PART_ZoomAndPanControl, this) is ZoomAndPanControl zoomAndPanControl)
        {
            ZoomAndPanControl = zoomAndPanControl;
        }

        ZoomAndPanControl.MouseLeftButtonDown += OnMouseLeftButtonDown;

        _adornerLayer = AdornerLayer.GetAdornerLayer(this);

        if (_adornerLayer is null)
        {
            throw new ArgumentNullException("Could not get an AdornerLayer object. Initialization cannot continue.", nameof(AdornerLayer));
        }
    }

    /// <inheritdoc/>
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new DesignLayer();
    }

    /// <inheritdoc/>
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is DesignLayer;
    }

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is not DesignLayer viewItem) return;

        //double left = (ViewPanelWidth - viewItem.Width) / 2;
        //Canvas.SetLeft(viewItem, left);

        //double top = (ViewPanelHeight - viewItem.Height) / 2;
        //Canvas.SetTop(viewItem, top);

        viewItem.MouseLeftButtonDown += OnItemMouseLeftButtonDown;
    }

    /// <inheritdoc/>
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!_isMouseDown)
            return;

        if (_draggStarted)
        {
            _isDragging = true;

            if (!_isInfoInit)
            {
                foreach (var selectedItem in SelectedItems)
                {
                    if (IsItemItsOwnContainer(selectedItem))
                        _dragItems.Add(new((DesignLayer)selectedItem, e.GetPosition((DesignLayer)selectedItem), ((DesignLayer)selectedItem).RenderTransform));
                    else
                    {
                        var portraitViewItem = (DesignLayer)ItemContainerGenerator.ContainerFromItem(selectedItem);
                        _dragItems.Add(new(portraitViewItem, e.GetPosition(portraitViewItem), portraitViewItem.RenderTransform));
                    }
                }

                _isInfoInit = true;
            }

            if (_dragItems.Count > 0)
            {
                foreach (var item in _dragItems)
                {
                    var itemsPresenter = DependencyObjectExtensions.GetVisualChild<ItemsPresenter>(this);
                    var itemsPanel = VisualTreeHelper.GetChild(itemsPresenter, 0) as Panel;
                    var currentPosition = e.GetPosition(itemsPanel);

                    if (item.Transform != null)
                        currentPosition = item.Transform.Transform(currentPosition);

                    Canvas.SetLeft(item.Item, currentPosition.X - item.StartPosition.X);
                    Canvas.SetTop(item.Item, currentPosition.Y - item.StartPosition.Y);
                }
            }
        }

        CurrentPosition = Mouse.GetPosition(this);
        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        ReleaseMouseCapture();

        if (_clickedItem != null && !_isDragging)
            UnselectAllExceptThisItem(_clickedItem);

        if (!_isMouseDown)
            return;

        _draggStarted = false;
        _isInfoInit = false;
        _isDragging = false;
        _dragItems.Clear();

        DesignLayer? item = null;

        if (LassoGeometry != null)
        {
            var capturedItems = new List<DesignLayer>();

            VisualTreeHelper.HitTest(this, a =>
            {
                item = a as DesignLayer;
                return item != null && item.InternalParent == this
                    ? HitTestFilterBehavior.ContinueSkipChildren
                    : HitTestFilterBehavior.ContinueSkipSelf;
            }, a =>
            {
                switch (((GeometryHitTestResult)a).IntersectionDetail)
                {
                    case IntersectionDetail.FullyInside:
                    case IntersectionDetail.Intersects:
                        if (a.VisualHit is DesignLayer controlItem)
                            capturedItems.Add(controlItem);
                        break;
                }

                return HitTestResultBehavior.Continue;
            }, new GeometryHitTestParameters(LassoGeometry));

            Select(capturedItems);
        }

        if (SelectedItems.Count == 0)
        {
            item = ContainerFromElement(null, this) as DesignLayer;

            if (item != null)
            {
                var area = item.InternalParent;

                area?.NotifyItemClicked(item);
            }
        }

        StartPosition = new Point(double.NegativeInfinity, double.NegativeInfinity);
        CurrentPosition = StartPosition;

        _isMouseDown = false;
        _adornerLayer.Remove(_lassoAdorner);
    }

    /// <inheritdoc/>
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        _clipGeometry = new RectangleGeometry(VisualTreeHelper.GetDescendantBounds(this)) { Transform = (Transform)TransformToVisual(_adornerLayer) };
        _clipGeometry.Freeze();

        if (LassoArrangeType == LassoArrangeType.ClipBounds)
            _lassoAdorner.Refresh(_clipGeometry.Bounds, (Transform)_clipGeometry.Transform.Inverse);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Notifies the canvas of a clicked item and handles selection based on the current selection type.
    /// </summary>
    /// <param name="item">The design layer item that was clicked.</param>
    private void NotifyItemClicked(DesignLayer item)
    {
        _clickedItem = null;

        switch (SelectionType)
        {
            case SelectionType.Single:
                if (!item.IsSelected)
                {
                    ClearTargetArea();
                    Select(item);
                }
                else if (IsControlKeyPressed)
                    Unselect(item);
                break;
            case SelectionType.Multiple:
                if (UseScopedSelection && FocusedArea != this)
                    ClearTargetArea();

                ToggleSelect(item);
                break;
            case SelectionType.Extended:
            case SelectionType.Lasso:
                if (!CanPerformSelection)
                    return;

                if (IsControlKeyPressed)
                    ToggleSelect(item);
                else if (!item.IsSelected)
                {
                    ClearTargetArea();
                    Select(item);
                }
                else _clickedItem = item;
                break;
            default:
                break;
        }

        Focus();

        FocusedArea = this;
        Mouse.Capture(this);
    }

    /// <summary>
    /// Selects the specified item and updates the selection list accordingly.
    /// </summary>
    /// <param name="item">The item to select.</param>
    public void Select(ISelectable item)
    {
        if (!IsUpdatingSelectedItems)
        {
            BeginUpdateSelectedItems();

            if (item is IGroupable groupable)
            {
                var groupItems = GetGroupMembers(groupable);

                foreach (var groupItem in groupItems)
                    SelectedItems.Add(this.ItemFromContainer(groupItem));
            }
            else SelectedItems.Add(this.ItemFromContainer(item));

            EndUpdateSelectedItems();
        }
    }

    /// <summary>
    /// Selects a collection of items and updates the selection list accordingly.
    /// </summary>
    /// <param name="items">The items to select.</param>
    private void Select(IEnumerable<ISelectable> items)
    {
        if (!IsUpdatingSelectedItems)
        {
            BeginUpdateSelectedItems();
            items.ForEach(item => SelectedItems.Add(this.ItemFromContainer(item)));
            EndUpdateSelectedItems();
        }
    }

    /// <summary>
    /// Unselects the specified item and updates the selection list accordingly.
    /// </summary>
    /// <param name="item">The item to unselect.</param>
    public void Unselect(ISelectable item)
    {
        if (!IsUpdatingSelectedItems)
        {
            BeginUpdateSelectedItems();

            if (item is IGroupable groupable)
            {
                var groupItems = GetGroupMembers(groupable);

                foreach (ISelectable groupItem in groupItems)
                    SelectedItems.Remove(ItemContainerGenerator.ItemFromContainer((DependencyObject)groupItem));
            }
            else SelectedItems.Remove(ItemContainerGenerator.ItemFromContainer((DependencyObject)item));

            EndUpdateSelectedItems();
        }
    }

    /// <summary>
    /// Toggles the selection state of the specified item.
    /// </summary>
    /// <param name="item">The item whose selection state is to be toggled.</param>
    private void ToggleSelect(ISelectable item)
    {
        if (item.IsSelected)
            Unselect(item);
        else Select(item);
    }

    /// <summary>
    /// Unselects all items except for the specified item.
    /// </summary>
    /// <param name="item">The item to keep selected.</param>
    private void UnselectAllExceptThisItem(ISelectable item)
    {
        if (!IsUpdatingSelectedItems)
        {
            BeginUpdateSelectedItems();
            SelectedItems.Cast<object>()
                .Where(a => item != this.ContainerFromItem(a))
                .ToList()
                .ForEach(SelectedItems.Remove);
            EndUpdateSelectedItems();
        }
    }

    /// <summary>
    /// Clears the target selection area by unselecting items in the focused area.
    /// </summary>
    private void ClearTargetArea()
    {
        var area = UseScopedSelection ? FocusedArea : this;
        area?.UnselectAll();
    }

    /// <summary>
    /// Handles changes to the scope of the selection area.
    /// </summary>
    /// <param name="d">The dependency object that changed.</param>
    /// <param name="e">Event arguments containing the old and new value.</param>
    private static void OnScopeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var scope = (string)e.NewValue;

        if (!_focusedAreas.ContainsKey(scope))
            _focusedAreas.Add(scope, null);
    }

    /// <summary>
    /// Handles changes to the lasso template used for selection.
    /// </summary>
    /// <param name="d">The dependency object that changed.</param>
    /// <param name="e">Event arguments containing the old and new template.</param>
    private static void OnLassoTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var designCanvas = (DesignCanvas)d;
        designCanvas.OnLassoTemplateChanged((ControlTemplate)e.OldValue, (ControlTemplate)e.NewValue);
    }

    /// <summary>
    /// Handles changes to the geometry used in the lasso selection.
    /// </summary>
    /// <param name="d">The dependency object that changed.</param>
    /// <param name="e">Event arguments containing the old and new geometry.</param>
    private static void OnLassoGeometryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var designCanvas = (DesignCanvas)d;
        designCanvas.OnLassoGeometryChanged((Geometry)e.OldValue, (Geometry)e.NewValue);
    }

    /// <summary>
    /// Handles changes to the arrangement type of the lasso selection.
    /// </summary>
    /// <param name="d">The dependency object that changed.</param>
    /// <param name="e">Event arguments containing the old and new arrangement type.</param>
    private static void OnLassoArrangeTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var designCanvas = (DesignCanvas)d;
        designCanvas.OnLassoArrangeTypeChanged((LassoArrangeType)e.OldValue, (LassoArrangeType)e.NewValue);
    }

    /// <summary>
    /// Refreshes the lasso adorner with a new template.
    /// </summary>
    /// <param name="oldValue">The old ControlTemplate value.</param>
    /// <param name="newValue">The new ControlTemplate value.</param>
    private void OnLassoTemplateChanged(ControlTemplate oldValue, ControlTemplate newValue)
    {
        if (newValue is null) return;
        _lassoAdorner.Refresh(newValue);
    }

    /// <summary>
    /// Refreshes the lasso adorner with new geometry.
    /// </summary>
    /// <param name="oldValue">The old Geometry value.</param>
    /// <param name="newValue">The new Geometry value.</param>
    private void OnLassoGeometryChanged(Geometry oldValue, Geometry newValue)
    {
        if (LassoArrangeType == LassoArrangeType.LassoBounds && newValue is not null)
            _lassoAdorner.Refresh(newValue.Bounds, null!);
    }

    /// <summary>
    /// Refreshes the lasso adorner based on the arrangement type change.
    /// </summary>
    /// <param name="oldValue">The old arrangement type.</param>
    /// <param name="newValue">The new arrangement type.</param>
    private void OnLassoArrangeTypeChanged(LassoArrangeType oldValue, LassoArrangeType newValue)
    {
        Geometry? geometry = null;
        Transform? transform = null;

        switch (newValue)
        {
            case LassoArrangeType.LassoBounds:
                geometry = LassoGeometry;
                break;
            case LassoArrangeType.ClipBounds:
                geometry = _clipGeometry;

                if (geometry != null)
                    transform = (Transform)geometry.Transform.Inverse;
                break;
            default:
                break;
        }

        if (geometry != null && transform != null)
            _lassoAdorner.Refresh(geometry.Bounds, transform);
    }

    /// <summary>
    /// Updates the Z-index of the items based on their order in the panel.
    /// </summary>
    protected void UpdateZIndex()
    {
        var ordered = (from UIElement item in Items.Select(x => this.ContainerFromItem(x))
                       orderby Panel.GetZIndex(item)
                       select item as UIElement).ToList();

        for (var i = 0; i < ordered.Count; i++)
        {
            Panel.SetZIndex(ordered[i], i);
        }
    }

    /// <summary>
    /// Calculates the bounding rectangle that encompasses all the specified design layer items.
    /// </summary>
    /// <param name="items">The collection of design layer items.</param>
    /// <returns>The bounding rectangle that encloses the items.</returns>
    private static Rect GetBoundingRectangle(IEnumerable<DesignLayer> items)
    {
        var x1 = double.MaxValue;
        var y1 = double.MaxValue;
        var x2 = double.MinValue;
        var y2 = double.MinValue;

        foreach (var item in items)
        {
            x1 = Math.Min(Canvas.GetLeft(item), x1);
            y1 = Math.Min(Canvas.GetTop(item), y1);

            x2 = Math.Max(Canvas.GetLeft(item) + item.Width, x2);
            y2 = Math.Max(Canvas.GetTop(item) + item.Height, y2);
        }

        return new Rect(new Point(x1, y1), new Point(x2, y2));
    }

    /// <summary>
    /// Adds a new item to the canvas and initializes it if necessary.
    /// </summary>
    /// <returns>The newly added item, or null if unable to add.</returns>
    internal object? AddNewItem()
    {
        object? newItem = null;
        var isReadOnly = ((IList)Items).IsReadOnly;

        IEditableCollectionViewAddNewItem ani = Items;

        if (ani.CanAddNewItem || !isReadOnly)
        {
            AddingNewItemEventArgs e = new();
            OnAddingNewItem(e);
            newItem = e.NewItem;
        }

        if (!isReadOnly)
        {
            if (newItem == null)
            {
                newItem = GetContainerForItemOverride();
                Items.Add(newItem);
            }
            else Items.Add(newItem);
        }
        else
        {
            newItem = (newItem != null) ? ani.AddNewItem(newItem) : EditableItems.AddNew();
            EditableItems.CommitNew();
        }

        if (newItem != null)
            OnInitializingNewItem(new InitializingNewItemEventArgs(newItem));

        CommandManager.InvalidateRequerySuggested();

        return newItem;
    }

    /// <summary>
    ///     A method that is called before a new item is created so that
    ///     overrides can participate in the construction of the new item.
    /// </summary>
    /// <remarks>
    ///     The default implementation raises the AddingNewItem event.
    /// </remarks>
    /// <param name="e">Event arguments that provide access to the new item.</param>
    protected virtual void OnAddingNewItem(AddingNewItemEventArgs e)
    {
        AddingNewItem?.Invoke(this, e);
    }

    /// <summary>
    ///     A method that is called when a new item is created so that
    ///     overrides can initialize the item with custom default values.
    /// </summary>
    /// <remarks>
    ///     The default implementation raises the InitializingNewItem event.
    /// </remarks>
    /// <param name="e">Event arguments that provide access to the new item.</param>
    protected virtual void OnInitializingNewItem(InitializingNewItemEventArgs e)
    {
        InitializingNewItem?.Invoke(this, e);
    }

    /// <summary>
    /// Determines whether two groupable items belong to the same group.
    /// </summary>
    /// <param name="item1">The first groupable item.</param>
    /// <param name="item2">The second groupable item.</param>
    /// <returns>True if both items belong to the same group; otherwise, false.</returns>
    public bool BelongToSameGroup(IGroupable item1, IGroupable item2)
    {
        var root1 = GetGroupRoot(item1);
        var root2 = GetGroupRoot(item2);

        return root1?.ID == root2?.ID;
    }

    /// <summary>
    /// Retrieves the group members for the specified item.
    /// </summary>
    /// <param name="item">The item for which to get group members.</param>
    /// <returns>An enumerable collection of groupable items.</returns>
    public IEnumerable<IGroupable> GetGroupMembers(IGroupable item)
    {
        var list = Items.Select(this.ContainerFromItem).OfType<IGroupable>();
        var rootItem = GetRoot(list, item);
        return rootItem == null ? Enumerable.Empty<IGroupable>() : GetGroupMembers(list, rootItem);
    }

    /// <summary>
    /// Gets the root group for the specified item.
    /// </summary>
    /// <param name="item">The item for which to get the group root.</param>
    /// <returns>The root groupable item, or null if not found.</returns>
    private IGroupable? GetGroupRoot(IGroupable item)
    {
        var list = Items.Select(this.ContainerFromItem).OfType<IGroupable>();
        return GetRoot(list, item);
    }

    /// <summary>
    /// Recursively retrieves the root of the group tree for the specified node.
    /// </summary>
    /// <param name="list">The list of groupable items.</param>
    /// <param name="node">The current groupable node.</param>
    /// <returns>The root groupable item, or null if no root is found.</returns>
    private IGroupable? GetRoot(IEnumerable<IGroupable> list, IGroupable node)
    {
        if (node == null || node.ParentID == Guid.Empty)
        {
            return node;
        }

        foreach (var item in list)
        {
            if (item.ID == node.ParentID)
            {
                return GetRoot(list, item);
            }
        }

        return null;
    }

    /// <summary>
    /// Retrieves all members of a specified group, including nested groups.
    /// </summary>
    /// <param name="list">The list of groupable items.</param>
    /// <param name="parent">The parent groupable item.</param>
    /// <returns>A list of groupable items belonging to the specified group.</returns>
    private List<IGroupable> GetGroupMembers(IEnumerable<IGroupable> list, IGroupable parent)
    {
        var groupMembers = new List<IGroupable> { parent };
        var children = list.Where(node => node.ParentID == parent.ID);

        foreach (var child in children)
        {
            groupMembers.AddRange(GetGroupMembers(list, child));
        }

        return groupMembers;
    }

    /// <summary>
    /// Saves the specified XElement to a file using a SaveFileDialog.
    /// </summary>
    /// <param name="xElement">The XElement to save.</param>
    protected static void SaveFile(XElement xElement)
    {
        var saveFile = new SaveFileDialog
        {
            Filter = "Files (*.xml)|*.xml|All Files (*.*)|*.*"
        };

        if (saveFile.ShowDialog() == true)
        {
            xElement.Save(saveFile.FileName);
        }
    }

    /// <summary>
    /// Loads serialized data from a selected XML file using an OpenFileDialog.
    /// </summary>
    /// <returns>The loaded XElement, or null if the dialog was canceled.</returns>
    protected static XElement? LoadSerializedDataFromFile()
    {
        var openFile = new OpenFileDialog
        {
            Filter = "Designer Files (*.xml)|*.xml|All Files (*.*)|*.*"
        };

        if (openFile.ShowDialog() == true)
        {
            return XElement.Load(openFile.FileName);
        }

        return null;
    }

    /// <summary>
    /// Loads serialized data from the clipboard if it contains XAML data.
    /// </summary>
    /// <returns>The loaded XElement, or null if the clipboard does not contain valid data.</returns>
    protected static XElement? LoadSerializedDataFromClipBoard()
    {
        if (Clipboard.ContainsData(DataFormats.Xaml))
        {
            var clipboardData = Clipboard.GetData(DataFormats.Xaml).ToString();

            if (string.IsNullOrEmpty(clipboardData))
                return null;

            return XElement.Load(new StringReader(clipboardData));
        }

        return null;
    }

    /// <summary>
    /// Serializes the given designer items into an XElement.
    /// </summary>
    /// <param name="designerItems">The collection of designer items to serialize.</param>
    /// <returns>An XElement representing the serialized designer items.</returns>
    private static XElement SerializeDesignerItems(IEnumerable<DesignLayer> designerItems)
    {
        var content = from item in designerItems
                      select item.Serialize();

        return new XElement("DesignerViewItems", content);
    }

    #endregion

    #region Events Subscriptions

    private void OnItemMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DesignLayer portraitViewItem) return;

        NotifyItemClicked(portraitViewItem);

        _draggStarted = true;
        _isMouseDown = true;
        e.Handled = true;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        switch (SelectionType)
        {
            case SelectionType.Single:
            case SelectionType.Multiple:
            case SelectionType.Extended:
                break;
            case SelectionType.Lasso:
                if (!CanPerformSelection || _draggStarted)
                    return;

                if (!IsControlKeyPressed)
                    ClearTargetArea();

                StartPosition = Mouse.GetPosition(this);
                CurrentPosition = StartPosition;

                _clickedItem = null;
                FocusedArea = this;

                _adornerLayer.Clip = _clipGeometry;
                _adornerLayer.Add(_lassoAdorner);

                _isMouseDown = true;
                e.Handled = true;

                Mouse.Capture(this);
                break;
            default:
                break;
        }
    }

    #endregion

    #region Commands

    /// <summary>
    /// Executes the grouping command for the selected items.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnGroupExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnGroupExecuted(e);

    /// <summary>
    /// Groups the selected items into a new group item.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnGroupExecuted(ExecutedRoutedEventArgs e)
    {
        var items = from item in SelectedItems.Select(x => this.ContainerFromItem(x) as DesignLayer)
                    where item.ParentID == Guid.Empty
                    select item;

        Rect rect = GetBoundingRectangle(items);

        var itemObject = AddNewItem();

        if (itemObject == null || this.ContainerFromItem(itemObject) is not DesignLayer groupItem) return;

        groupItem.IsGroup = true;
        groupItem.Width = rect.Width;
        groupItem.Height = rect.Height;
        Canvas.SetLeft(groupItem, rect.Left);
        Canvas.SetTop(groupItem, rect.Top);
        Canvas groupCanvas = new();
        groupItem.Content = groupCanvas;
        Panel.SetZIndex(groupItem, Items.Count);

        foreach (var item in items)
            item.ParentID = groupItem.ID;

        Select(groupItem);
    }

    /// <summary>
    /// Determines if the group command can be executed based on selected items.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnGroupCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnGroupCanExecute(e);

    /// <summary>
    /// Checks if grouping is possible based on the number of selected items.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnGroupCanExecute(CanExecuteRoutedEventArgs e)
    {
        int count = (from item in SelectedItems.Select(x => this.ContainerFromItem(x) as DesignLayer)
                     where item.ParentID == Guid.Empty
                     select item).Count();

        e.CanExecute = count > 1;
    }

    /// <summary>
    /// Executes the ungrouping command for the selected items.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnUngroupExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnUngroupExecuted(e);

    /// <summary>
    /// Ungroups the selected group items and resets their parent IDs.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnUngroupExecuted(ExecutedRoutedEventArgs e)
    {
        var groups = (from DesignLayer item in SelectedItems.Select(x => this.ContainerFromItem(x))
                      where item.IsGroup && item.ParentID == Guid.Empty
                      select item).ToArray();

        foreach (var groupRoot in groups)
        {
            var children = from DesignLayer child in SelectedItems.Select(x => this.ContainerFromItem(x))
                           where child.ParentID == groupRoot.ID
                           select child;

            foreach (var child in children)
                child.ParentID = Guid.Empty;

            Unselect(groupRoot);
            var item = this.ItemFromContainer(groupRoot);
            EditableItems.Remove(item);
            UpdateZIndex();
        }
    }

    /// <summary>
    /// Determines if the ungroup command can be executed based on selected items.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void UngroupCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.UngroupCanExecute(e);

    /// <summary>
    /// Checks if ungrouping is possible based on the selected items.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void UngroupCanExecute(CanExecuteRoutedEventArgs e) =>
        e.CanExecute = (from DesignLayer item in SelectedItems.Select(x => this.ContainerFromItem(x))
                        where item.ParentID != Guid.Empty
                        select item).Any();

    /// <summary>
    /// Executes the print command for the design canvas.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnPrintExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is not DesignCanvas designerView) return;

        designerView.UnselectAll();

        var printDialog = new PrintDialog();

        if (printDialog.ShowDialog() ?? false)
            printDialog.PrintVisual(designerView, "WPF Diagram");
    }

    /// <summary>
    /// Determines if the print command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnPrintCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnPrintCanExecute(e);

    /// <summary>
    /// Checks if printing is possible.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnPrintCanExecute(CanExecuteRoutedEventArgs e) =>
        e.CanExecute = true;

    /// <summary>
    /// Executes the bring forward command for selected items.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnBringForwardExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnBringForwardExecuted(e);

    /// <summary>
    /// Brings the selected items forward in the Z-order.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnBringForwardExecuted(ExecutedRoutedEventArgs e)
    {
        var ordered = (from DesignLayer item in SelectedItems.Select(x => this.ContainerFromItem(x))
                       orderby Panel.GetZIndex(item) descending
                       select item as UIElement).ToList();

        var count = Items.Count;

        for (var i = 0; i < ordered.Count; i++)
        {
            var currentIndex = Panel.GetZIndex(ordered[i]);
            var newIndex = Math.Min(count - 1 - i, currentIndex + 1);
            if (currentIndex != newIndex)
            {
                Panel.SetZIndex(ordered[i], newIndex);
                var elements = Items.Select(x => this.ContainerFromItem(x) as UIElement)
                                    .Where(item => Panel.GetZIndex(item) == newIndex);

                foreach (var element in elements)
                {
                    if (element != ordered[i])
                    {
                        Panel.SetZIndex(element, currentIndex);
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Determines if the bring forward command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnBringForwardCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnBringForwardCanExecute(e);

    /// <summary>
    /// Checks if bringing forward is possible based on selected items.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnBringForwardCanExecute(CanExecuteRoutedEventArgs e) =>
        e.CanExecute = SelectedItems.Count > 0;

    /// <summary>
    /// Executes the bring to front command for selected items.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnBringToFrontExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnBringToFrontExecuted(e);

    /// <summary>
    /// Brings the selected items to the front in the Z-order.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnBringToFrontExecuted(ExecutedRoutedEventArgs e)
    {
        var selectionSorted = (from DesignLayer item in SelectedItems.Select(item => this.ContainerFromItem(item))
                               orderby Panel.GetZIndex(item) ascending
                               select item as UIElement).ToList();

        var childrenSorted = (from DesignLayer item in Items.Select(item => this.ContainerFromItem(item))
                              orderby Panel.GetZIndex(item) ascending
                              select item as UIElement).ToList();

        int i = 0;
        int j = 0;
        foreach (var item in childrenSorted)
        {
            if (selectionSorted.Contains(item))
            {
                int idx = Panel.GetZIndex(item);
                Panel.SetZIndex(item, childrenSorted.Count - selectionSorted.Count + j++);
            }
            else
            {
                Panel.SetZIndex(item, i++);
            }
        }
    }

    /// <summary>
    /// Determines if the bring to front command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnBringToFrontCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnBringToFrontCanExecute(e);

    /// <summary>
    /// Checks if bringing to front is possible based on selected items.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnBringToFrontCanExecute(CanExecuteRoutedEventArgs e) =>
        e.CanExecute = SelectedItems.Count > 0;

    /// <summary>
    /// Executes the send backward command for selected items.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnSendBackwardExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnSendBackwardExecuted(e);

    /// <summary>
    /// Sends the selected items backward in the Z-order.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnSendBackwardExecuted(ExecutedRoutedEventArgs e)
    {
        var ordered = (from DesignLayer item in SelectedItems.Select(x => this.ContainerFromItem(x))
                       orderby Panel.GetZIndex(item) ascending
                       select item as UIElement).ToList();

        var count = Items.Count;

        for (var i = 0; i < ordered.Count; i++)
        {
            var currentIndex = Panel.GetZIndex(ordered[i]);
            var newIndex = Math.Max(0, currentIndex - 1);
            if (currentIndex != newIndex)
            {
                Panel.SetZIndex(ordered[i], newIndex);
                var elements = Items.Select(x => this.ContainerFromItem(x) as UIElement)
                                    .Where(item => Panel.GetZIndex(item) == newIndex);

                foreach (var element in elements)
                {
                    if (element != ordered[i])
                    {
                        Panel.SetZIndex(element, currentIndex);
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Determines if the send backward command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnSendBackwardCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnSendBackwardCanExecute(e);

    /// <summary>
    /// Checks if sending backward is possible based on selected items.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnSendBackwardCanExecute(CanExecuteRoutedEventArgs e) =>
        e.CanExecute = SelectedItems.Count > 0;

    /// <summary>
    /// Executes the send to back command for selected items.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnSendToBackExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnSendToBackExecuted(e);

    /// <summary>
    /// Sends the selected items to the back in the Z-order.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnSendToBackExecuted(ExecutedRoutedEventArgs e)
    {
        var selectionSorted = (from DesignLayer item in SelectedItems.Select(item => this.ContainerFromItem(item))
                               orderby Panel.GetZIndex(item) descending
                               select item as UIElement).ToList();

        var childrenSorted = (from DesignLayer item in Items.Select(item => this.ContainerFromItem(item))
                              orderby Panel.GetZIndex(item) descending
                              select item as UIElement).ToList();

        int i = 0;
        int j = 0;
        foreach (var item in childrenSorted)
        {
            if (selectionSorted.Contains(item))
            {
                int idx = Panel.GetZIndex(item);
                Panel.SetZIndex(item, j++);
            }
            else
            {
                Panel.SetZIndex(item, i++);
            }
        }
    }

    /// <summary>
    /// Determines if the send to back command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnSendToBackCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnSendToBackCanExecute(e);

    /// <summary>
    /// Checks if sending to back is possible based on selected items.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnSendToBackCanExecute(CanExecuteRoutedEventArgs e) =>
        e.CanExecute = SelectedItems.Count > 0;
    /// <summary>
    /// Executes the align top command for selected items.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnAlignTopExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnAlignTopExecuted(e);

    /// <summary>
    /// Aligns the top of the selected items to the top of the first selected item.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnAlignTopExecuted(ExecutedRoutedEventArgs e)
    {
        var selectedItems = from DesignLayer item in SelectedItems.Select(x => this.ContainerFromItem(x))
                            where item.ParentID == Guid.Empty
                            select item;

        if (selectedItems.Count() > 1)
        {
            double top = Canvas.GetTop(selectedItems.First());

            foreach (var item in selectedItems)
            {
                double delta = top - Canvas.GetTop(item);
                foreach (DesignLayer di in GetGroupMembers(item))
                {
                    Canvas.SetTop(di, Canvas.GetTop(di) + delta);
                }
            }
        }
    }

    /// <summary>
    /// Determines if the align top command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnAlignTopCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnAlignTopCanExecute(e);

    /// <summary>
    /// Checks if the align top command can be executed based on the selected items.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnAlignTopCanExecute(CanExecuteRoutedEventArgs e) =>
        //var groupedItem = from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
        //                  where item.ParentID == Guid.Empty
        //                  select item;


        //e.CanExecute = groupedItem.Count() > 1;
        e.CanExecute = true;

    /// <summary>
    /// Executes the align vertical centers command for selected items.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnAlignVerticalCentersExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnAlignVerticalCentersExecuted(e);

    /// <summary>
    /// Aligns the vertical centers of the selected items to the center of the first selected item.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnAlignVerticalCentersExecuted(ExecutedRoutedEventArgs e)
    {
        var selectedItems = from DesignLayer item in SelectedItems.Select(x => this.ContainerFromItem(x))
                            where item.ParentID == Guid.Empty
                            select item;

        if (selectedItems.Count() > 1)
        {
            double bottom = Canvas.GetTop(selectedItems.First()) + selectedItems.First().Height / 2;

            foreach (var item in selectedItems)
            {
                double delta = bottom - (Canvas.GetTop(item) + item.Height / 2);
                foreach (DesignLayer di in GetGroupMembers(item))
                {
                    Canvas.SetTop(di, Canvas.GetTop(di) + delta);
                }
            }
        }
    }

    /// <summary>
    /// Determines if the align vertical centers command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnAlignVerticalCentersCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnAlignVerticalCentersCanExecute(e);

    /// <summary>
    /// Checks if the align vertical centers command can be executed based on the selected items.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnAlignVerticalCentersCanExecute(CanExecuteRoutedEventArgs e) =>
        //var groupedItem = from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
        //                  where item.ParentID == Guid.Empty
        //                  select item;


        //e.CanExecute = groupedItem.Count() > 1;
        e.CanExecute = true;

    /// <summary>
    /// Executes the align bottom command for selected items.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnAlignBottomExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnAlignBottomExecuted(e);

    /// <summary>
    /// Aligns the bottom of the selected items to the bottom of the first selected item.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnAlignBottomExecuted(ExecutedRoutedEventArgs e)
    {
        var selectedItems = from DesignLayer item in SelectedItems.Select(x => this.ContainerFromItem(x))
                            where item.ParentID == Guid.Empty
                            select item;

        if (selectedItems.Count() > 1)
        {
            double bottom = Canvas.GetTop(selectedItems.First()) + selectedItems.First().Height;

            foreach (var item in selectedItems)
            {
                double delta = bottom - (Canvas.GetTop(item) + item.Height);
                foreach (DesignLayer di in GetGroupMembers(item))
                    Canvas.SetTop(di, Canvas.GetTop(di) + delta);
            }
        }
    }

    /// <summary>
    /// Determines if the align bottom command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnAlignBottomCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnAlignBottomCanExecute(e);

    /// <summary>
    /// Checks if the align bottom command can be executed based on the selected items.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnAlignBottomCanExecute(CanExecuteRoutedEventArgs e) =>
        //var groupedItem = from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
        //                  where item.ParentID == Guid.Empty
        //                  select item;


        //e.CanExecute = groupedItem.Count() > 1;
        e.CanExecute = true;

    /// <summary>
    /// Executes the align left command for selected items.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnAlignLeftExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnAlignLeftExecuted(e);

    /// <summary>
    /// Aligns the left edge of the selected items to the left edge of the first selected item.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnAlignLeftExecuted(ExecutedRoutedEventArgs e)
    {
        var selectedItems = from DesignLayer item in SelectedItems.Select(x => this.ContainerFromItem(x))
                            where item.ParentID == Guid.Empty
                            select item;

        if (selectedItems.Count() > 1)
        {
            double left = Canvas.GetLeft(selectedItems.First());

            foreach (var item in selectedItems)
            {
                double delta = left - Canvas.GetLeft(item);
                foreach (DesignLayer di in GetGroupMembers(item))
                {
                    Canvas.SetLeft(di, Canvas.GetLeft(di) + delta);
                }
            }
        }
    }

    /// <summary>
    /// Determines if the align left command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnAlignLeftCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnAlignLeftCanExecute(e);

    /// <summary>
    /// Checks if the align left command can be executed based on the selected items.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnAlignLeftCanExecute(CanExecuteRoutedEventArgs e) =>
        //var groupedItem = from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
        //                  where item.ParentID == Guid.Empty
        //                  select item;

        //e.CanExecute = groupedItem.Count() > 1;
        e.CanExecute = true;

    /// <summary>
    /// Executes the align horizontal centers command for selected items.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnAlignHorizontalCentersExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnAlignHorizontalCentersExecuted(e);

    /// <summary>
    /// Aligns the horizontal centers of the selected items to the center of the first selected item.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnAlignHorizontalCentersExecuted(ExecutedRoutedEventArgs e)
    {
        var selectedItems = from DesignLayer item in SelectedItems.Select(x => this.ContainerFromItem(x))
                            where item.ParentID == Guid.Empty
                            select item;

        if (selectedItems.Count() > 1)
        {
            double center = Canvas.GetLeft(selectedItems.First()) + selectedItems.First().Width / 2;

            foreach (var item in selectedItems)
            {
                double delta = center - (Canvas.GetLeft(item) + item.Width / 2);
                foreach (DesignLayer di in GetGroupMembers(item))
                    Canvas.SetLeft(di, Canvas.GetLeft(di) + delta);
            }
        }
    }

    /// <summary>
    /// Determines if the align horizontal centers command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnAlignHorizontalCentersCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnAlignHorizontalCentersCanExecute(e);

    /// <summary>
    /// Checks if the align horizontal centers command can be executed based on the selected items.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnAlignHorizontalCentersCanExecute(CanExecuteRoutedEventArgs e) =>
        //var groupedItem = from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
        //                  where item.ParentID == Guid.Empty
        //                  select item;

        //e.CanExecute = groupedItem.Count() > 1;
        e.CanExecute = true;

    /// <summary>
    /// Executes the align right command for selected items.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnAlignRightExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnAlignRightExecuted(e);

    /// <summary>
    /// Aligns the right edge of the selected items to the right edge of the first selected item.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnAlignRightExecuted(ExecutedRoutedEventArgs e)
    {
        var selectedItems = from DesignLayer item in SelectedItems.Select(x => this.ContainerFromItem(x))
                            where item.ParentID == Guid.Empty
                            select item;

        if (selectedItems.Count() > 1)
        {
            double right = Canvas.GetLeft(selectedItems.First()) + selectedItems.First().Width;

            foreach (var item in selectedItems)
            {
                double delta = right - (Canvas.GetLeft(item) + item.Width);
                foreach (DesignLayer di in GetGroupMembers(item))
                    Canvas.SetLeft(di, Canvas.GetLeft(di) + delta);
            }
        }
    }

    /// <summary>
    /// Determines if the align right command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnAlignRightCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnAlignRightCanExecute(e);

    /// <summary>
    /// Checks if the align right command can be executed based on the selected items.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnAlignRightCanExecute(CanExecuteRoutedEventArgs e) =>
        //var groupedItem = from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
        //                  where item.ParentID == Guid.Empty
        //                  select item;


        //e.CanExecute = groupedItem.Count() > 1;
        e.CanExecute = true;

    /// <summary>
    /// Executes the distribute horizontal command for selected items.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnDistributeHorizontalExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnDistributeHorizontalExecuted(e);

    /// <summary>
    /// Distributes the selected items evenly along the horizontal axis.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnDistributeHorizontalExecuted(ExecutedRoutedEventArgs e)
    {
        var selectedItems = from DesignLayer item in SelectedItems.Select(x => this.ContainerFromItem(x))
                            where item.ParentID == Guid.Empty
                            let itemLeft = Canvas.GetLeft(item)
                            orderby itemLeft
                            select item;

        if (selectedItems.Count() > 1)
        {
            double left = Double.MaxValue;
            double right = Double.MinValue;
            double sumWidth = 0;
            foreach (var item in selectedItems)
            {
                left = Math.Min(left, Canvas.GetLeft(item));
                right = Math.Max(right, Canvas.GetLeft(item) + item.Width);
                sumWidth += item.Width;
            }

            double distance = Math.Max(0, (right - left - sumWidth) / (selectedItems.Count() - 1));
            double offset = Canvas.GetLeft(selectedItems.First());

            foreach (var item in selectedItems)
            {
                double delta = offset - Canvas.GetLeft(item);
                foreach (DesignLayer di in GetGroupMembers(item))
                    Canvas.SetLeft(di, Canvas.GetLeft(di) + delta);
                offset = offset + item.Width + distance;
            }
        }
    }

    /// <summary>
    /// Determines if the distribute horizontal command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnDistributeHorizontalCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnDistributeHorizontalCanExecute(e);

    /// <summary>
    /// Checks if the distribute horizontal command can be executed based on the selected items.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnDistributeHorizontalCanExecute(CanExecuteRoutedEventArgs e) =>
        //var groupedItem = from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
        //                  where item.ParentID == Guid.Empty
        //                  select item;

        //e.CanExecute = groupedItem.Count() > 1;
        e.CanExecute = true;

    /// <summary>
    /// Executes the distribute vertical command for selected items.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnDistributeVerticalExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnDistributeVerticalExecuted(e);

    /// <summary>
    /// Distributes the selected items evenly along the vertical axis.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnDistributeVerticalExecuted(ExecutedRoutedEventArgs e)
    {
        var selectedItems = from DesignLayer item in SelectedItems.Select(x => this.ContainerFromItem(x))
                            where item.ParentID == Guid.Empty
                            let itemTop = Canvas.GetTop(item)
                            orderby itemTop
                            select item;

        if (selectedItems.Count() > 1)
        {
            double top = Double.MaxValue;
            double bottom = Double.MinValue;
            double sumHeight = 0;
            foreach (var item in selectedItems)
            {
                top = Math.Min(top, Canvas.GetTop(item));
                bottom = Math.Max(bottom, Canvas.GetTop(item) + item.Height);
                sumHeight += item.Height;
            }

            double distance = Math.Max(0, (bottom - top - sumHeight) / (selectedItems.Count() - 1));
            double offset = Canvas.GetTop(selectedItems.First());

            foreach (var item in selectedItems)
            {
                double delta = offset - Canvas.GetTop(item);
                foreach (DesignLayer di in GetGroupMembers(item))
                    Canvas.SetTop(di, Canvas.GetTop(di) + delta);
                offset = offset + item.Height + distance;
            }
        }
    }

    /// <summary>
    /// Determines if the distribute vertical command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnDistributeVerticalCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnDistributeVerticalCanExecute(e);

    /// <summary>
    /// Checks if the distribute vertical command can be executed based on the selected items.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnDistributeVerticalCanExecute(CanExecuteRoutedEventArgs e) =>
        //var groupedItem = from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
        //                  where item.ParentID == Guid.Empty
        //                  select item;

        //e.CanExecute = groupedItem.Count() > 1;
        e.CanExecute = true;
    /// <summary>
    /// Executes the select all command.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnSelectAllExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnSelectAllExecuted(e);

    /// <summary>
    /// Selects all items in the canvas.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnSelectAllExecuted(ExecutedRoutedEventArgs e) =>
        SelectAll();

    /// <summary>
    /// Determines if the select all command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnSelectAllCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnSelectAllCanExecute(e);

    /// <summary>
    /// Checks if the select all command can be executed.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnSelectAllCanExecute(CanExecuteRoutedEventArgs e) =>
        e.CanExecute = true;

    /// <summary>
    /// Executes the save command for the canvas items.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnSaveExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnSaveExecuted(e);

    /// <summary>
    /// Saves the current state of the designer items to a file.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnSaveExecuted(ExecutedRoutedEventArgs e)
    {
        var designerItems = Items.Select(x => this.ContainerFromItem(x) as DesignLayer).OfType<DesignLayer>();
        var designerItemsXML = SerializeDesignerItems(designerItems);

        var root = new XElement("Root");
        root.Add(designerItemsXML);

        SaveFile(root);
    }

    /// <summary>
    /// Determines if the save command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnSaveCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnSaveCanExecute(e);

    /// <summary>
    /// Checks if the save command can be executed.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnSaveCanExecute(CanExecuteRoutedEventArgs e) =>
        e.CanExecute = true;

    /// <summary>
    /// Executes the new command to create a new canvas.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnNewExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnNewExecuted(e);

    /// <summary>
    /// Creates a new canvas by clearing existing items.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnNewExecuted(ExecutedRoutedEventArgs e)
    {
        var isReadOnly = ((IList)Items).IsReadOnly;

        if (!isReadOnly)
        {
            while (Items.Count > 0)
                Items.RemoveAt(0);
        }
        else
        {
            while (Items.Count > 0)
                EditableItems.RemoveAt(0);
        }
    }

    /// <summary>
    /// Determines if the new command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnNewCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnNewCanExecute(e);

    /// <summary>
    /// Checks if the new command can be executed based on editability.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnNewCanExecute(CanExecuteRoutedEventArgs e) =>
        e.CanExecute = CanEdit;

    /// <summary>
    /// Executes the open command to load items from a file.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnOpenExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnOpenExecuted(e);

    /// <summary>
    /// Opens and deserializes items from a saved file into the canvas.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnOpenExecuted(ExecutedRoutedEventArgs e)
    {
        var root = LoadSerializedDataFromFile();

        if (root == null)
            return;

        NewCommand.Execute(null, this);

        var itemsXML = root.Elements("DesignerItems").Elements("DesignerItem");
        foreach (var itemXML in itemsXML)
        {
            _ = Guid.TryParse(itemXML.Element("ID")?.Value, out var id);
            var item = AddNewItem();
            if (item == null || this.ContainerFromItem(item) is not DesignLayer container) continue;
            container.Deserialize(itemXML, id, 0, 0);
        }

        InvalidateVisual();
    }

    /// <summary>
    /// Determines if the open command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnOpenCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnOpenCanExecute(e);

    /// <summary>
    /// Checks if the open command can be executed based on editability.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnOpenCanExecute(CanExecuteRoutedEventArgs e) =>
        e.CanExecute = CanEdit;

    /// <summary>
    /// Executes the copy command for selected items.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnCopyExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnCopyExecuted(e);

    /// <summary>
    /// Copies the selected items to the clipboard as XML.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnCopyExecuted(ExecutedRoutedEventArgs e)
    {
        var designerItemsXML = SerializeDesignerItems(SelectedItems.Select(item => this.ContainerFromItem(item) as DesignLayer).OfType<DesignLayer>());

        var root = new XElement("Root");
        root.Add(designerItemsXML);

        root.Add(new XAttribute("OffsetX", 10));
        root.Add(new XAttribute("OffsetY", 10));

        Clipboard.Clear();
        Clipboard.SetData(DataFormats.Xaml, root);
    }

    /// <summary>
    /// Determines if the copy command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnCopyCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnCopyCanExecute(e);

    /// <summary>
    /// Checks if the copy command can be executed based on selected items.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnCopyCanExecute(CanExecuteRoutedEventArgs e) =>
        e.CanExecute = SelectedItems.Count > 0;

    /// <summary>
    /// Executes the paste command to add copied items to the canvas.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnPasteExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnPasteExecuted(e);

    /// <summary>
    /// Pastes items from the clipboard into the canvas.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnPasteExecuted(ExecutedRoutedEventArgs e)
    {
        var root = LoadSerializedDataFromClipBoard();

        if (root == null)
            return;

        var mappingOldToNewIDs = new Dictionary<Guid, Guid>();
        var newItems = new List<DesignLayer>();
        var itemsXML = root.Elements("DesignerItems").Elements("DesignerItem");

        _ = double.TryParse(root.Attribute("OffsetX")?.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var offsetX);
        _ = double.TryParse(root.Attribute("OffsetY")?.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var offsetY);

        foreach (var itemXML in itemsXML)
        {
            _ = Guid.TryParse(itemXML.Element("ID")?.Value, out var oldID);
            var newID = Guid.NewGuid();
            mappingOldToNewIDs.Add(oldID, newID);
            var item = AddNewItem();
            if (item == null || this.ContainerFromItem(item) is not DesignLayer container) continue;
            container.Deserialize(itemXML, newID, offsetX, offsetY);
            newItems.Add(container);
        }

        UnselectAll();

        foreach (var item in newItems)
        {
            if (item.ParentID != Guid.Empty)
                item.ParentID = mappingOldToNewIDs[item.ParentID];
        }

        foreach (var item in newItems)
        {
            if (item.ParentID == Guid.Empty)
                Select(item);
        }

        BringToFrontCommand.Execute(null, this);

        // Update paste offset
        if (root.Attribute("OffsetX") is XAttribute offsetXAttribute)
            offsetXAttribute.Value = (offsetX + 10).ToString();

        if (root.Attribute("OffsetY") is XAttribute offsetYAttribute)
            offsetYAttribute.Value = (offsetY + 10).ToString();

        Clipboard.Clear();
        Clipboard.SetData(DataFormats.Xaml, root);
    }

    /// <summary>
    /// Determines if the paste command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnPasteCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnPasteCanExecute(e);

    /// <summary>
    /// Checks if the paste command can be executed based on clipboard content.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnPasteCanExecute(CanExecuteRoutedEventArgs e) =>
        e.CanExecute = Clipboard.ContainsData(DataFormats.Xaml);

    /// <summary>
    /// Executes the delete command to remove selected items from the canvas.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnDeleteExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnDeleteExecuted(e);

    /// <summary>
    /// Deletes the currently selected items from the canvas.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnDeleteExecuted(ExecutedRoutedEventArgs e)
    {
        var isReadOnly = ((IList)Items).IsReadOnly;

        if (!isReadOnly)
        {
            while (SelectedItems.Count > 0)
            {
                var item = SelectedItems[0];
                Items.Remove(item);
            }
        }
        else
        {
            while (SelectedItems.Count > 0)
            {
                var item = SelectedItems[0];
                EditableItems.Remove(item);
            }
        }

        UnselectAll();
        UpdateZIndex();
    }

    /// <summary>
    /// Determines if the delete command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnDeleteCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnDeleteCanExecute(e);

    /// <summary>
    /// Checks if the delete command can be executed based on selected items.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnDeleteCanExecute(CanExecuteRoutedEventArgs e) =>
        e.CanExecute = SelectedItems.Count > 0 && CanEdit;

    /// <summary>
    /// Executes the cut command to remove selected items after copying them.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The executed routed event arguments.</param>
    private static void OnCutExecuted(object sender, ExecutedRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnCutExecuted(e);

    /// <summary>
    /// Cuts the selected items by copying them and then deleting them.
    /// </summary>
    /// <param name="e">The executed routed event arguments.</param>
    protected virtual void OnCutExecuted(ExecutedRoutedEventArgs e)
    {
        CopyCommand.Execute(null, this);
        DeleteCommand.Execute(null, this);
    }

    /// <summary>
    /// Determines if the cut command can be executed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The can execute routed event arguments.</param>
    private static void OnCutCanExecute(object sender, CanExecuteRoutedEventArgs e) =>
        (sender as DesignCanvas)?.OnCutCanExecute(e);

    /// <summary>
    /// Checks if the cut command can be executed based on selected items.
    /// </summary>
    /// <param name="e">The can execute routed event arguments.</param>
    protected virtual void OnCutCanExecute(CanExecuteRoutedEventArgs e) =>
        e.CanExecute = SelectedItems.Count > 0 && CanEdit;

    #endregion
}