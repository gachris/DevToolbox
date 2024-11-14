using System.Xml;
using DevToolbox.Wpf.Controls;

namespace DevToolbox.Wpf.Serialization;

public interface ILayoutSerializable
{
    void Serialize(XmlDocument doc, XmlNode parentNode);

    void Deserialize(DockManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler);
}