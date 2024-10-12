using System.Windows;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents information about a connector, including its position, size, and orientation 
/// relative to a designer item in a diagram.
/// </summary>
internal struct ConnectorInfo
{
    /// <summary>
    /// Gets or sets the left position of the designer item containing the connector.
    /// </summary>
    public double Left { get; set; }

    /// <summary>
    /// Gets or sets the top position of the designer item containing the connector.
    /// </summary>
    public double Top { get; set; }

    /// <summary>
    /// Gets or sets the size of the designer item containing the connector.
    /// </summary>
    public Size Size { get; set; }

    /// <summary>
    /// Gets or sets the position of the connector within the designer item.
    /// </summary>
    public Point Position { get; set; }

    /// <summary>
    /// Gets or sets the orientation of the connector, indicating which side of the 
    /// designer item the connector is positioned (e.g., Top, Bottom, Left, Right).
    /// </summary>
    public ConnectorOrientation Orientation { get; set; }
}