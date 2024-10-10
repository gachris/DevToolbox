using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DevToolbox.Wpf.Windows;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// A control representing a color chip that allows users to switch, reset, or choose colors using a dialog.
/// This control provides commands for color manipulation and displays primary and secondary colors.
/// </summary>
public partial class ColorChip : Control
{
    #region Fields/Consts

    /// <summary>
    /// Command for switching primary and secondary colors.
    /// </summary>
    private static readonly RoutedUICommand _switchColorsCommand = new(nameof(SwitchColorsCommand), nameof(SwitchColorsCommand), typeof(ColorChip));

    /// <summary>
    /// Command for resetting colors to their default values.
    /// </summary>
    private static readonly RoutedUICommand _resetColorsCommand = new(nameof(ResetColorsCommand), nameof(ResetColorsCommand), typeof(ColorChip));

    /// <summary>
    /// Command for showing the color picker dialog to change the primary color.
    /// </summary>
    private static readonly RoutedUICommand _showDialogCommand = new(nameof(ShowDialogCommand), nameof(ShowDialogCommand), typeof(ColorChip));

    /// <summary>
    /// Dependency property for the primary color of the color chip.
    /// </summary>
    public static readonly DependencyProperty PrimaryColorProperty =
        DependencyProperty.Register("PrimaryColor", typeof(Color), typeof(ColorChip), new PropertyMetadata(Colors.Black));

    /// <summary>
    /// Dependency property for the default primary color.
    /// </summary>
    public static readonly DependencyProperty PrimaryDefaultColorProperty =
        DependencyProperty.Register("PrimaryDefaultColor", typeof(Color), typeof(ColorChip), new PropertyMetadata(Colors.Black));

    /// <summary>
    /// Dependency property for the secondary color of the color chip.
    /// </summary>
    public static readonly DependencyProperty SecondaryColorProperty =
        DependencyProperty.Register("SecondaryColor", typeof(Color), typeof(ColorChip), new PropertyMetadata(Colors.White));

    /// <summary>
    /// Dependency property for the default secondary color.
    /// </summary>
    public static readonly DependencyProperty SecondaryDefaultColorProperty =
        DependencyProperty.Register("SecondaryDefaultColor", typeof(Color), typeof(ColorChip), new PropertyMetadata(Colors.White));

    #endregion

    #region Properties

    /// <summary>
    /// Gets the command for switching primary and secondary colors.
    /// </summary>
    public static RoutedUICommand SwitchColorsCommand => _switchColorsCommand;

    /// <summary>
    /// Gets the command for resetting colors to their default values.
    /// </summary>
    public static RoutedUICommand ResetColorsCommand => _resetColorsCommand;

    /// <summary>
    /// Gets the command for showing the color picker dialog.
    /// </summary>
    public static RoutedUICommand ShowDialogCommand => _showDialogCommand;

    /// <summary>
    /// Gets or sets the primary color of the color chip.
    /// </summary>
    public Color PrimaryColor
    {
        get => (Color)GetValue(PrimaryColorProperty);
        set => SetValue(PrimaryColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the secondary color of the color chip.
    /// </summary>
    public Color SecondaryColor
    {
        get => (Color)GetValue(SecondaryColorProperty);
        set => SetValue(SecondaryColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the default primary color.
    /// </summary>
    public Color PrimaryDefaultColor
    {
        get => (Color)GetValue(PrimaryDefaultColorProperty);
        set => SetValue(PrimaryDefaultColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the default secondary color.
    /// </summary>
    public Color SecondaryDefaultColor
    {
        get => (Color)GetValue(SecondaryDefaultColorProperty);
        set => SetValue(SecondaryDefaultColorProperty, value);
    }

    #endregion

    static ColorChip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorChip), new FrameworkPropertyMetadata(typeof(ColorChip)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorChip"/> class.
    /// Sets up command bindings for switching, resetting, and showing the color dialog.
    /// </summary>
    public ColorChip()
    {
        CommandBindings.Add(new CommandBinding(SwitchColorsCommand, OnSwitchColorsExecuted, OnSwitchColorsCanExecute));
        CommandBindings.Add(new CommandBinding(ResetColorsCommand, OnResetColorsExecuted, OnResetColorsCanExecute));
        CommandBindings.Add(new CommandBinding(ShowDialogCommand, OnShowDialogExecuted, OnShowDialogCanExecute));
    }

    #region Methods

    /// <summary>
    /// Executes the switch colors command by swapping the primary and secondary colors.
    /// </summary>
    private void OnSwitchColorsExecuted(object sender, ExecutedRoutedEventArgs e) => (SecondaryColor, PrimaryColor) = (PrimaryColor, SecondaryColor);

    /// <summary>
    /// Determines if the switch colors command can execute. Always returns true.
    /// </summary>
    private void OnSwitchColorsCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

    /// <summary>
    /// Executes the reset colors command by resetting the primary and secondary colors to their default values.
    /// </summary>
    private void OnResetColorsExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        PrimaryColor = PrimaryDefaultColor;
        SecondaryColor = SecondaryDefaultColor;
    }

    /// <summary>
    /// Determines if the reset colors command can execute. Always returns true.
    /// </summary>
    private void OnResetColorsCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

    /// <summary>
    /// Executes the show dialog command, opens a color picker dialog, and updates the primary color if a new color is selected.
    /// </summary>
    private void OnShowDialogExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var dialog = new ColorPickerDialog();
        var initialColor = PrimaryColor;
        dialog.colorPicker.SelectedColor = initialColor;
        dialog.colorPicker.InitialColor = initialColor;

        if (dialog.ShowDialog() == true && dialog.colorPicker.SelectedColor != initialColor)
            PrimaryColor = dialog.colorPicker.SelectedColor;
    }

    /// <summary>
    /// Determines if the show dialog command can execute. Always returns true.
    /// </summary>
    private void OnShowDialogCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

    #endregion
}