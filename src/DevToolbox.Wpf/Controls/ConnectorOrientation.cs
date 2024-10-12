namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Specifies the possible orientations of a connector in relation to its parent item.
/// </summary>
public enum ConnectorOrientation
{
    /// <summary>
    /// No specific orientation is set for the connector.
    /// </summary>
    None,

    /// <summary>
    /// The connector is located on the left side of the parent item.
    /// </summary>
    Left,

    /// <summary>
    /// The connector is located on the top side of the parent item.
    /// </summary>
    Top,

    /// <summary>
    /// The connector is located on the right side of the parent item.
    /// </summary>
    Right,

    /// <summary>
    /// The connector is located on the bottom side of the parent item.
    /// </summary>
    Bottom
}