using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using DevToolbox.Wpf.Extensions;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// DiagramCanvas is a custom design canvas for hosting diagram layers.
/// It allows interaction, serialization, and deserialization of designer items and connections.
/// </summary>
public class DiagramCanvas : DesignCanvas
{
    #region Properties

    /// <summary>
    /// Provides access to the editable collection of items in the canvas.
    /// </summary>
    private IEditableCollectionView EditableItems => Items;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagramCanvas"/> class.
    /// This constructor is currently empty, but can be used for future setup or initialization logic.
    /// </summary>
    public DiagramCanvas()
    {
    }

    #region Methods Override

    /// <summary>
    /// Returns a new container for diagram items (DiagramLayer).
    /// </summary>
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new DiagramLayer();
    }

    /// <summary>
    /// Determines if the given item is its own container.
    /// </summary>
    /// <param name="item">Item to check.</param>
    /// <returns>True if the item is a DiagramLayer, otherwise false.</returns>
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is DiagramLayer;
    }

    /// <summary>
    /// Handles saving the current state of the diagram canvas to an XML file.
    /// </summary>
    protected override void OnSaveExecuted(ExecutedRoutedEventArgs e)
    {
        var designerItems = SelectedItems.Select(item => this.ContainerFromItem(item) as DiagramLayer).Where(x => !(x?.IsConnection ?? false));
        var connections = SelectedItems.Select(item => this.ContainerFromItem(item) as DiagramLayer).Where(x => x?.IsConnection ?? false);

        var designerItemsXML = SerializeDesignerItems(designerItems.OfType<DiagramLayer>());
        var connectionsXML = SerializeConnections(connections.OfType<DiagramLayer>());

        var root = new XElement("Root");
        root.Add(designerItemsXML);
        root.Add(connectionsXML);

        SaveFile(root);
    }

    /// <summary>
    /// Handles loading previously saved data from an XML file into the diagram canvas.
    /// </summary>
    protected override void OnOpenExecuted(ExecutedRoutedEventArgs e)
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
            if (item == null || this.ContainerFromItem(item) is not DiagramLayer container) continue;
            container.Deserialize(itemXML, id, 0, 0);
        }

        InvalidateVisual();

        var connectionsXML = root.Elements("Connections").Elements("Connection");
        foreach (var connectionXML in connectionsXML)
        {
            _ = Guid.TryParse(connectionXML.Element("SourceID")?.Value, out var sourceID);
            _ = Guid.TryParse(connectionXML.Element("SinkID")?.Value, out var sinkID);

            var sourceConnectorName = connectionXML.Element("SourceConnectorName")!.Value;
            var sinkConnectorName = connectionXML.Element("SinkConnectorName")!.Value;

            var sourceConnector = GetConnector(sourceID, sourceConnectorName);
            if (sourceConnector is null) continue;
            var sinkConnector = GetConnector(sinkID, sinkConnectorName);
            if (sinkConnector is null) continue;

            var connectionItem = AddNewItem();
            if (connectionItem == null || this.ContainerFromItem(connectionItem) is not DiagramLayer connection) continue;
            connection.Attach(sourceConnector, sinkConnector);

            _ = int.TryParse(connectionXML.Element("zIndex")?.Value, out var zIndex);
            Panel.SetZIndex(connection, zIndex);
        }
    }

    /// <summary>
    /// Handles copying the selected items and connections to the clipboard in XML format.
    /// </summary>
    protected override void OnCopyExecuted(ExecutedRoutedEventArgs e)
    {
        var selectedDesignerItems = SelectedItems.Select(item => this.ContainerFromItem(item) as DiagramLayer).Where(x => !(x?.IsConnection ?? false));
        var selectedConnections = SelectedItems.Select(item => this.ContainerFromItem(item) as DiagramLayer).Where(x => x?.IsConnection ?? false).ToList();

        var connections = Items.Select(item => this.ContainerFromItem(item) as DiagramLayer).OfType<DiagramLayer>().Where(x => x.IsConnection);

        foreach (var connection in connections.OfType<DiagramLayer>())
        {
            if (!selectedConnections.Contains(connection))
            {
                var sourceItem = (from item in selectedDesignerItems
                                  where item.ID == connection.Source?.ParentDesignerItem?.ID
                                  select item).FirstOrDefault();

                var sinkItem = (from item in selectedDesignerItems
                                where item.ID == connection.Sink?.ParentDesignerItem?.ID
                                select item).FirstOrDefault();

                if (sourceItem != null &&
                    sinkItem != null &&
                    BelongToSameGroup(sourceItem, sinkItem))
                {
                    selectedConnections.Add(connection);
                }
            }
        }

        var designerItemsXML = SerializeDesignerItems(selectedDesignerItems.OfType<DiagramLayer>());
        var connectionsXML = SerializeConnections(selectedConnections.OfType<DiagramLayer>());

        var root = new XElement("Root");
        root.Add(designerItemsXML);
        root.Add(connectionsXML);

        root.Add(new XAttribute("OffsetX", 10));
        root.Add(new XAttribute("OffsetY", 10));

        Clipboard.Clear();
        Clipboard.SetData(DataFormats.Xaml, root);
    }

    /// <summary>
    /// Handles pasting items from the clipboard into the diagram canvas.
    /// </summary>
    protected override void OnPasteExecuted(ExecutedRoutedEventArgs e)
    {
        var root = LoadSerializedDataFromClipBoard();

        if (root == null) return;

        var mappingOldToNewIDs = new Dictionary<Guid, Guid>();
        var newItems = new List<DiagramLayer>();
        var itemsXML = root.Elements("DesignerItems").Elements("DesignerItem");

        _ = double.TryParse(root.Attribute("OffsetX")?.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var offsetX);
        _ = double.TryParse(root.Attribute("OffsetY")?.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var offsetY);

        foreach (var itemXML in itemsXML)
        {
            _ = Guid.TryParse(itemXML.Element("ID")?.Value, out var oldID);
            var newID = Guid.NewGuid();
            mappingOldToNewIDs.Add(oldID, newID);
            var item = AddNewItem();
            if (item == null || this.ContainerFromItem(item) is not DiagramLayer container)
            {
                continue;
            }
            container.Deserialize(itemXML, newID, offsetX, offsetY);
            newItems.Add(container);
        }

        UnselectAll();

        foreach (var item in newItems)
        {
            if (item.ParentID != Guid.Empty)
            {
                item.ParentID = mappingOldToNewIDs[item.ParentID];
            }
        }

        foreach (var item in newItems)
        {
            if (item.ParentID == Guid.Empty)
            {
                Select(item);
            }
        }

        var connectionsXML = root.Elements("Connections").Elements("Connection");
        foreach (var connectionXML in connectionsXML)
        {
            _ = Guid.TryParse(connectionXML.Element("SourceID")?.Value, out var oldSourceID);
            _ = Guid.TryParse(connectionXML.Element("SinkID")?.Value, out var oldSinkID);

            if (mappingOldToNewIDs.ContainsKey(oldSourceID) && mappingOldToNewIDs.ContainsKey(oldSinkID))
            {
                var newSourceID = mappingOldToNewIDs[oldSourceID];
                var newSinkID = mappingOldToNewIDs[oldSinkID];

                var sourceConnectorName = connectionXML.Element("SourceConnectorName")!.Value;
                var sinkConnectorName = connectionXML.Element("SinkConnectorName")!.Value;

                var sourceConnector = GetConnector(newSourceID, sourceConnectorName);
                if (sourceConnector is null) continue;
                var sinkConnector = GetConnector(newSinkID, sinkConnectorName);
                if (sinkConnector is null) continue;

                var connectionItem = AddNewItem();
                if (connectionItem == null || this.ContainerFromItem(connectionItem) is not DiagramLayer connection) continue;
                connection.Attach(sourceConnector, sinkConnector);

                _ = int.TryParse(connectionXML.Element("zIndex")?.Value, out var zIndex);
                Panel.SetZIndex(connection, zIndex);

                Select(connection);
            }
        }

        BringToFrontCommand.Execute(null, this);

        if (root.Attribute("OffsetX") is XAttribute offsetXAttribute)
            offsetXAttribute.Value = (offsetX + 10).ToString();

        if (root.Attribute("OffsetY") is XAttribute offsetYAttribute)
            offsetYAttribute.Value = (offsetY + 10).ToString();

        Clipboard.Clear();
        Clipboard.SetData(DataFormats.Xaml, root);
    }

    /// <summary>
    /// Handles deleting selected items and their associated connections from the diagram canvas.
    /// </summary>
    protected override void OnDeleteExecuted(ExecutedRoutedEventArgs e)
    {
        var isReadOnly = ((IList)Items).IsReadOnly;
        var connections = SelectedItems.Select(item => this.ContainerFromItem(item) as DiagramLayer).OfType<DiagramLayer>().Where(x => x.IsConnection).ToList();

        foreach (var connection in connections)
        {
            if (!isReadOnly)
            {
                Items.Remove(connection);
            }
            else
            {
                EditableItems.Remove(this.ItemFromContainer(connection));
            }
        }

        while (SelectedItems.Count > 0)
        {
            var item = SelectedItems[0];

            if (item == null || this.ContainerFromItem(item) is not DiagramLayer element)
            {
                continue;
            }

            if (element.Template.FindName("PART_ConnectorDecorator", element) is not Control connectorDecoratorControl)
            {
                continue;
            }

            var connectors = new List<Connector>();
            GetConnectors(connectorDecoratorControl, connectors);

            if (!isReadOnly)
            {
                connectors.ForEach(connector => connector.Connections.ForEach(connection => Items.Remove(connection)));
                Items.Remove(item);
            }
            else
            {
                connectors.ForEach(connector => connector.Connections.ForEach(connection => EditableItems.Remove(this.ItemFromContainer(connection))));
                EditableItems.Remove(item);
            }
        }

        UnselectAll();
        UpdateZIndex();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Retrieves a connector by its associated item ID and name.
    /// </summary>
    private Connector? GetConnector(Guid itemID, string connectorName)
    {
        var elements = Items.Select(item => this.ContainerFromItem(item) as DiagramLayer);

        var designerItem = (from item in elements
                            where item.ID == itemID
                            select item).FirstOrDefault();

        var connectorDecorator = designerItem?.Template.FindName("PART_ConnectorDecorator", designerItem) as Control;
        connectorDecorator?.ApplyTemplate();

        return connectorDecorator?.Template.FindName(connectorName, connectorDecorator) as Connector;
    }

    /// <summary>
    /// Recursively gathers connectors from a parent visual element.
    /// </summary>
    private static void GetConnectors(DependencyObject parent, List<Connector> connectors)
    {
        var childrenCount = VisualTreeHelper.GetChildrenCount(parent);

        for (var i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is Connector connector)
            {
                connectors.Add(connector);
            }
            else
            {
                GetConnectors(child, connectors);
            }
        }
    }

    /// <summary>
    /// Serializes the designer items into an XML element.
    /// </summary>
    private static XElement SerializeDesignerItems(IEnumerable<DiagramLayer> designerItems)
    {
        return new XElement("DesignerItems", from item in designerItems select item.Serialize());
    }

    /// <summary>
    /// Serializes the connections into an XML element.
    /// </summary>
    private static XElement SerializeConnections(IEnumerable<DiagramLayer> connections)
    {
        return new XElement("ConnectionItems", from item in connections select item.Serialize());
    }

    #endregion
}