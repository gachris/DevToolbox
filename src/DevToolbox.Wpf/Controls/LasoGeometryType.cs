namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Defines the types of geometric shapes that can be used in a lasso selection tool.
/// </summary>
public enum LasoGeometryType
{
    /// <summary>
    /// A rectangular selection shape.
    /// </summary>
    Rectangle = 0,

    /// <summary>
    /// A bounded ellipse selection shape.
    /// </summary>
    BoundedEclipse = 1,

    /// <summary>
    /// A centered circular selection shape.
    /// </summary>
    CenteredCircle = 2,

    /// <summary>
    /// Two perpendicular lines for selection.
    /// </summary>
    PerpendicularLines = 3,

    /// <summary>
    /// A polyline shape, which consists of multiple connected line segments.
    /// </summary>
    Polyline = 4,

    /// <summary>
    /// A custom selection shape.
    /// </summary>
    Custom = 5
}