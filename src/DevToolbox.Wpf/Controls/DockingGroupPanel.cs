using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml;
using DevToolbox.Wpf.Data;
using DevToolbox.Wpf.Serialization;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A panel that manages a group of docking elements within a DockManager, allowing nested splitting and resizing.
/// Implements <see cref="ILayoutSerializable"/> for XML-based layout persistence.
/// </summary>
public class DockingGroupPanel : Grid, ILayoutSerializable
{
    #region Fields/Consts

    private DockManager? _owner;
    private DockingGroup? _dockableGroup;

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

    /// <summary>
    /// Clears children and definitions then arranges the layout based on the current docking group.
    /// </summary>
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

    /// <summary>
    /// Recursively arranges the content of the grid based on the specified docking group structure.
    /// </summary>
    /// <param name="grid">Target grid for arrangement.</param>
    /// <param name="dockableGroup">The docking group describing layout.</param>
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

        _owner = TemplatedParent as DockManager;

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

    /// <summary>
    /// Searches recursively for the first <see cref="DocumentControl"/> in the docking group.
    /// </summary>
    /// <param name="group">The docking group to search.</param>
    /// <returns>The found <see cref="DocumentControl"/> or null.</returns>
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

    /// <inheritdoc/>
    public void Serialize(XmlDocument doc, XmlNode parentNode)
    {
        var rootGroupNode = doc.CreateElement("_dockablePaneGroup");
        _dockableGroup?.Serialize(doc, parentNode);
        parentNode.AppendChild(rootGroupNode);
    }

    /// <inheritdoc/>
    public void Deserialize(DockManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
    {
        _dockableGroup = new DockingGroup();
        _dockableGroup.Deserialize(managerToAttach, node, getObjectHandler);
        InvalidateArrange();
    }

    #endregion
}
