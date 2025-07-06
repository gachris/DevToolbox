using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml;
using DevToolbox.Wpf.Serialization;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A panel that manages a group of docking elements within a DockManager, allowing nested splitting and resizing.
/// Implements <see cref="ILayoutSerializable"/> for XML-based layout persistence.
/// </summary>
public class LayoutDockGroupPanel : Grid, ILayoutSerializable
{
    /// <summary>
    /// Represents a hierarchical grouping of dockable or document elements within a <see cref="LayoutManager"/>.
    /// A group can contain either an <see cref="AttachedElement"/>,
    /// or two child <see cref="LayoutDockGroup"/> instances split by a <see cref="Dock"/> position.
    /// </summary>
    class LayoutDockGroup : ILayoutSerializable
    {
        #region Fields/Consts

        /// <summary>
        /// The first (or left/top) child group.
        /// </summary>
        private LayoutDockGroup? _firstChild;

        /// <summary>
        /// The second (or right/bottom) child group.
        /// </summary>
        private LayoutDockGroup? _secondChild;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the docking direction separating child groups.
        /// </summary>
        public Dock Dock { get; set; }

        /// <summary>
        /// Gets or sets the UIElement attached directly to this group
        /// (null if the group is purely a splitter for two children).
        /// </summary>
        public UIElement? AttachedElement { get; set; }

        /// <summary>
        /// Gets or sets the first child group (left or top).
        /// </summary>
        public LayoutDockGroup? FirstChild
        {
            get => _firstChild;
            set
            {
                if (value is not null)
                    value.ParentGroup = this;
                _firstChild = value;
            }
        }

        /// <summary>
        /// Gets or sets the second child group (right or bottom).
        /// </summary>
        public LayoutDockGroup? SecondChild
        {
            get => _secondChild;
            set
            {
                if (value is not null)
                    value.ParentGroup = this;
                _secondChild = value;
            }
        }

        /// <summary>
        /// Gets the parent group in the hierarchy, or null if this is the root.
        /// </summary>
        public LayoutDockGroup? ParentGroup { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the group (and all its children) is hidden.
        /// </summary>
        public bool IsHidden
        {
            get
            {
                if (AttachedElement is LayoutDockItemsControl dockable)
                    return dockable.IsHidden;

                return (FirstChild is not null || SecondChild is not null) && (FirstChild?.IsHidden ?? true) && (SecondChild?.IsHidden ?? false);
            }
        }

        /// <summary>
        /// Gets the combined width of this group as a <see cref="GridLength"/>,
        /// computed from attached element or child group sizes.
        /// </summary>
        public GridLength GroupWidth
        {
            get
            {
                if (AttachedElement is LayoutDockItemsControl dockable)
                    return new GridLength(dockable.PaneWidth, GridUnitType.Pixel);

                if (Dock is Dock.Left or Dock.Right)
                {
                    double first = FirstChild?.GroupWidth.Value ?? 0;
                    double second = SecondChild?.GroupWidth.Value ?? 0;
                    return new GridLength(first + second + 4, GridUnitType.Pixel);
                }

                return FirstChild?.GroupWidth ?? GridLength.Auto;
            }
        }

        /// <summary>
        /// Gets the combined height of this group as a <see cref="GridLength"/>,
        /// computed from attached element or child group sizes.
        /// </summary>
        public GridLength GroupHeight
        {
            get
            {
                if (AttachedElement is LayoutDockItemsControl dockable)
                    return new GridLength(dockable.PaneHeight, GridUnitType.Pixel);

                if (Dock is Dock.Top or Dock.Bottom)
                {
                    double first = FirstChild?.GroupHeight.Value ?? 0;
                    double second = SecondChild?.GroupHeight.Value ?? 0;
                    return new GridLength(first + second + 4, GridUnitType.Pixel);
                }

                return FirstChild?.GroupHeight ?? GridLength.Auto;
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new empty <see cref="LayoutDockGroup"/>.
        /// Used for deserialization purposes.
        /// </summary>
        public LayoutDockGroup() { }

        /// <summary>
        /// Initializes a new <see cref="LayoutDockGroup"/> that directly holds a single <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The element to attach to this group.</param>
        public LayoutDockGroup(UIElement element) => AttachedElement = element;

        /// <summary>
        /// Initializes a new split <see cref="LayoutDockGroup"/>, combining two child groups at the specified <paramref name="groupDock"/>.
        /// </summary>
        /// <param name="firstGroup">The first (left/top) child group.</param>
        /// <param name="secondGroup">The second (right/bottom) child group.</param>
        /// <param name="groupDock">The docking side where the split occurs.</param>
        public LayoutDockGroup(LayoutDockGroup firstGroup, LayoutDockGroup secondGroup, Dock groupDock)
        {
            FirstChild = firstGroup;
            SecondChild = secondGroup;
            Dock = groupDock;
        }

        #region Methods


        /// <summary>
        /// Finds the direct parent element that contains the specified <paramref name="content"/>.
        /// </summary>
        /// <param name="content">The child element to locate.</param>
        /// <returns>The UIElement acting as the container, or null if not found.</returns>
        public UIElement? GetParentFromContent(UIElement content)
        {
            if (AttachedElement is ItemsControl list && list.Items.Contains(content))
                return AttachedElement;

            return FirstChild?.GetParentFromContent(content)
                   ?? SecondChild?.GetParentFromContent(content);
        }

        /// <summary>
        /// Finds the <see cref="LayoutDockGroup"/> that directly contains the specified control.
        /// </summary>
        /// <param name="control">The control to locate.</param>
        /// <returns>The group containing the control, or null if not found.</returns>
        public LayoutDockGroup? GetDockableGroup(UIElement control)
        {
            if (AttachedElement == control)
                return this;

            return FirstChild?.GetDockableGroup(control)
                   ?? SecondChild?.GetDockableGroup(control);
        }

        /// <summary>
        /// Replaces a child group within this group hierarchy.
        /// </summary>
        /// <param name="groupToFind">The existing child group to replace.</param>
        /// <param name="groupToReplace">The new group instance.</param>
        public void ReplaceChildGroup(LayoutDockGroup groupToFind, LayoutDockGroup groupToReplace)
        {
            if (FirstChild == groupToFind) FirstChild = groupToReplace;
            else if (SecondChild == groupToFind) SecondChild = groupToReplace;
        }

        /// <summary>
        /// Adds a new control at the specified <paramref name="dock"/>, returning the new root group.
        /// </summary>
        /// <param name="control">The UIElement to add.</param>
        /// <param name="dock">The docking side relative to this group.</param>
        /// <returns>The updated root <see cref="LayoutDockGroup"/>.</returns>
        public LayoutDockGroup Add(UIElement control, Dock dock) => dock switch
        {
            Dock.Right or Dock.Bottom => new LayoutDockGroup(this, new LayoutDockGroup(control), dock),
            Dock.Left or Dock.Top => new LayoutDockGroup(new LayoutDockGroup(control), this, dock),
            _ => throw new ArgumentOutOfRangeException(nameof(dock)),
        };

        /// <summary>
        /// Removes the specified control from the group hierarchy.
        /// </summary>
        /// <param name="control">The control to remove.</param>
        /// <returns>
        /// A replacement group if this one collapses, or null if no replacement is necessary.
        /// </returns>
        public LayoutDockGroup? Remove(UIElement control)
        {
            if (AttachedElement is not null)
                return null;

            if (FirstChild?.AttachedElement == control)
                return SecondChild;
            if (SecondChild?.AttachedElement == control)
                return FirstChild;

            var result = FirstChild?.Remove(control);
            if (result is not null)
            {
                FirstChild = result;
                return null;
            }

            result = SecondChild?.Remove(control);
            if (result is not null)
            {
                SecondChild = result;
                return null;
            }

            return null;
        }

        /// <summary>
        /// Recursively saves the size of any <see cref="LayoutDockItemsControl"/> instances in this group.
        /// </summary>
        public void SaveChildSize()
        {
            if (AttachedElement is LayoutDockItemsControl dockable)
                dockable.SaveSize();
            else
            {
                FirstChild?.SaveChildSize();
                SecondChild?.SaveChildSize();
            }
        }

        #endregion

        #region ILayoutSerializable

        /// <inheritdoc/>
        public void Serialize(XmlDocument doc, XmlNode parentNode)
        {
            if (parentNode.Attributes is not null)
            {
                parentNode.Attributes.Append(doc.CreateAttribute(nameof(Dock)));
                parentNode.Attributes[nameof(Dock)]!.Value = Dock.ToString();
            }

            if (AttachedElement is LayoutDockItemsControl dockable)
            {
                var nodeAttachedControl = dockable is LayoutDockItemsControl ? doc.CreateElement(nameof(LayoutDockItemsControl)) : (XmlNode)doc.CreateElement(nameof(LayoutItemsControl));
                dockable.Serialize(doc, nodeAttachedControl);
                parentNode.AppendChild(nodeAttachedControl);
            }
            else
            {
                var nodeChildGroups = doc.CreateElement("ChildGroups");

                var nodeFirstChildGroup = doc.CreateElement(nameof(FirstChild));
                FirstChild?.Serialize(doc, nodeFirstChildGroup);
                nodeChildGroups.AppendChild(nodeFirstChildGroup);

                var nodeSecondChildGroup = doc.CreateElement(nameof(SecondChild));
                SecondChild?.Serialize(doc, nodeSecondChildGroup);
                nodeChildGroups.AppendChild(nodeSecondChildGroup);

                parentNode.AppendChild(nodeChildGroups);
            }
        }

        /// <inheritdoc/>
        public void Deserialize(LayoutManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (managerToAttach == null) throw new ArgumentNullException(nameof(managerToAttach));
            if (getObjectHandler == null) throw new ArgumentNullException(nameof(getObjectHandler));

            var dockAttr = node.Attributes?[nameof(Dock)];
            if (dockAttr == null || string.IsNullOrWhiteSpace(dockAttr.Value))
                throw new InvalidOperationException($"Missing or empty '{nameof(Dock)}' attribute on '{node.Name}' element.");
            Dock = (Dock)Enum.Parse(typeof(Dock), dockAttr.Value, ignoreCase: true);

            if (node.ChildNodes.Count == 0)
                throw new InvalidOperationException($"Expected at least one child node under '{node.Name}', but found none.");

            var firstChildNode = node.ChildNodes[0];
            if (firstChildNode is null)
            {
                return;
            }

            switch (firstChildNode.Name)
            {
                case nameof(LayoutDockItemsControl):
                    var dockable = new LayoutDockItemsControl
                    {
                        DockManager = managerToAttach
                    };
                    dockable.Deserialize(managerToAttach, firstChildNode, getObjectHandler);
                    AttachedElement = dockable;
                    break;

                case nameof(LayoutItemsControl):
                    var document = new LayoutItemsControl
                    {
                        DockManager = managerToAttach
                    };
                    document.Deserialize(managerToAttach, firstChildNode, getObjectHandler);
                    AttachedElement = document;
                    break;

                default:
                    // Expecting a nested DockingGroup element
                    if (firstChildNode.ChildNodes == null || firstChildNode.ChildNodes.Count < 2)
                    {
                        throw new InvalidOperationException(
                            $"Expected two child groups under '{firstChildNode.Name}', but found {(firstChildNode.ChildNodes?.Count ?? 0)}.");
                    }

                    var leftNode = firstChildNode.ChildNodes[0];
                    var rightNode = firstChildNode.ChildNodes[1];

                    if (leftNode == null || rightNode == null)
                    {
                        throw new InvalidOperationException(
                            $"Child nodes under '{firstChildNode.Name}' contain null entries.");
                    }

                    FirstChild = new LayoutDockGroup { ParentGroup = this };
                    FirstChild.Deserialize(managerToAttach, leftNode, getObjectHandler);

                    SecondChild = new LayoutDockGroup { ParentGroup = this };
                    SecondChild.Deserialize(managerToAttach, rightNode, getObjectHandler);
                    break;
            }
        }

        #endregion
    }

    #region Fields/Consts

    private LayoutManager? _owner;
    private LayoutDockGroup? _dockableGroup;

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
    /// Initializes the docking group with a document list control.
    /// </summary>
    /// <param name="listControl">The control that hosts the document list.</param>
    internal void AttachDocumentList(UIElement listControl)
    {
        _dockableGroup = new LayoutDockGroup(listControl);

        if (_owner is not null && _owner.Items.Count > 0)
        {
            _owner.Items.Refresh();
        }
        else
        {
            ArrangeLayout();
        }
    }

    /// <summary>
    /// Clears children and definitions then arranges the layout based on the current docking group.
    /// </summary>
    private void ArrangeLayout()
    {
        if (_dockableGroup == null) throw new Exception();

        _dockableGroup.SaveChildSize();

        Clear(this);
        Arrange(this, _dockableGroup);
        InvalidateArrange();
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

    /// <summary>
    /// Recursively arranges the content of the grid based on the specified docking group structure.
    /// </summary>
    /// <param name="grid">Target grid for arrangement.</param>
    /// <param name="dockableGroup">The docking group describing layout.</param>
    private static void Arrange(Grid grid, LayoutDockGroup dockableGroup)
    {
        if (dockableGroup.AttachedElement is not null)
        {
            if (dockableGroup.AttachedElement is LayoutGroupItemsControl)
            {
                grid.Children.Add(dockableGroup.AttachedElement);
            }
            else
            {
                grid.Children.Add(new ContentControl { Content = dockableGroup.AttachedElement });
            }
        }
        else if (dockableGroup.FirstChild is not null && dockableGroup.FirstChild.IsHidden && dockableGroup.SecondChild is not null && !dockableGroup.SecondChild.IsHidden)
        {
            LayoutDockGroupPanel.Arrange(grid, dockableGroup.SecondChild);
        }
        else if (dockableGroup.FirstChild is not null && !dockableGroup.FirstChild.IsHidden && dockableGroup.SecondChild is not null && dockableGroup.SecondChild.IsHidden)
        {
            LayoutDockGroupPanel.Arrange(grid, dockableGroup.FirstChild);
        }
        else if (dockableGroup.FirstChild is not null && dockableGroup.SecondChild is not null)
        {
            if (dockableGroup.Dock is Dock.Left or Dock.Right)
            {
                grid.ColumnDefinitions.Add(new());
                grid.ColumnDefinitions.Add(new());
                grid.ColumnDefinitions.Add(new());
                grid.ColumnDefinitions[0].Width = dockableGroup.Dock == Dock.Left ? dockableGroup.FirstChild.GroupWidth : new(1, GridUnitType.Star);
                grid.ColumnDefinitions[1].Width = new(1, GridUnitType.Auto);
                grid.ColumnDefinitions[2].Width = dockableGroup.Dock == Dock.Right ? dockableGroup.SecondChild.GroupWidth : new(1, GridUnitType.Star);

                var firstChildGrid = new Grid();
                firstChildGrid.SetValue(ColumnProperty, 0);
                LayoutDockGroupPanel.Arrange(firstChildGrid, dockableGroup.FirstChild);
                grid.Children.Add(firstChildGrid);

                var splitter = new GridSplitter()
                {
                    Background = System.Windows.Media.Brushes.Transparent,
                    ShowsPreview = true,
                    Width = 5
                };
                splitter.SetValue(ColumnProperty, 1);
                splitter.ResizeDirection = GridResizeDirection.Columns;
                splitter.ResizeBehavior = GridResizeBehavior.PreviousAndNext;
                splitter.VerticalAlignment = VerticalAlignment.Stretch;
                grid.Children.Add(splitter);

                var secondChildGrid = new Grid();
                secondChildGrid.SetValue(ColumnProperty, 2);
                LayoutDockGroupPanel.Arrange(secondChildGrid, dockableGroup.SecondChild);
                grid.Children.Add(secondChildGrid);
            }
            else
            {
                grid.RowDefinitions.Add(new());
                grid.RowDefinitions.Add(new());
                grid.RowDefinitions.Add(new());
                grid.RowDefinitions[0].Height = dockableGroup.Dock == Dock.Top ? dockableGroup.FirstChild.GroupHeight : new(1, GridUnitType.Star);
                grid.RowDefinitions[1].Height = new(1, GridUnitType.Auto);
                grid.RowDefinitions[2].Height = dockableGroup.Dock == Dock.Bottom ? dockableGroup.SecondChild.GroupHeight : new(1, GridUnitType.Star);

                var firstChildGrid = new Grid();
                firstChildGrid.SetValue(RowProperty, 0);
                LayoutDockGroupPanel.Arrange(firstChildGrid, dockableGroup.FirstChild);
                grid.Children.Add(firstChildGrid);

                var splitter = new GridSplitter()
                {
                    Background = System.Windows.Media.Brushes.Transparent,
                    ShowsPreview = true,
                    Height = 5
                };
                splitter.SetValue(RowProperty, 1);
                splitter.ResizeDirection = GridResizeDirection.Rows;
                splitter.ResizeBehavior = GridResizeBehavior.PreviousAndNext;
                splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
                grid.Children.Add(splitter);

                var secondChildGrid = new Grid();
                secondChildGrid.SetValue(RowProperty, 2);
                LayoutDockGroupPanel.Arrange(secondChildGrid, dockableGroup.SecondChild);
                grid.Children.Add(secondChildGrid);
            }
        }
    }

    /// <summary>
    /// Adds a new element to the root docking group at the specified dock position.
    /// </summary>
    /// <param name="element">The UI element to dock.</param>
    /// <param name="dock">The dock position relative to existing elements.</param>
    internal void Add(UIElement element, Dock dock)
    {
        _dockableGroup = _dockableGroup?.Add(element, dock);
        ArrangeLayout();
    }

    /// <summary>
    /// Adds a new element relative to an existing element at the specified dock position.
    /// </summary>
    /// <param name="element">The UI element to dock.</param>
    /// <param name="relativeElement">Existing element to dock relative to.</param>
    /// <param name="relativeDock">The dock position relative to the existing element.</param>
    internal void Add(UIElement element, UIElement relativeElement, Dock relativeDock)
    {
        if (_dockableGroup?.GetDockableGroup(relativeElement) is not LayoutDockGroup dockableGroup) return;

        switch (relativeDock)
        {
            case Dock.Right:
            case Dock.Bottom:
                {
                    if (dockableGroup == _dockableGroup)
                        _dockableGroup = new LayoutDockGroup(dockableGroup, new(element), relativeDock);
                    else
                    {
                        var parentGroup = dockableGroup.ParentGroup;
                        var newChildGroup = new LayoutDockGroup(dockableGroup, new(element), relativeDock);
                        parentGroup?.ReplaceChildGroup(dockableGroup, newChildGroup);
                    }
                }
                break;
            case Dock.Left:
            case Dock.Top:
                {
                    if (dockableGroup == _dockableGroup)
                        _dockableGroup = new(new(element), dockableGroup, relativeDock);
                    else
                    {
                        var parentGroup = dockableGroup.ParentGroup;
                        var newChildGroup = new LayoutDockGroup(new(element), dockableGroup, relativeDock);
                        parentGroup?.ReplaceChildGroup(dockableGroup, newChildGroup);
                    }
                }
                break;
        }

        ArrangeLayout();
    }

    /// <summary>
    /// Removes a docked control from the layout.
    /// </summary>
    /// <param name="control">The UI element to remove.</param>
    internal void Remove(UIElement control)
    {
        var groupToAttach = _dockableGroup?.Remove(control);

        if (groupToAttach != null)
        {
            _dockableGroup = groupToAttach;
            _dockableGroup.ParentGroup = null;
        }

        ArrangeLayout();
    }

    /// <summary>
    /// Attaches the panel to the parent DockManager and listens for item changes.
    /// </summary>
    private void AttachToOwner()
    {
        if (_owner != null) _owner.ItemContainerGenerator.ItemsChanged -= ItemContainerGenerator_ItemsChanged;

        _owner = TemplatedParent as LayoutManager;

        if (_owner != null)
        {
            _owner.ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
            if (_owner.Items.Count > 0 && _dockableGroup != null) _owner.Items.Refresh();
        }
    }

    /// <summary>
    /// Handles changes in the DockManager's item collection to regenerate containers.
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

    #endregion

    #region ILayoutSerializable

    /// <inheritdoc/>
    public void Serialize(XmlDocument doc, XmlNode parentNode)
    {
        var rootGroupNode = doc.CreateElement("_dockablePaneGroup");
        _dockableGroup?.Serialize(doc, parentNode);
        parentNode.AppendChild(rootGroupNode);
    }

    /// <inheritdoc/>
    public void Deserialize(LayoutManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
    {
        _dockableGroup = new LayoutDockGroup();
        _dockableGroup.Deserialize(managerToAttach, node, getObjectHandler);
        InvalidateArrange();
    }

    #endregion
}
