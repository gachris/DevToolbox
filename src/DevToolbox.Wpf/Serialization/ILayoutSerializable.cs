using System.Xml;
using DevToolbox.Wpf.Controls;

namespace DevToolbox.Wpf.Serialization;

/// <summary>
/// Defines methods for serializing and deserializing the layout of controls
/// in a <see cref="LayoutManager"/> to and from XML.
/// </summary>
public interface ILayoutSerializable
{
    /// <summary>
    /// Serializes the current layout state of the control into the specified XML document
    /// under the given parent node.
    /// </summary>
    /// <param name="doc">The XML document to which layout data will be written.</param>
    /// <param name="parentNode">The XML node under which layout elements will be appended.</param>
    void Serialize(XmlDocument doc, XmlNode parentNode);

    /// <summary>
    /// Deserializes and applies previously saved layout information from the specified XML node,
    /// attaching the deserialized control to the provided <see cref="LayoutManager"/>.
    /// </summary>
    /// <param name="managerToAttach">The DockManager instance to which deserialized controls will be added.</param>
    /// <param name="node">The XML node containing serialized layout data.</param>
    /// <param name="getObjectHandler">
    /// A callback that maps XML element names to control instances.
    /// Receives the type-string and returns the corresponding content object.
    /// </param>
    void Deserialize(LayoutManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler);
}