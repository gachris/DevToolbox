using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml;
using DevToolbox.Wpf.Data;
using DevToolbox.Wpf.Serialization;

namespace DevToolbox.Wpf.Controls;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class DockingGroupPanel : Grid, ILayoutSerializable
{
    #region Fields/Consts

    private DockManager? _owner;
    private DockingGroup? _dockableGroup;

    #endregion

    #region Methods Override

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        AttachToOwner();
    }

    #endregion

    #region Methods

    internal void AttachDocumentList(UIElement listControl)
    {
        _dockableGroup = new DockingGroup(listControl);

        if (_owner is not null && _owner.Items.Count > 0)
        {
            _owner.Items.Refresh();
        }
        else
        {
            ArrangeLayout();
        }
    }

    private void ArrangeLayout()
    {
        if (_dockableGroup == null) throw new Exception();

        _dockableGroup.SaveChildSize();

        Children.Clear();
        ColumnDefinitions.Clear();
        RowDefinitions.Clear();

        DockingGroupPanel.Arrange(this, _dockableGroup);

        InvalidateArrange();
    }

    private static void Arrange(Grid grid, DockingGroup dockableGroup)
    {
        if (dockableGroup.AttachedElement is not null)
        {
            grid.Children.Add(new ContentControl { Content = dockableGroup.AttachedElement });
        }
        else if (dockableGroup.FirstChild is not null && dockableGroup.FirstChild.IsHidden && dockableGroup.SecondChild is not null && !dockableGroup.SecondChild.IsHidden)
        {
            DockingGroupPanel.Arrange(grid, dockableGroup.SecondChild);
        }
        else if (dockableGroup.FirstChild is not null && !dockableGroup.FirstChild.IsHidden && dockableGroup.SecondChild is not null && dockableGroup.SecondChild.IsHidden)
        {
            DockingGroupPanel.Arrange(grid, dockableGroup.FirstChild);
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
                DockingGroupPanel.Arrange(firstChildGrid, dockableGroup.FirstChild);
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
                DockingGroupPanel.Arrange(secondChildGrid, dockableGroup.SecondChild);
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
                DockingGroupPanel.Arrange(firstChildGrid, dockableGroup.FirstChild);
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
                DockingGroupPanel.Arrange(secondChildGrid, dockableGroup.SecondChild);
                grid.Children.Add(secondChildGrid);
            }
        }
    }

    internal void Add(UIElement element, Dock dock)
    {
        _dockableGroup = _dockableGroup?.Add(element, dock);

        ArrangeLayout();
    }

    internal void Add(UIElement element, UIElement relativeElement, Dock relativeDock)
    {
        if (_dockableGroup?.GetDockableGroup(relativeElement) is not DockingGroup dockableGroup) return;

        switch (relativeDock)
        {
            case Dock.Right:
            case Dock.Bottom:
                {
                    if (dockableGroup == _dockableGroup)
                        _dockableGroup = new DockingGroup(dockableGroup, new(element), relativeDock);
                    else
                    {
                        var parentGroup = dockableGroup.ParentGroup;
                        var newChildGroup = new DockingGroup(dockableGroup, new(element), relativeDock);
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
                        var newChildGroup = new DockingGroup(new(element), dockableGroup, relativeDock);
                        parentGroup?.ReplaceChildGroup(dockableGroup, newChildGroup);
                    }
                }
                break;
        }

        ArrangeLayout();
    }

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

    private void AttachToOwner()
    {
        if (_owner != null) _owner.ItemContainerGenerator.ItemsChanged -= ItemContainerGenerator_ItemsChanged;

        _owner = TemplatedParent as DockManager;

        if (_owner != null)
        {
            _owner.ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
            if (_owner.Items.Count > 0 && _dockableGroup != null) _owner.Items.Refresh();
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

    private static DocumentControl? FindDocumentsPane(DockingGroup? group)
    {
        if (group == null)
            return null;

        if (group.AttachedElement is DocumentControl)
            return group.AttachedElement as DocumentControl;
        else
        {
            var docsPane = DockingGroupPanel.FindDocumentsPane(group.FirstChild);
            if (docsPane != null)
                return docsPane;

            docsPane = DockingGroupPanel.FindDocumentsPane(group.SecondChild);
            if (docsPane != null)
                return docsPane;
        }

        return null;
    }

    #endregion

    #region ILayoutSerializable

    public void Serialize(XmlDocument doc, XmlNode parentNode)
    {
        var node_rootGroup = doc.CreateElement("_dockablePaneGroup");

        _dockableGroup?.Serialize(doc, parentNode);

        parentNode.AppendChild(node_rootGroup);
    }

    public void Deserialize(DockManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
    {
        _dockableGroup = new DockingGroup();
        _dockableGroup.Deserialize(managerToAttach, node, getObjectHandler);

        //_docsPane = FindDocumentsPane(_rootGroup);

        InvalidateArrange();
    }

    #endregion
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
