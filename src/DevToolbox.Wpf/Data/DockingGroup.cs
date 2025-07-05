using System;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using DevToolbox.Wpf.Controls;
using DevToolbox.Wpf.Serialization;

namespace DevToolbox.Wpf.Data;

/// <summary>
/// Represents a hierarchical grouping of dockable or document elements within a <see cref="DockManager"/>.
/// A group can contain either an <see cref="AttachedElement"/>,
/// or two child <see cref="DockingGroup"/> instances split by a <see cref="Dock"/> position.
/// </summary>
internal class DockingGroup : ILayoutSerializable
{
    #region Fields/Consts

    /// <summary>
    /// The first (or left/top) child group.
    /// </summary>
    private DockingGroup? _firstChild;

    /// <summary>
    /// The second (or right/bottom) child group.
    /// </summary>
    private DockingGroup? _secondChild;

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
    public DockingGroup? FirstChild
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
    public DockingGroup? SecondChild
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
    public DockingGroup? ParentGroup { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the group (and all its children) is hidden.
    /// </summary>
    public bool IsHidden
    {
        get
        {
            if (AttachedElement is DockableControl dockable)
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
            if (AttachedElement is DockableControl dockable)
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
            if (AttachedElement is DockableControl dockable)
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
    /// Initializes a new empty <see cref="DockingGroup"/>.
    /// Used for deserialization purposes.
    /// </summary>
    public DockingGroup() { }

    /// <summary>
    /// Initializes a new <see cref="DockingGroup"/> that directly holds a single <paramref name="element"/>.
    /// </summary>
    /// <param name="element">The element to attach to this group.</param>
    public DockingGroup(UIElement element) => AttachedElement = element;

    /// <summary>
    /// Initializes a new split <see cref="DockingGroup"/>, combining two child groups at the specified <paramref name="groupDock"/>.
    /// </summary>
    /// <param name="firstGroup">The first (left/top) child group.</param>
    /// <param name="secondGroup">The second (right/bottom) child group.</param>
    /// <param name="groupDock">The docking side where the split occurs.</param>
    public DockingGroup(DockingGroup firstGroup, DockingGroup secondGroup, Dock groupDock)
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
    /// Finds the <see cref="DockingGroup"/> that directly contains the specified control.
    /// </summary>
    /// <param name="control">The control to locate.</param>
    /// <returns>The group containing the control, or null if not found.</returns>
    public DockingGroup? GetDockableGroup(UIElement control)
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
    public void ReplaceChildGroup(DockingGroup groupToFind, DockingGroup groupToReplace)
    {
        if (FirstChild == groupToFind) FirstChild = groupToReplace;
        else if (SecondChild == groupToFind) SecondChild = groupToReplace;
    }

    /// <summary>
    /// Adds a new control at the specified <paramref name="dock"/>, returning the new root group.
    /// </summary>
    /// <param name="control">The UIElement to add.</param>
    /// <param name="dock">The docking side relative to this group.</param>
    /// <returns>The updated root <see cref="DockingGroup"/>.</returns>
    public DockingGroup Add(UIElement control, Dock dock) => dock switch
    {
        Dock.Right or Dock.Bottom => new DockingGroup(this, new DockingGroup(control), dock),
        Dock.Left or Dock.Top => new DockingGroup(new DockingGroup(control), this, dock),
        _ => throw new ArgumentOutOfRangeException(nameof(dock)),
    };

    /// <summary>
    /// Removes the specified control from the group hierarchy.
    /// </summary>
    /// <param name="control">The control to remove.</param>
    /// <returns>
    /// A replacement group if this one collapses, or null if no replacement is necessary.
    /// </returns>
    public DockingGroup? Remove(UIElement control)
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
    /// Recursively saves the size of any <see cref="DockableControl"/> instances in this group.
    /// </summary>
    public void SaveChildSize()
    {
        if (AttachedElement is DockableControl dockable)
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

        if (AttachedElement is DockableControl dockable)
        {
            var nodeAttachedControl = dockable is DockableControl ? doc.CreateElement(nameof(DockableControl)) : (XmlNode)doc.CreateElement(nameof(DocumentControl));
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
    public void Deserialize(DockManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
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
            case nameof(DockableControl):
                var dockable = new DockableControl
                {
                    DockManager = managerToAttach
                };
                dockable.Deserialize(managerToAttach, firstChildNode, getObjectHandler);
                AttachedElement = dockable;
                break;

            case nameof(DocumentControl):
                var document = new DocumentControl
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

                FirstChild = new DockingGroup { ParentGroup = this };
                FirstChild.Deserialize(managerToAttach, leftNode, getObjectHandler);

                SecondChild = new DockingGroup { ParentGroup = this };
                SecondChild.Deserialize(managerToAttach, rightNode, getObjectHandler);
                break;
        }
    }

    #endregion
}
