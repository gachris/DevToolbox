using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace DevToolbox.Wpf.Helpers;

/// <summary>
/// A helper class that provides attached properties to enhance UI elements 
/// by defining various visual states such as pressed, hovered, disabled, and selected.
/// </summary>
public class UIElementHelper
{
    #region Fields/Consts

    /// <summary>
    /// DependencyProperty for the background color when the element is pressed.
    /// </summary>
    public static readonly DependencyProperty PressedBackgroundProperty =
        DependencyProperty.RegisterAttached("PressedBackground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the foreground color when the element is pressed.
    /// </summary>
    public static readonly DependencyProperty PressedForegroundProperty =
        DependencyProperty.RegisterAttached("PressedForeground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the border brush when the element is pressed.
    /// </summary>
    public static readonly DependencyProperty PressedBorderBrushProperty =
        DependencyProperty.RegisterAttached("PressedBorderBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the background color when the mouse is over the element.
    /// </summary>
    public static readonly DependencyProperty MouseOverBackgroundProperty =
        DependencyProperty.RegisterAttached("MouseOverBackground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the foreground color when the mouse is over the element.
    /// </summary>
    public static readonly DependencyProperty MouseOverForegroundProperty =
        DependencyProperty.RegisterAttached("MouseOverForeground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the border brush when the mouse is over the element.
    /// </summary>
    public static readonly DependencyProperty MouseOverBorderBrushProperty =
        DependencyProperty.RegisterAttached("MouseOverBorderBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the background color when the element is disabled.
    /// </summary>
    public static readonly DependencyProperty DisabledBackgroundProperty =
        DependencyProperty.RegisterAttached("DisabledBackground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the foreground color when the element is disabled.
    /// </summary>
    public static readonly DependencyProperty DisabledForegroundProperty =
        DependencyProperty.RegisterAttached("DisabledForeground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the border brush when the element is disabled.
    /// </summary>
    public static readonly DependencyProperty DisabledBorderBrushProperty =
        DependencyProperty.RegisterAttached("DisabledBorderBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the corner radius of the element.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.RegisterAttached("CornerRadius", typeof(CornerRadius), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the background color of popups.
    /// </summary>
    public static readonly DependencyProperty PopupBackgroundProperty =
        DependencyProperty.RegisterAttached("PopupBackground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the foreground color of popups.
    /// </summary>
    public static readonly DependencyProperty PopupForegroundProperty =
        DependencyProperty.RegisterAttached("PopupForeground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the border brush of popups.
    /// </summary>
    public static readonly DependencyProperty PopupBorderBrushProperty =
        DependencyProperty.RegisterAttached("PopupBorderBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the background color when the element is focused.
    /// </summary>
    public static readonly DependencyProperty FocusedBackgroundProperty =
        DependencyProperty.RegisterAttached("FocusedBackground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the foreground color when the element is focused.
    /// </summary>
    public static readonly DependencyProperty FocusedForegroundProperty =
        DependencyProperty.RegisterAttached("FocusedForeground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the border brush when the element is focused.
    /// </summary>
    public static readonly DependencyProperty FocusedBorderBrushProperty =
        DependencyProperty.RegisterAttached("FocusedBorderBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the background color when the mouse hovers over the element.
    /// </summary>
    public static readonly DependencyProperty HoverBackgroundProperty =
        DependencyProperty.RegisterAttached("HoverBackground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the foreground color when the mouse hovers over the element.
    /// </summary>
    public static readonly DependencyProperty HoverForegroundProperty =
        DependencyProperty.RegisterAttached("HoverForeground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the border brush when the mouse hovers over the element.
    /// </summary>
    public static readonly DependencyProperty HoverBorderBrushProperty =
        DependencyProperty.RegisterAttached("HoverBorderBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the background color when the element is selected.
    /// </summary>
    public static readonly DependencyProperty SelectedBackgroundProperty =
        DependencyProperty.RegisterAttached("SelectedBackground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the foreground color when the element is selected.
    /// </summary>
    public static readonly DependencyProperty SelectedForegroundProperty =
        DependencyProperty.RegisterAttached("SelectedForeground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the border brush when the element is selected.
    /// </summary>
    public static readonly DependencyProperty SelectedBorderBrushProperty =
        DependencyProperty.RegisterAttached("SelectedBorderBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the background color when the selected element is hovered over.
    /// </summary>
    public static readonly DependencyProperty SelectedHoverBackgroundProperty =
        DependencyProperty.RegisterAttached("SelectedHoverBackground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the foreground color when the selected element is hovered over.
    /// </summary>
    public static readonly DependencyProperty SelectedHoverForegroundProperty =
        DependencyProperty.RegisterAttached("SelectedHoverForeground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the border brush when the selected element is hovered over.
    /// </summary>
    public static readonly DependencyProperty SelectedHoverBorderBrushProperty =
        DependencyProperty.RegisterAttached("SelectedHoverBorderBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the inactive background color when the selected element is hovered over.
    /// </summary>
    public static readonly DependencyProperty InactiveSelectionMouseOverBackgroundProperty =
        DependencyProperty.RegisterAttached("InactiveSelectionMouseOverBackground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the inactive foreground color when the selected element is hovered over.
    /// </summary>
    public static readonly DependencyProperty InactiveSelectionMouseOverForegroundProperty =
        DependencyProperty.RegisterAttached("InactiveSelectionMouseOverForeground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the inactive border brush when the selected element is hovered over.
    /// </summary>
    public static readonly DependencyProperty InactiveSelectionMouseOverBorderBrushProperty =
        DependencyProperty.RegisterAttached("InactiveSelectionMouseOverBorderBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the background color when the selected element is not focused.
    /// </summary>
    public static readonly DependencyProperty SelectedNoFocusedBackgroundProperty =
        DependencyProperty.RegisterAttached("SelectedNoFocusedBackground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the foreground color when the selected element is not focused.
    /// </summary>
    public static readonly DependencyProperty SelectedNoFocusedForegroundProperty =
        DependencyProperty.RegisterAttached("SelectedNoFocusedForeground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the border brush when the selected element is not focused.
    /// </summary>
    public static readonly DependencyProperty SelectedNoFocusedBorderBrushProperty =
        DependencyProperty.RegisterAttached("SelectedNoFocusedBorderBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the background color when the element is hovered over while focused.
    /// </summary>
    public static readonly DependencyProperty HoverFocusedBackgroundProperty =
        DependencyProperty.RegisterAttached("HoverFocusedBackground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the foreground color when the element is hovered over while focused.
    /// </summary>
    public static readonly DependencyProperty HoverFocusedForegroundProperty =
        DependencyProperty.RegisterAttached("HoverFocusedForeground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the border brush when the element is hovered over while focused.
    /// </summary>
    public static readonly DependencyProperty HoverFocusedBorderBrushProperty =
        DependencyProperty.RegisterAttached("HoverFocusedBorderBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the brush used for glyphs.
    /// </summary>
    public static readonly DependencyProperty GlyphBrushProperty =
        DependencyProperty.RegisterAttached("GlyphBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the brush used for glyphs when the mouse is over the element.
    /// </summary>
    public static readonly DependencyProperty MouseOverGlyphBrushProperty =
        DependencyProperty.RegisterAttached("MouseOverGlyphBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the brush used for glyphs when the element is pressed.
    /// </summary>
    public static readonly DependencyProperty PressedGlyphBrushProperty =
        DependencyProperty.RegisterAttached("PressedGlyphBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the brush used for glyphs when the element is disabled.
    /// </summary>
    public static readonly DependencyProperty DisabledGlyphBrushProperty =
        DependencyProperty.RegisterAttached("DisabledGlyphBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the foreground color of placeholder text.
    /// </summary>
    public static readonly DependencyProperty PlaceholderForegroundProperty =
        DependencyProperty.RegisterAttached("PlaceholderForeground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the brush used for selection.
    /// </summary>
    public static readonly DependencyProperty SelectionBrushProperty =
        DependencyProperty.RegisterAttached("SelectionBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the placeholder text of input elements.
    /// </summary>
    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.RegisterAttached("Placeholder", typeof(string), typeof(UIElementHelper), new PropertyMetadata(null));

    /// <summary>
    /// DependencyProperty for the highlight background color of elements.
    /// </summary>
    public static readonly DependencyProperty HighlightBackgroundProperty =
        DependencyProperty.RegisterAttached("HighlightBackground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the border brush used for highlights.
    /// </summary>
    public static readonly DependencyProperty HighlightBorderBrushProperty =
        DependencyProperty.RegisterAttached("HighlightBorderBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the background color of submenu items.
    /// </summary>
    public static readonly DependencyProperty SubMenuBackgroundProperty =
        DependencyProperty.RegisterAttached("SubMenuBackground", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the border brush of submenu items.
    /// </summary>
    public static readonly DependencyProperty SubMenuBorderBrushProperty =
        DependencyProperty.RegisterAttached("SubMenuBorderBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the default chrome brush.
    /// </summary>
    public static readonly DependencyProperty ChromeBrushProperty =
        DependencyProperty.RegisterAttached(
            "ChromeBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the chrome brush when the mouse is over the element.
    /// </summary>
    public static readonly DependencyProperty MouseOverChromeBrushProperty =
        DependencyProperty.RegisterAttached(
            "MouseOverChromeBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the chrome brush when the element is pressed.
    /// </summary>
    public static readonly DependencyProperty PressedChromeBrushProperty =
        DependencyProperty.RegisterAttached(
            "PressedChromeBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    /// <summary>
    /// DependencyProperty for the chrome brush when the element is disabled.
    /// </summary>
    public static readonly DependencyProperty DisabledChromeBrushProperty =
        DependencyProperty.RegisterAttached(
            "DisabledChromeBrush", typeof(Brush), typeof(UIElementHelper), new FrameworkPropertyMetadata());

    #endregion

    #region Methods

    /// <summary>
    /// Sets the pressed background brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the pressed background for.</param>
    /// <param name="value">The Brush to use as the pressed background.</param>
    public static void SetPressedBackground(UIElement element, Brush value)
    {
        element.SetValue(PressedBackgroundProperty, value);
    }

    /// <summary>
    /// Gets the pressed background brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the pressed background from.</param>
    /// <returns>The Brush used as the pressed background.</returns>
    public static Brush GetPressedBackground(UIElement element)
    {
        return (Brush)element.GetValue(PressedBackgroundProperty);
    }

    /// <summary>
    /// Sets the pressed foreground brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the pressed foreground for.</param>
    /// <param name="value">The Brush to use as the pressed foreground.</param>
    public static void SetPressedForeground(UIElement element, Brush value)
    {
        element.SetValue(PressedForegroundProperty, value);
    }

    /// <summary>
    /// Gets the pressed foreground brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the pressed foreground from.</param>
    /// <returns>The Brush used as the pressed foreground.</returns>
    public static Brush GetPressedForeground(UIElement element)
    {
        return (Brush)element.GetValue(PressedForegroundProperty);
    }

    /// <summary>
    /// Sets the pressed border brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the pressed border brush for.</param>
    /// <param name="value">The Brush to use as the pressed border brush.</param>
    public static void SetPressedBorderBrush(UIElement element, Brush value)
    {
        element.SetValue(PressedBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets the pressed border brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the pressed border brush from.</param>
    /// <returns>The Brush used as the pressed border brush.</returns>
    public static Brush GetPressedBorderBrush(UIElement element)
    {
        return (Brush)element.GetValue(PressedBorderBrushProperty);
    }

    /// <summary>
    /// Sets the mouse-over background brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the mouse-over background for.</param>
    /// <param name="value">The Brush to use as the mouse-over background.</param>
    public static void SetMouseOverBackground(UIElement element, Brush value)
    {
        element.SetValue(MouseOverBackgroundProperty, value);
    }

    /// <summary>
    /// Gets the mouse-over background brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the mouse-over background from.</param>
    /// <returns>The Brush used as the mouse-over background.</returns>
    public static Brush GetMouseOverBackground(UIElement element)
    {
        return (Brush)element.GetValue(MouseOverBackgroundProperty);
    }

    /// <summary>
    /// Sets the mouse-over foreground brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the mouse-over foreground for.</param>
    /// <param name="value">The Brush to use as the mouse-over foreground.</param>
    public static void SetMouseOverForeground(UIElement element, Brush value)
    {
        element.SetValue(MouseOverForegroundProperty, value);
    }

    /// <summary>
    /// Gets the mouse-over foreground brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the mouse-over foreground from.</param>
    /// <returns>The Brush used as the mouse-over foreground.</returns>
    public static Brush GetMouseOverForeground(UIElement element)
    {
        return (Brush)element.GetValue(MouseOverForegroundProperty);
    }

    /// <summary>
    /// Sets the mouse-over border brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the mouse-over border brush for.</param>
    /// <param name="value">The Brush to use as the mouse-over border brush.</param>
    public static void SetMouseOverBorderBrush(UIElement element, Brush value)
    {
        element.SetValue(MouseOverBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets the mouse-over border brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the mouse-over border brush from.</param>
    /// <returns>The Brush used as the mouse-over border brush.</returns>
    public static Brush GetMouseOverBorderBrush(UIElement element)
    {
        return (Brush)element.GetValue(MouseOverBorderBrushProperty);
    }

    /// <summary>
    /// Sets the disabled background brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the disabled background for.</param>
    /// <param name="value">The Brush to use as the disabled background.</param>
    public static void SetDisabledBackground(UIElement element, Brush value)
    {
        element.SetValue(DisabledBackgroundProperty, value);
    }

    /// <summary>
    /// Gets the disabled background brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the disabled background from.</param>
    /// <returns>The Brush used as the disabled background.</returns>
    public static Brush GetDisabledBackground(UIElement element)
    {
        return (Brush)element.GetValue(DisabledBackgroundProperty);
    }

    /// <summary>
    /// Sets the disabled foreground brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the disabled foreground for.</param>
    /// <param name="value">The Brush to use as the disabled foreground.</param>
    public static void SetDisabledForeground(UIElement element, Brush value)
    {
        element.SetValue(DisabledForegroundProperty, value);
    }

    /// <summary>
    /// Gets the disabled foreground brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the disabled foreground from.</param>
    /// <returns>The Brush used as the disabled foreground.</returns>
    public static Brush GetDisabledForeground(UIElement element)
    {
        return (Brush)element.GetValue(DisabledForegroundProperty);
    }

    /// <summary>
    /// Sets the disabled border brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the disabled border brush for.</param>
    /// <param name="value">The Brush to use as the disabled border brush.</param>
    public static void SetDisabledBorderBrush(UIElement element, Brush value)
    {
        element.SetValue(DisabledBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets the disabled border brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the disabled border brush from.</param>
    /// <returns>The Brush used as the disabled border brush.</returns>
    public static Brush GetDisabledBorderBrush(UIElement element)
    {
        return (Brush)element.GetValue(DisabledBorderBrushProperty);
    }

    /// <summary>
    /// Sets the corner radius for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the corner radius for.</param>
    /// <param name="value">The CornerRadius to use.</param>
    public static void SetCornerRadius(UIElement element, CornerRadius value)
    {
        element.SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets the corner radius for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the corner radius from.</param>
    /// <returns>The CornerRadius used for the UIElement.</returns>
    public static CornerRadius GetCornerRadius(UIElement element)
    {
        return (CornerRadius)element.GetValue(CornerRadiusProperty);
    }

    /// <summary>
    /// Sets the popup background brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the popup background for.</param>
    /// <param name="value">The Brush to use as the popup background.</param>
    public static void SetPopupBackground(UIElement element, Brush value)
    {
        element.SetValue(PopupBackgroundProperty, value);
    }

    /// <summary>
    /// Gets the popup background brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the popup background from.</param>
    /// <returns>The Brush used as the popup background.</returns>
    public static Brush GetPopupBackground(UIElement element)
    {
        return (Brush)element.GetValue(PopupBackgroundProperty);
    }

    /// <summary>
    /// Sets the popup foreground brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the popup foreground for.</param>
    /// <param name="value">The Brush to use as the popup foreground.</param>
    public static void SetPopupForeground(UIElement element, Brush value)
    {
        element.SetValue(PopupForegroundProperty, value);
    }

    /// <summary>
    /// Gets the popup foreground brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the popup foreground from.</param>
    /// <returns>The Brush used as the popup foreground.</returns>
    public static Brush GetPopupForeground(UIElement element)
    {
        return (Brush)element.GetValue(PopupForegroundProperty);
    }

    /// <summary>
    /// Sets the popup border brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the popup border brush for.</param>
    /// <param name="value">The Brush to use as the popup border brush.</param>
    public static void SetPopupBorderBrush(UIElement element, Brush value)
    {
        element.SetValue(PopupBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets the popup border brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the popup border brush from.</param>
    /// <returns>The Brush used as the popup border brush.</returns>
    public static Brush GetPopupBorderBrush(UIElement element)
    {
        return (Brush)element.GetValue(PopupBorderBrushProperty);
    }

    /// <summary>
    /// Sets the focused background brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the focused background for.</param>
    /// <param name="value">The Brush to use as the focused background.</param>
    public static void SetFocusedBackground(UIElement element, Brush value)
    {
        element.SetValue(FocusedBackgroundProperty, value);
    }

    /// <summary>
    /// Gets the focused background brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the focused background from.</param>
    /// <returns>The Brush used as the focused background.</returns>
    public static Brush GetFocusedBackground(UIElement element)
    {
        return (Brush)element.GetValue(FocusedBackgroundProperty);
    }

    /// <summary>
    /// Sets the focused foreground brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the focused foreground for.</param>
    /// <param name="value">The Brush to use as the focused foreground.</param>
    public static void SetFocusedForeground(UIElement element, Brush value)
    {
        element.SetValue(FocusedForegroundProperty, value);
    }

    /// <summary>
    /// Gets the focused foreground brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the focused foreground from.</param>
    /// <returns>The Brush used as the focused foreground.</returns>
    public static Brush GetFocusedForeground(UIElement element)
    {
        return (Brush)element.GetValue(FocusedForegroundProperty);
    }

    /// <summary>
    /// Sets the focused border brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the focused border brush for.</param>
    /// <param name="value">The Brush to use as the focused border brush.</param>
    public static void SetFocusedBorderBrush(UIElement element, Brush value)
    {
        element.SetValue(FocusedBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets the focused border brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the focused border brush from.</param>
    /// <returns>The Brush used as the focused border brush.</returns>
    public static Brush GetFocusedBorderBrush(UIElement element)
    {
        return (Brush)element.GetValue(FocusedBorderBrushProperty);
    }

    /// <summary>
    /// Sets the hover background brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the hover background for.</param>
    /// <param name="value">The Brush to use as the hover background.</param>
    public static void SetHoverBackground(UIElement element, Brush value)
    {
        element.SetValue(HoverBackgroundProperty, value);
    }

    /// <summary>
    /// Gets the hover background brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the hover background from.</param>
    /// <returns>The Brush used as the hover background.</returns>
    public static Brush GetHoverBackground(UIElement element)
    {
        return (Brush)element.GetValue(HoverBackgroundProperty);
    }

    /// <summary>
    /// Sets the hover foreground brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the hover foreground for.</param>
    /// <param name="value">The Brush to use as the hover foreground.</param>
    public static void SetHoverForeground(UIElement element, Brush value)
    {
        element.SetValue(HoverForegroundProperty, value);
    }

    /// <summary>
    /// Gets the hover foreground brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the hover foreground from.</param>
    /// <returns>The Brush used as the hover foreground.</returns>
    public static Brush GetHoverForeground(UIElement element)
    {
        return (Brush)element.GetValue(HoverForegroundProperty);
    }

    /// <summary>
    /// Sets the hover border brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the hover border brush for.</param>
    /// <param name="value">The Brush to use as the hover border brush.</param>
    public static void SetHoverBorderBrush(UIElement element, Brush value)
    {
        element.SetValue(HoverBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets the hover border brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the hover border brush from.</param>
    /// <returns>The Brush used as the hover border brush.</returns>
    public static Brush GetHoverBorderBrush(UIElement element)
    {
        return (Brush)element.GetValue(HoverBorderBrushProperty);
    }

    /// <summary>
    /// Sets the selected background brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the selected background for.</param>
    /// <param name="value">The Brush to use as the selected background.</param>
    public static void SetSelectedBackground(UIElement element, Brush value)
    {
        element.SetValue(SelectedBackgroundProperty, value);
    }

    /// <summary>
    /// Gets the selected background brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the selected background from.</param>
    /// <returns>The Brush used as the selected background.</returns>
    public static Brush GetSelectedBackground(UIElement element)
    {
        return (Brush)element.GetValue(SelectedBackgroundProperty);
    }

    /// <summary>
    /// Sets the selected foreground brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the selected foreground for.</param>
    /// <param name="value">The Brush to use as the selected foreground.</param>
    public static void SetSelectedForeground(UIElement element, Brush value)
    {
        element.SetValue(SelectedForegroundProperty, value);
    }

    /// <summary>
    /// Gets the selected foreground brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the selected foreground from.</param>
    /// <returns>The Brush used as the selected foreground.</returns>
    public static Brush GetSelectedForeground(UIElement element)
    {
        return (Brush)element.GetValue(SelectedForegroundProperty);
    }

    /// <summary>
    /// Sets the selected border brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the selected border brush for.</param>
    /// <param name="value">The Brush to use as the selected border brush.</param>
    public static void SetSelectedBorderBrush(UIElement element, Brush value)
    {
        element.SetValue(SelectedBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets the selected border brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the selected border brush from.</param>
    /// <returns>The Brush used as the selected border brush.</returns>
    public static Brush GetSelectedBorderBrush(UIElement element)
    {
        return (Brush)element.GetValue(SelectedBorderBrushProperty);
    }

    /// <summary>
    /// Sets the selected hover background brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the selected hover background for.</param>
    /// <param name="value">The Brush to use as the selected hover background.</param>
    public static void SetSelectedHoverBackground(UIElement element, Brush value)
    {
        element.SetValue(SelectedHoverBackgroundProperty, value);
    }

    /// <summary>
    /// Gets the selected hover background brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the selected hover background from.</param>
    /// <returns>The Brush used as the selected hover background.</returns>
    public static Brush GetSelectedHoverBackground(UIElement element)
    {
        return (Brush)element.GetValue(SelectedHoverBackgroundProperty);
    }

    /// <summary>
    /// Sets the selected hover foreground brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the selected hover foreground for.</param>
    /// <param name="value">The Brush to use as the selected hover foreground.</param>
    public static void SetSelectedHoverForeground(UIElement element, Brush value)
    {
        element.SetValue(SelectedHoverForegroundProperty, value);
    }

    /// <summary>
    /// Gets the selected hover foreground brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the selected hover foreground from.</param>
    /// <returns>The Brush used as the selected hover foreground.</returns>
    public static Brush GetSelectedHoverForeground(UIElement element)
    {
        return (Brush)element.GetValue(SelectedHoverForegroundProperty);
    }

    /// <summary>
    /// Sets the selected hover border brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the selected hover border brush for.</param>
    /// <param name="value">The Brush to use as the selected hover border brush.</param>
    public static void SetSelectedHoverBorderBrush(UIElement element, Brush value)
    {
        element.SetValue(SelectedHoverBorderBrushProperty, value);
    }

    /// <summary>
    /// Sets the inactive background brush for the specified UIElement when it is hovered over.
    /// </summary>
    /// <param name="element">The UIElement to set the inactive hover background for.</param>
    /// <param name="value">The Brush to use as the inactive hover background.</param>
    public static void SetInactiveSelectionMouseOverBackground(UIElement element, Brush value)
    {
        element.SetValue(InactiveSelectionMouseOverBackgroundProperty, value);
    }

    /// <summary>
    /// Gets the inactive background brush for the specified UIElement when it is hovered over.
    /// </summary>
    /// <param name="element">The UIElement to get the inactive hover background from.</param>
    /// <returns>The Brush used as the inactive hover background.</returns>
    public static Brush GetInactiveSelectionMouseOverBackground(UIElement element)
    {
        return (Brush)element.GetValue(InactiveSelectionMouseOverBackgroundProperty);
    }

    /// <summary>
    /// Sets the inactive foreground brush for the specified UIElement when it is hovered over.
    /// </summary>
    /// <param name="element">The UIElement to set the inactive hover foreground for.</param>
    /// <param name="value">The Brush to use as the inactive hover foreground.</param>
    public static void SetInactiveSelectionMouseOverForeground(UIElement element, Brush value)
    {
        element.SetValue(InactiveSelectionMouseOverForegroundProperty, value);
    }

    /// <summary>
    /// Gets the inactive foreground brush for the specified UIElement when it is hovered over.
    /// </summary>
    /// <param name="element">The UIElement to get the inactive hover foreground from.</param>
    /// <returns>The Brush used as the inactive hover foreground.</returns>
    public static Brush GetInactiveSelectionMouseOverForeground(UIElement element)
    {
        return (Brush)element.GetValue(InactiveSelectionMouseOverForegroundProperty);
    }

    /// <summary>
    /// Sets the inactive border brush for the specified UIElement when it is hovered over.
    /// </summary>
    /// <param name="element">The UIElement to set the inactive hover border brush for.</param>
    /// <param name="value">The Brush to use as the inactive hover border brush.</param>
    public static void SetInactiveSelectionMouseOverBorderBrush(UIElement element, Brush value)
    {
        element.SetValue(InactiveSelectionMouseOverBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets the inactive border brush for the specified UIElement when it is hovered over.
    /// </summary>
    /// <param name="element">The UIElement to get the inactive hover border brush from.</param>
    /// <returns>The Brush used as the inactive hover border brush.</returns>
    public static Brush GetInactiveSelectionMouseOverBorderBrush(UIElement element)
    {
        return (Brush)element.GetValue(InactiveSelectionMouseOverBorderBrushProperty);
    }

    /// <summary>
    /// Gets the selected hover border brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the selected hover border brush from.</param>
    /// <returns>The Brush used as the selected hover border brush.</returns>
    public static Brush GetSelectedHoverBorderBrush(UIElement element)
    {
        return (Brush)element.GetValue(SelectedHoverBorderBrushProperty);
    }

    /// <summary>
    /// Sets the selected background brush for the specified UIElement when it is not focused.
    /// </summary>
    /// <param name="element">The UIElement to set the selected background for when not focused.</param>
    /// <param name="value">The Brush to use as the selected background when not focused.</param>
    public static void SetSelectedNoFocusedBackground(UIElement element, Brush value)
    {
        element.SetValue(SelectedNoFocusedBackgroundProperty, value);
    }

    /// <summary>
    /// Gets the selected background brush for the specified UIElement when it is not focused.
    /// </summary>
    /// <param name="element">The UIElement to get the selected background from when not focused.</param>
    /// <returns>The Brush used as the selected background when not focused.</returns>
    public static Brush GetSelectedNoFocusBackground(UIElement element)
    {
        return (Brush)element.GetValue(SelectedNoFocusedBackgroundProperty);
    }

    /// <summary>
    /// Sets the selected foreground brush for the specified UIElement when it is not focused.
    /// </summary>
    /// <param name="element">The UIElement to set the selected foreground for when not focused.</param>
    /// <param name="value">The Brush to use as the selected foreground when not focused.</param>
    public static void SetSelectedNoFocusedForeground(UIElement element, Brush value)
    {
        element.SetValue(SelectedNoFocusedForegroundProperty, value);
    }

    /// <summary>
    /// Gets the selected foreground brush for the specified UIElement when it is not focused.
    /// </summary>
    /// <param name="element">The UIElement to get the selected foreground from when not focused.</param>
    /// <returns>The Brush used as the selected foreground when not focused.</returns>
    public static Brush GetSelectedNoFocusedForeground(UIElement element)
    {
        return (Brush)element.GetValue(SelectedNoFocusedForegroundProperty);
    }

    /// <summary>
    /// Sets the selected border brush for the specified UIElement when it is not focused.
    /// </summary>
    /// <param name="element">The UIElement to set the selected border brush for when not focused.</param>
    /// <param name="value">The Brush to use as the selected border brush when not focused.</param>
    public static void SetSelectedNoFocusedBorderBrush(UIElement element, Brush value)
    {
        element.SetValue(SelectedNoFocusedBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets the selected border brush for the specified UIElement when it is not focused.
    /// </summary>
    /// <param name="element">The UIElement to get the selected border brush from when not focused.</param>
    /// <returns>The Brush used as the selected border brush when not focused.</returns>
    public static Brush GetSelectedNoFocusedBorderBrush(UIElement element)
    {
        return (Brush)element.GetValue(SelectedNoFocusedBorderBrushProperty);
    }

    /// <summary>
    /// Sets the hover background brush for the specified UIElement when focused.
    /// </summary>
    /// <param name="element">The UIElement to set the hover background for when focused.</param>
    /// <param name="value">The Brush to use as the hover background when focused.</param>
    public static void SetHoverFocusedBackground(UIElement element, Brush value)
    {
        element.SetValue(HoverFocusedBackgroundProperty, value);
    }

    /// <summary>
    /// Gets the hover background brush for the specified UIElement when focused.
    /// </summary>
    /// <param name="element">The UIElement to get the hover background from when focused.</param>
    /// <returns>The Brush used as the hover background when focused.</returns>
    public static Brush GetHoverFocusedBackground(UIElement element)
    {
        return (Brush)element.GetValue(HoverFocusedBackgroundProperty);
    }

    /// <summary>
    /// Sets the hover foreground brush for the specified UIElement when focused.
    /// </summary>
    /// <param name="element">The UIElement to set the hover foreground for when focused.</param>
    /// <param name="value">The Brush to use as the hover foreground when focused.</param>
    public static void SetHoverFocusedForeground(UIElement element, Brush value)
    {
        element.SetValue(HoverFocusedForegroundProperty, value);
    }

    /// <summary>
    /// Gets the hover foreground brush for the specified UIElement when focused.
    /// </summary>
    /// <param name="element">The UIElement to get the hover foreground from when focused.</param>
    /// <returns>The Brush used as the hover foreground when focused.</returns>
    public static Brush GetHoverFocusedForeground(UIElement element)
    {
        return (Brush)element.GetValue(HoverFocusedForegroundProperty);
    }

    /// <summary>
    /// Sets the hover border brush for the specified UIElement when focused.
    /// </summary>
    /// <param name="element">The UIElement to set the hover border brush for when focused.</param>
    /// <param name="value">The Brush to use as the hover border brush when focused.</param>
    public static void SetHoverFocusedBorderBrush(UIElement element, Brush value)
    {
        element.SetValue(HoverFocusedBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets the hover border brush for the specified UIElement when focused.
    /// </summary>
    /// <param name="element">The UIElement to get the hover border brush from when focused.</param>
    /// <returns>The Brush used as the hover border brush when focused.</returns>
    public static Brush GetHoverFocusedBorderBrush(UIElement element)
    {
        return (Brush)element.GetValue(HoverFocusedBorderBrushProperty);
    }

    /// <summary>
    /// Sets the glyph brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the glyph brush for.</param>
    /// <param name="value">The Brush to use for the glyph.</param>
    public static void SetGlyphBrush(UIElement element, Brush value)
    {
        element.SetValue(GlyphBrushProperty, value);
    }

    /// <summary>
    /// Gets the glyph brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the glyph brush from.</param>
    /// <returns>The Brush used for the glyph.</returns>
    public static Brush GetGlyphBrush(UIElement element)
    {
        return (Brush)element.GetValue(GlyphBrushProperty);
    }

    /// <summary>
    /// Sets the mouse-over glyph brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the mouse-over glyph brush for.</param>
    /// <param name="value">The Brush to use for the mouse-over glyph.</param>
    public static void SetMouseOverGlyphBrush(UIElement element, Brush value)
    {
        element.SetValue(MouseOverGlyphBrushProperty, value);
    }

    /// <summary>
    /// Gets the mouse-over glyph brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the mouse-over glyph brush from.</param>
    /// <returns>The Brush used for the mouse-over glyph.</returns>
    public static Brush GetMouseOverGlyphBrush(UIElement element)
    {
        return (Brush)element.GetValue(MouseOverGlyphBrushProperty);
    }

    /// <summary>
    /// Sets the pressed glyph brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the pressed glyph brush for.</param>
    /// <param name="value">The Brush to use for the pressed glyph.</param>
    public static void SetPressedGlyphBrush(UIElement element, Brush value)
    {
        element.SetValue(PressedGlyphBrushProperty, value);
    }

    /// <summary>
    /// Gets the pressed glyph brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the pressed glyph brush from.</param>
    /// <returns>The Brush used for the pressed glyph.</returns>
    public static Brush GetPressedGlyphBrush(UIElement element)
    {
        return (Brush)element.GetValue(PressedGlyphBrushProperty);
    }

    /// <summary>
    /// Sets the disabled glyph brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the disabled glyph brush for.</param>
    /// <param name="value">The Brush to use for the disabled glyph.</param>
    public static void SetDisabledGlyphBrush(UIElement element, Brush value)
    {
        element.SetValue(DisabledGlyphBrushProperty, value);
    }

    /// <summary>
    /// Gets the disabled glyph brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the disabled glyph brush from.</param>
    /// <returns>The Brush used for the disabled glyph.</returns>
    public static Brush GetDisabledGlyphBrush(UIElement element)
    {
        return (Brush)element.GetValue(DisabledGlyphBrushProperty);
    }

    /// <summary>
    /// Sets the placeholder foreground brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the placeholder foreground for.</param>
    /// <param name="value">The Brush to use as the placeholder foreground.</param>
    public static void SetPlaceholderForeground(UIElement element, Brush value)
    {
        element.SetValue(PlaceholderForegroundProperty, value);
    }

    /// <summary>
    /// Gets the placeholder foreground brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the placeholder foreground from.</param>
    /// <returns>The Brush used as the placeholder foreground.</returns>
    public static Brush GetPlaceholderForeground(UIElement element)
    {
        return (Brush)element.GetValue(PlaceholderForegroundProperty);
    }

    /// <summary>
    /// Sets the selection brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the selection brush for.</param>
    /// <param name="value">The Brush to use as the selection brush.</param>
    public static void SetSelectionBrush(UIElement element, Brush value)
    {
        element.SetValue(SelectionBrushProperty, value);
    }

    /// <summary>
    /// Gets the selection brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the selection brush from.</param>
    /// <returns>The Brush used as the selection brush.</returns>
    public static Brush GetSelectionBrush(UIElement element)
    {
        return (Brush)element.GetValue(SelectionBrushProperty);
    }

    /// <summary>
    /// Gets the placeholder text for the specified DependencyObject.
    /// </summary>
    /// <param name="dependencyObject">The DependencyObject to get the placeholder from.</param>
    /// <returns>The placeholder string.</returns>
    public static string GetPlaceholder(DependencyObject dependencyObject)
    {
        return (string)dependencyObject.GetValue(PlaceholderProperty);
    }

    /// <summary>
    /// Sets the placeholder text for the specified DependencyObject.
    /// </summary>
    /// <param name="dependencyObject">The DependencyObject to set the placeholder for.</param>
    /// <param name="value">The placeholder string to set.</param>
    public static void SetPlaceholder(DependencyObject dependencyObject, string value)
    {
        dependencyObject.SetValue(PlaceholderProperty, value);
    }

    /// <summary>
    /// Sets the highlight background brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the highlight background for.</param>
    /// <param name="value">The Brush to use as the highlight background.</param>
    public static void SetHighlightBackground(UIElement element, Brush value)
    {
        element.SetValue(HighlightBackgroundProperty, value);
    }

    /// <summary>
    /// Gets the highlight background brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the highlight background from.</param>
    /// <returns>The Brush used as the highlight background.</returns>
    public static Brush GetHighlightBackground(UIElement element)
    {
        return (Brush)element.GetValue(HighlightBackgroundProperty);
    }

    /// <summary>
    /// Sets the highlight border brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the highlight border brush for.</param>
    /// <param name="value">The Brush to use as the highlight border brush.</param>
    public static void SetHighlightBorderBrush(UIElement element, Brush value)
    {
        element.SetValue(HighlightBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets the highlight border brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the highlight border brush from.</param>
    /// <returns>The Brush used as the highlight border brush.</returns>
    public static Brush GetHighlightBorderBrush(UIElement element)
    {
        return (Brush)element.GetValue(HighlightBorderBrushProperty);
    }

    /// <summary>
    /// Sets the submenu background brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the submenu background brush for.</param>
    /// <param name="value">The Brush to use as the submenu background.</param>
    public static void SetSubMenuBackground(UIElement element, Brush value)
    {
        element.SetValue(SubMenuBackgroundProperty, value);
    }

    /// <summary>
    /// Gets the submenu background brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the submenu background brush from.</param>
    /// <returns>The Brush used as the submenu background.</returns>
    public static Brush GetSubMenuBackground(UIElement element)
    {
        return (Brush)element.GetValue(SubMenuBackgroundProperty);
    }

    /// <summary>
    /// Sets the submenu border brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to set the submenu border brush for.</param>
    /// <param name="value">The Brush to use as the submenu border brush.</param>
    public static void SetSubMenuBorderBrush(UIElement element, Brush value)
    {
        element.SetValue(SubMenuBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets the submenu border brush for the specified UIElement.
    /// </summary>
    /// <param name="element">The UIElement to get the submenu border brush from.</param>
    /// <returns>The Brush used as the submenu border brush.</returns>
    public static Brush GetSubMenuBorderBrush(UIElement element)
    {
        return (Brush)element.GetValue(SubMenuBorderBrushProperty);
    }

    /// <summary>
    /// Sets the default chrome brush for the specified UIElement.
    /// </summary>
    public static void SetChromeBrush(UIElement element, Brush value)
    {
        element.SetValue(ChromeBrushProperty, value);
    }

    /// <summary>
    /// Gets the default chrome brush for the specified UIElement.
    /// </summary>
    public static Brush GetChromeBrush(UIElement element)
    {
        return (Brush)element.GetValue(ChromeBrushProperty);
    }

    /// <summary>
    /// Sets the chrome brush for the specified UIElement when the mouse is over it.
    /// </summary>
    public static void SetMouseOverChromeBrush(UIElement element, Brush value)
    {
        element.SetValue(MouseOverChromeBrushProperty, value);
    }

    /// <summary>
    /// Gets the chrome brush for the specified UIElement when the mouse is over it.
    /// </summary>
    public static Brush GetMouseOverChromeBrush(UIElement element)
    {
        return (Brush)element.GetValue(MouseOverChromeBrushProperty);
    }

    /// <summary>
    /// Sets the chrome brush for the specified UIElement when it is pressed.
    /// </summary>
    public static void SetPressedChromeBrush(UIElement element, Brush value)
    {
        element.SetValue(PressedChromeBrushProperty, value);
    }

    /// <summary>
    /// Gets the chrome brush for the specified UIElement when it is pressed.
    /// </summary>
    public static Brush GetPressedChromeBrush(UIElement element)
    {
        return (Brush)element.GetValue(PressedChromeBrushProperty);
    }

    /// <summary>
    /// Sets the chrome brush for the specified UIElement when it is disabled.
    /// </summary>
    public static void SetDisabledChromeBrush(UIElement element, Brush value)
    {
        element.SetValue(DisabledChromeBrushProperty, value);
    }

    /// <summary>
    /// Gets the chrome brush for the specified UIElement when it is disabled.
    /// </summary>
    public static Brush GetDisabledChromeBrush(UIElement element)
    {
        return (Brush)element.GetValue(DisabledChromeBrushProperty);
    }

    #endregion
}