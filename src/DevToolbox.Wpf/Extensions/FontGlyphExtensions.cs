using System.Linq;
using System.Windows;
using System.Windows.Media;
using DevToolbox.Wpf.Controls;

namespace DevToolbox.Wpf.Extensions;

/// <summary>
/// Control extensions
/// </summary>
internal static class FontGlyphExtensions
{
    /// <summary>
    /// Sets the rotation for the control
    /// </summary>
    /// <param name="control">Control to apply the rotation</param>
    public static void SetRotation(this FontGlyph control)
    {
        var transformGroup = control.RenderTransform as TransformGroup ?? new TransformGroup();
        var rotateTransform = transformGroup.Children.OfType<RotateTransform>().FirstOrDefault();

        if (rotateTransform != null)
        {
            rotateTransform.Angle = control.Rotation;
            return;
        }

        transformGroup.Children.Add(new RotateTransform(control.Rotation));
        control.RenderTransform = transformGroup;
        control.RenderTransformOrigin = new Point(0.5, 0.5);
    }

    /// <summary>
    /// Sets the flip orientation for the control
    /// </summary>
    /// <param name="control">Control to apply the orientation</param>
    public static void SetFlipOrientation(this FontGlyph control)
    {
        var transformGroup = control.RenderTransform as TransformGroup ?? new TransformGroup();
        var scaleX = control.FlipOrientation is FlipOrientation.Normal or FlipOrientation.Vertical ? 1 : -1;
        var scaleY = control.FlipOrientation is FlipOrientation.Normal or FlipOrientation.Horizontal ? 1 : -1;
        var scaleTransform = transformGroup.Children.OfType<ScaleTransform>().FirstOrDefault();

        if (scaleTransform != null)
        {
            scaleTransform.ScaleX = scaleX;
            scaleTransform.ScaleY = scaleY;
        }
        else
        {
            transformGroup.Children.Add(new ScaleTransform(scaleX, scaleY));
            control.RenderTransform = transformGroup;
            control.RenderTransformOrigin = new Point(0.5, 0.5);
        }
    }
}
