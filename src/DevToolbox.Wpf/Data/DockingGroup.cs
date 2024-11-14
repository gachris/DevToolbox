using System;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using DevToolbox.Wpf.Controls;
using DevToolbox.Wpf.Serialization;

namespace DevToolbox.Wpf.Data;

/// <summary>
/// Group of one or more child groups
/// </summary>
internal class DockingGroup : ILayoutSerializable
{
    #region Fields/Consts

    public DockingGroup? _firstChild;
    public DockingGroup? _secondChild;

    #endregion

    #region Properties

    public Dock Dock { get; set; }

    public UIElement? AttachedElement { get; set; }

    public DockingGroup? FirstChild
    {
        get => _firstChild;
        set
        {
            if (value is not null) value.ParentGroup = this;
            _firstChild = value;
        }
    }

    public DockingGroup? SecondChild
    {
        get => _secondChild;
        set
        {
            if (value is not null) value.ParentGroup = this;
            _secondChild = value;
        }
    }

    public DockingGroup? ParentGroup { get; internal set; }

    public bool IsHidden
    {
        get
        {
            return AttachedElement is DockableControl dockable
                ? dockable.IsHidden
                : (FirstChild is not null || SecondChild is not null) && (FirstChild?.IsHidden ?? true) && (SecondChild?.IsHidden ?? false);
        }
    }

    public GridLength GroupWidth
    {
        get
        {
            if (AttachedElement is DockableControl dockable)
                return new(dockable.PaneWidth, GridUnitType.Pixel);
            else if (Dock is Dock.Left or Dock.Right)
            {
                var firstChildGridLength = FirstChild?.GroupWidth ?? GridLength.Auto;
                var secondChildGridLength = SecondChild?.GroupWidth ?? GridLength.Auto;
                return new(firstChildGridLength.Value + secondChildGridLength.Value + 4, GridUnitType.Pixel);
            }
            else
                return FirstChild?.GroupWidth ?? GridLength.Auto;
        }
    }

    public GridLength GroupHeight
    {
        get
        {
            if (AttachedElement is DockableControl dockable)
                return new(dockable.PaneHeight, GridUnitType.Pixel);
            else if (Dock is Dock.Top or Dock.Bottom)
            {
                var firstChildGridLength = FirstChild?.GroupHeight ?? GridLength.Auto;
                var secondChildGridLength = SecondChild?.GroupHeight ?? GridLength.Auto;
                return new(firstChildGridLength.Value + secondChildGridLength.Value + 4, GridUnitType.Pixel);
            }
            else
                return FirstChild?.GroupHeight ?? GridLength.Auto;
        }
    }

    #endregion

    /// <summary>
    /// Needed only for deserialization
    /// </summary>
    public DockingGroup()
    {
    }

    /// <summary>
    /// Create a group with a single control
    /// </summary>
    /// <param name="element">Attached control</param>
    public DockingGroup(UIElement element) => AttachedElement = element;

    /// <summary>
    /// Create a group with no controls
    /// </summary>
    public DockingGroup(DockingGroup firstGroup, DockingGroup secondGroup, Dock groupDock)
    {
        FirstChild = firstGroup;
        SecondChild = secondGroup;
        Dock = groupDock;
    }

    #region Methods

    public UIElement? GetParentFromContent(UIElement content)
    {
        if (AttachedElement is ItemsControl itemsControl && itemsControl.Items.Contains(content)) return AttachedElement;

        var control = FirstChild?.GetParentFromContent(content);

        return control is not null ? control : SecondChild?.GetParentFromContent(content);
    }

    public DockingGroup? GetDockableGroup(UIElement control)
    {
        if (AttachedElement == control) return this;

        var dockableGroup = FirstChild?.GetDockableGroup(control);

        return dockableGroup is not null ? dockableGroup : SecondChild?.GetDockableGroup(control);
    }

    public void ReplaceChildGroup(DockingGroup groupToFind, DockingGroup groupToReplace)
    {
        if (FirstChild == groupToFind) FirstChild = groupToReplace;
        else if (SecondChild == groupToFind) SecondChild = groupToReplace;
    }

    public DockingGroup Add(UIElement control, Dock dock) => dock switch
    {
        Dock.Right or Dock.Bottom => new(this, new(control), dock),
        Dock.Left or Dock.Top => new(new(control), this, dock),
        _ => throw new ArgumentOutOfRangeException(nameof(dock)),
    };

    public DockingGroup? Remove(UIElement control)
    {
        if (AttachedElement is not null) return null;

        if (FirstChild?.AttachedElement == control) return SecondChild;
        else if (SecondChild?.AttachedElement == control) return FirstChild;
        else
        {
            var group = FirstChild?.Remove(control);

            if (group is not null)
            {
                FirstChild = group;
                group.ParentGroup = this;
                return null;
            }

            group = SecondChild?.Remove(control);

            if (group is not null)
            {
                SecondChild = group;
                group.ParentGroup = this;
                return null;
            }
        }

        return null;
    }

    #endregion

    #region Save Size

    public void SaveChildSize()
    {
        if (ParentGroup != null && AttachedElement is DockableControl dockable) dockable?.SaveSize();
        else
        {
            FirstChild?.SaveChildSize();
            SecondChild?.SaveChildSize();
        }
    }

    #endregion

    #region ILayoutSerializable

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

    public void Deserialize(DockManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
    {
        //Dock = (Dock)Enum.Parse(typeof(Dock), node.Attributes[nameof(Dock)].Value);

        //if (node.ChildNodes[0].Name == nameof(DockableControl))
        //{
        //    DockableControl control = new();
        //    //control.DockManager = managerToAttach;
        //    control.Deserialize(managerToAttach, node.ChildNodes[0], getObjectHandler);
        //    //_attachedControl = control;
        //}
        //else if (node.ChildNodes[0].Name == nameof(DocumentControl))
        //{
        //    //DocumentControl control = managerToAttach.DocumentControl;
        //    //control.Deserialize(managerToAttach, node.ChildNodes[0], getObjectHandler);
        //    //_attachedControl = control;
        //}
        //else
        //{
        //    FirstChild = new()
        //    {
        //        ParentGroup = this
        //    };
        //    FirstChild.Deserialize(managerToAttach, node.ChildNodes[0].ChildNodes[0], getObjectHandler);

        //    SecondChild = new()
        //    {
        //        ParentGroup = this
        //    };
        //    SecondChild.Deserialize(managerToAttach, node.ChildNodes[0].ChildNodes[1], getObjectHandler);
        //}
    }

    #endregion
}
