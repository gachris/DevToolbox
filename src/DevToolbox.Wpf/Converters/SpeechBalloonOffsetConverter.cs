using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using DevToolbox.Wpf.Controls;

namespace DevToolbox.Wpf.Converters;

/// <summary>
/// A converter that calculates the offset for a speech balloon 
/// based on various parameters such as placement mode, target dimensions, and offsets.
/// Implements IMultiValueConverter for use in data binding with multiple values.
/// </summary>
public class SpeechBalloonOffsetConverter : IMultiValueConverter
{
    /// <summary>
    /// Converts multiple values into a Thickness value representing 
    /// the offsets for positioning a speech balloon.
    /// </summary>
    /// <param name="values">An array of values containing parameters needed for calculation.</param>
    /// <param name="targetType">The target type of the conversion (not used).</param>
    /// <param name="parameter">Additional parameter for the conversion (not used).</param>
    /// <param name="culture">Culture information for conversion (not used).</param>
    /// <returns>A Thickness value that specifies the offset for the speech balloon.</returns>

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        const double offsetPadding = 10;
        const double speechBalloonSize = 22;

        var placement = (PlacementMode)values[0];
        var placementTargetActualWidth = values[1] != DependencyProperty.UnsetValue ? (double)values[1] : 0;
        var placementTargetActualHeight = values[2] != DependencyProperty.UnsetValue ? (double)values[2] : 0;
        var verticalOffset = (double)values[3];
        var horizontalOffset = (double)values[4];
        var dropDownWidth = (double)values[5];
        var dropDownHeight = (double)values[6];
        var speechBalloonPlacement = (SpeechBalloonPlacement)values[7];
        var speechBalloonVerticalOffset = (double)values[8];
        var speechBalloonHorizontalOffset = (double)values[9];

        if (speechBalloonVerticalOffset.Equals(double.NaN))
            speechBalloonVerticalOffset = 0;

        if (speechBalloonHorizontalOffset.Equals(double.NaN))
            speechBalloonHorizontalOffset = 0;

        switch (speechBalloonPlacement)
        {
            case SpeechBalloonPlacement.None:
                return new Thickness(0);
            case SpeechBalloonPlacement.Left:
            case SpeechBalloonPlacement.Right:
                {
                    speechBalloonVerticalOffset = -verticalOffset + (placementTargetActualHeight / 2) - (speechBalloonSize / 2) + speechBalloonVerticalOffset;

                    if (speechBalloonVerticalOffset < offsetPadding)
                        speechBalloonVerticalOffset = offsetPadding;

                    if (speechBalloonVerticalOffset > dropDownHeight)
                        speechBalloonVerticalOffset = dropDownHeight - speechBalloonSize - offsetPadding;

                    return new Thickness(0, speechBalloonVerticalOffset, 0, 0);
                }
            case SpeechBalloonPlacement.Top:
            case SpeechBalloonPlacement.Bottom:
                {
                    speechBalloonHorizontalOffset = -horizontalOffset + (placementTargetActualWidth / 2) - (speechBalloonSize / 2) + speechBalloonHorizontalOffset;

                    if (speechBalloonHorizontalOffset < offsetPadding)
                        speechBalloonHorizontalOffset = offsetPadding;

                    if (speechBalloonHorizontalOffset > dropDownWidth)
                        speechBalloonHorizontalOffset = dropDownWidth - speechBalloonSize - offsetPadding;

                    return new Thickness(speechBalloonHorizontalOffset, 0, 0, 0);
                }
            case SpeechBalloonPlacement.Auto:
                {
                    switch (placement)
                    {
                        case PlacementMode.Left:
                        case PlacementMode.Right:
                            speechBalloonVerticalOffset = -verticalOffset + (placementTargetActualHeight / 2) - (speechBalloonSize / 2) + speechBalloonVerticalOffset;

                            if (speechBalloonVerticalOffset < offsetPadding)
                                speechBalloonVerticalOffset = offsetPadding;

                            if (speechBalloonVerticalOffset > dropDownHeight)
                                speechBalloonVerticalOffset = dropDownHeight - speechBalloonSize - offsetPadding;

                            return new Thickness(0, speechBalloonVerticalOffset, 0, 0);
                        case PlacementMode.Center:
                            speechBalloonVerticalOffset = -verticalOffset + (dropDownHeight / 2) - (speechBalloonSize / 2) + speechBalloonVerticalOffset;

                            if (speechBalloonVerticalOffset < offsetPadding)
                                speechBalloonVerticalOffset = offsetPadding;

                            if (speechBalloonVerticalOffset > dropDownHeight)
                                speechBalloonVerticalOffset = dropDownHeight - speechBalloonSize - offsetPadding;

                            return new Thickness(0, speechBalloonVerticalOffset, 0, 0);
                        case PlacementMode.Absolute:
                        case PlacementMode.Relative:
                        case PlacementMode.AbsolutePoint:
                        case PlacementMode.RelativePoint:
                        case PlacementMode.Mouse:
                        case PlacementMode.MousePoint:
                        case PlacementMode.Top:
                        case PlacementMode.Bottom:
                        case PlacementMode.Custom:
                            speechBalloonHorizontalOffset = -horizontalOffset + (placementTargetActualWidth / 2) - (speechBalloonSize / 2) + speechBalloonHorizontalOffset;

                            if (speechBalloonHorizontalOffset < offsetPadding)
                                speechBalloonHorizontalOffset = offsetPadding;

                            if (speechBalloonHorizontalOffset > dropDownWidth)
                                speechBalloonHorizontalOffset = dropDownWidth - speechBalloonSize - offsetPadding;

                            return new Thickness(speechBalloonHorizontalOffset, 0, 0, 0);
                        default:
                            throw new ArgumentOutOfRangeException(nameof(SpeechBalloonPlacement));
                    }
                }
            default:
                throw new ArgumentOutOfRangeException(nameof(SpeechBalloonPlacement));
        }
    }

    /// <summary>
    /// ConvertBack method is not implemented as this converter does not support two-way binding.
    /// </summary>
    /// <param name="value">The value to convert back (not used).</param>
    /// <param name="targetTypes">The target types for conversion (not used).</param>
    /// <param name="parameter">Additional parameter for the conversion (not used).</param>
    /// <param name="culture">Culture information for conversion (not used).</param>
    /// <returns>Throws NotImplementedException.</returns>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
