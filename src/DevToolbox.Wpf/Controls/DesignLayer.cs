using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a design layer in a design canvas, which supports grouping, selection, and drag functionality.
/// </summary>
public class DesignLayer : ContentControl, ISelectable, IGroupable
{
    #region Fields/Consts

    /// <summary>
    /// Unique identifier for the design layer.
    /// </summary>
    private readonly Guid _id;

    /// <summary>
    /// Event for when the layer is selected.
    /// </summary>
    public static readonly RoutedEvent SelectedEvent = Selector.SelectedEvent.AddOwner(typeof(DesignLayer));

    /// <summary>
    /// Event for when the layer is unselected.
    /// </summary>
    public static readonly RoutedEvent UnselectedEvent = Selector.UnselectedEvent.AddOwner(typeof(DesignLayer));

    /// <summary>
    /// Dependency property for the layer's position (read-only).
    /// </summary>
    private static readonly DependencyPropertyKey PositionPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(Position), typeof(Point), typeof(DesignLayer), new FrameworkPropertyMetadata(default));

    /// <summary>
    /// Public dependency property for the layer's position.
    /// </summary>
    public static readonly DependencyProperty PositionProperty = PositionPropertyKey.DependencyProperty;

    /// <summary>
    /// Dependency property to control whether dragging is allowed.
    /// </summary>
    public static readonly DependencyProperty AllowDragProperty =
        DependencyProperty.Register(nameof(AllowDrag), typeof(bool), typeof(DesignLayer), new FrameworkPropertyMetadata(true));

    /// <summary>
    /// Dependency property to indicate if the layer is selected.
    /// </summary>
    public static readonly DependencyProperty IsSelectedProperty =
        Selector.IsSelectedProperty.AddOwner(typeof(DesignLayer), new FrameworkPropertyMetadata(false, (d, e) => (d as DesignLayer)?.OnIsSelectedChanged(e)));

    /// <summary>
    /// Dependency property for the parent layer's ID.
    /// </summary>
    public static readonly DependencyProperty ParentIDProperty =
        DependencyProperty.Register(nameof(ParentID), typeof(Guid), typeof(DesignLayer));

    /// <summary>
    /// Dependency property to indicate if the layer is a group.
    /// </summary>
    public static readonly DependencyProperty IsGroupProperty =
        DependencyProperty.Register(nameof(IsGroup), typeof(bool), typeof(DesignLayer));

    /// <summary>
    /// Dependency property for the resize decorator template.
    /// </summary>
    public static readonly DependencyProperty ResizeDecoratorTemplateProperty =
        DependencyProperty.Register(nameof(ResizeDecoratorTemplate), typeof(ControlTemplate), typeof(DesignLayer));

    #endregion

    #region Events

    /// <summary>
    /// Event that occurs when the layer is selected.
    /// </summary>
    public event RoutedEventHandler Selected
    {
        add => AddHandler(SelectedEvent, value);
        remove => RemoveHandler(SelectedEvent, value);
    }

    /// <summary>
    /// Event that occurs when the layer is unselected.
    /// </summary>
    public event RoutedEventHandler Unselected
    {
        add => AddHandler(UnselectedEvent, value);
        remove => RemoveHandler(UnselectedEvent, value);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the unique ID of the design layer.
    /// </summary>
    public Guid ID => _id;

    /// <summary>
    /// Gets the internal parent of the design layer, if any.
    /// </summary>
    internal DesignCanvas? InternalParent => ItemsControl.ItemsControlFromItemContainer(this) as DesignCanvas;

    /// <summary>
    /// Gets or sets a value indicating whether the layer is selected.
    /// </summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the layer can be dragged.
    /// </summary>
    public bool AllowDrag
    {
        get => (bool)GetValue(AllowDragProperty);
        set => SetValue(AllowDragProperty, value);
    }

    /// <summary>
    /// Gets or sets the position of the design layer (internal set).
    /// </summary>
    public Point Position
    {
        get => (Point)GetValue(PositionProperty);
        internal set => SetValue(PositionPropertyKey, value);
    }

    /// <summary>
    /// Gets or sets the parent ID of the design layer.
    /// </summary>
    public Guid ParentID
    {
        get => (Guid)GetValue(ParentIDProperty);
        set => SetValue(ParentIDProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the design layer is a group.
    /// </summary>
    public bool IsGroup
    {
        get => (bool)GetValue(IsGroupProperty);
        set => SetValue(IsGroupProperty, value);
    }

    /// <summary>
    /// Gets or sets the template for the resize decorator.
    /// </summary>
    public ControlTemplate ResizeDecoratorTemplate
    {
        get => (ControlTemplate)GetValue(ResizeDecoratorTemplateProperty);
        set => SetValue(ResizeDecoratorTemplateProperty, value);
    }

    #endregion

    /// <summary>
    /// Static constructor to override the default style key.
    /// </summary>
    static DesignLayer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignLayer), new FrameworkPropertyMetadata(typeof(DesignLayer)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DesignLayer"/> class with a new unique ID.
    /// </summary>
    public DesignLayer() : this(Guid.NewGuid())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DesignLayer"/> class with a specified ID.
    /// </summary>
    /// <param name="id">The unique identifier for the design layer.</param>
    private DesignLayer(Guid id)
    {
        _id = id;
    }

    #region Methods Overrides

    /// <summary>
    /// Performs a hit test to determine whether the point is inside the layer's visual bounds.
    /// </summary>
    /// <param name="hitTestParameters">The parameters for the hit test.</param>
    /// <returns>A <see cref="HitTestResult"/> indicating the result of the hit test.</returns>
    protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
    {
        return VisualTreeHelper.GetDescendantBounds(this).Contains(hitTestParameters.HitPoint)
            ? new PointHitTestResult(this, hitTestParameters.HitPoint)
            : default!;
    }

    /// <summary>
    /// Performs a hit test with geometry to determine whether the point is inside the layer's geometry bounds.
    /// </summary>
    /// <param name="hitTestParameters">The parameters for the geometry hit test.</param>
    /// <returns>A <see cref="GeometryHitTestResult"/> indicating the result of the hit test.</returns>
    protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
    {
        var geometry = new RectangleGeometry(VisualTreeHelper.GetDescendantBounds(this));
        return new GeometryHitTestResult(this, geometry.FillContainsWithDetail(hitTestParameters.HitGeometry));
    }

    #endregion

    #region Methods

    /// <summary>
    /// Raises the appropriate event when the selection state of the layer changes.
    /// </summary>
    /// <param name="e">Event data containing the new selection state.</param>
    private void OnIsSelectedChanged(DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue)
        {
            RaiseEvent(new RoutedEventArgs(Selector.SelectedEvent, this));
        }
        else
        {
            RaiseEvent(new RoutedEventArgs(Selector.UnselectedEvent, this));
        }

        OnIsSelectedChanged((bool)e.NewValue);
    }

    /// <summary>
    /// Called when the selection state of the layer changes.
    /// </summary>
    /// <param name="isSelected">Indicates whether the layer is now selected.</param>
    protected virtual void OnIsSelectedChanged(bool isSelected)
    {
    }

    /// <summary>
    /// Serializes the design layer into an XML element.
    /// </summary>
    /// <returns>An <see cref="XElement"/> representing the serialized layer.</returns>
    public virtual XElement Serialize()
    {
        var contentXaml = XamlWriter.Save(Content);
        return new XElement(GetType().Name,
                   new XElement("Left", Canvas.GetLeft(this)),
                   new XElement("Top", Canvas.GetTop(this)),
                   new XElement("Width", Width),
                   new XElement("Height", Height),
                   new XElement("ID", ID),
                   new XElement("zIndex", Canvas.GetZIndex(this)),
                   new XElement("IsGroup", IsGroup),
                   new XElement("ParentID", ParentID),
                   new XElement("Content", contentXaml));
    }

    /// <summary>
    /// Deserializes the design layer from an XML element.
    /// </summary>
    /// <param name="itemXML">The XML element containing the serialized data.</param>
    /// <param name="id">The ID of the design layer.</param>
    /// <param name="OffsetX">The X offset for the layer's position.</param>
    /// <param name="OffsetY">The Y offset for the layer's position.</param>
    public virtual void Deserialize(XElement itemXML, Guid id, double OffsetX, double OffsetY)
    {
        _ = double.TryParse(itemXML.Element("Width")?.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var width);
        _ = double.TryParse(itemXML.Element("Height")?.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var height);
        _ = Guid.TryParse(itemXML.Element("ParentID")?.Value, out var parentID);
        _ = bool.TryParse(itemXML.Element("IsGroup")?.Value, out var isGroup);

        _ = double.TryParse(itemXML.Element("Left")?.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var left);
        _ = double.TryParse(itemXML.Element("Top")?.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var top);
        _ = int.TryParse(itemXML.Element("zIndex")?.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var zIndex);

        Width = width;
        Height = height;
        ParentID = parentID;
        IsGroup = isGroup;

        Canvas.SetLeft(this, left + OffsetX);
        Canvas.SetTop(this, top + OffsetY);
        Panel.SetZIndex(this, zIndex);

        var content = itemXML.Element("Content")?.Value ?? string.Empty;
        Content = XamlReader.Load(XmlReader.Create(new StringReader(content)));
    }

    #endregion
}
