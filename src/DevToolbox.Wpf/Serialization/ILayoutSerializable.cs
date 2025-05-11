using System.Xml;
using DevToolbox.Wpf.Controls;

namespace DevToolbox.Wpf.Serialization;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public interface ILayoutSerializable
{
    void Serialize(XmlDocument doc, XmlNode parentNode);

    void Deserialize(DockManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler);
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member