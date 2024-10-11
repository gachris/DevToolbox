using System;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Provides data for the capture state change event in the <see cref="EyeDropper"/> control.
/// </summary>
public class CaptureEventArgs : EventArgs
{
    /// <summary>
    /// Gets the state of the capture (e.g., Started, Continued, or Finished).
    /// </summary>
    public CaptureState CaptureState { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CaptureEventArgs"/> class.
    /// </summary>
    /// <param name="captureState">The current capture state.</param>
    public CaptureEventArgs(CaptureState captureState)
    {
        CaptureState = captureState;
    }
}