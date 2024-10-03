namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Defines the available styles for the <see cref="ProgressRing"/> control.
/// Each style represents a different animation pattern that can be used to indicate progress.
/// </summary>
public enum ProgressRingStyle
{
    /// <summary>
    /// A rotating plane style where a plane rotates around a center point.
    /// </summary>
    RotatingPlane = 0,

    /// <summary>
    /// A double bounce style where two balls bounce alternately.
    /// </summary>
    DoubleBounce = 1,

    /// <summary>
    /// A wave style where a wave of circles moves across the control.
    /// </summary>
    Wave = 2,

    /// <summary>
    /// A wandering cubes style where cubes move around in a random fashion.
    /// </summary>
    WanderingCubes = 3,

    /// <summary>
    /// A pulse style that creates a pulsing effect.
    /// </summary>
    Pulse = 4,

    /// <summary>
    /// A chasing dots style where dots chase each other in a circular pattern.
    /// </summary>
    ChasingDots = 5,

    /// <summary>
    /// A three bounce style where three dots bounce in succession.
    /// </summary>
    ThreeBounce = 6,

    /// <summary>
    /// A simple circular style that indicates progress in a circular motion.
    /// </summary>
    Circle = 7
}
