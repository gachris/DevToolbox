using System.Data;
using System;
using System.Windows;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// Represents a summary item for a DataGrid, allowing configuration of various summary types and display properties.
/// </summary>
public class DataGridSummaryItem : DependencyObject
{
    #region Fields/Consts

    /// <summary>
    /// Identifies the <see cref="CustomCallback"/> dependency property.
    /// Specifies a custom callback function to compute the summary value for the custom summary type.
    /// </summary>
    public static readonly DependencyProperty CustomCallbackProperty =
        DependencyProperty.Register(nameof(CustomCallback), typeof(Func<DataTable, string, object>), typeof(DataGridSummaryItem), new FrameworkPropertyMetadata(null));

    /// <summary>
    /// Identifies the <see cref="Tag"/> dependency property.
    /// Used to store a custom tag object associated with the summary item.
    /// </summary>
    public static readonly DependencyProperty TagProperty =
        DependencyProperty.Register(nameof(Tag), typeof(object), typeof(DataGridSummaryItem), new FrameworkPropertyMetadata(default));

    /// <summary>
    /// Identifies the <see cref="SummaryType"/> dependency property.
    /// Specifies the type of summary calculation to perform (e.g., Sum, Average).
    /// </summary>
    public static readonly DependencyProperty SummaryTypeProperty =
        DependencyProperty.Register(nameof(SummaryType), typeof(DataGridSummaryItemType), typeof(DataGridSummaryItem), new FrameworkPropertyMetadata(DataGridSummaryItemType.None));

    /// <summary>
    /// Identifies the <see cref="FieldName"/> dependency property.
    /// Specifies the name of the field for which the summary is calculated.
    /// </summary>
    public static readonly DependencyProperty FieldNameProperty =
        DependencyProperty.Register(nameof(FieldName), typeof(string), typeof(DataGridSummaryItem), new FrameworkPropertyMetadata(default(string)));

    /// <summary>
    /// Identifies the <see cref="DisplayFormat"/> dependency property.
    /// Specifies the format string used to display the summary value.
    /// </summary>
    public static readonly DependencyProperty DisplayFormatProperty =
        DependencyProperty.Register(nameof(DisplayFormat), typeof(string), typeof(DataGridSummaryItem), new FrameworkPropertyMetadata(default(string)));

    /// <summary>
    /// Identifies the <see cref="ShowInColumn"/> dependency property.
    /// Specifies the column name where the summary value should be displayed.
    /// </summary>
    public static readonly DependencyProperty ShowInColumnProperty =
        DependencyProperty.Register(nameof(ShowInColumn), typeof(string), typeof(DataGridSummaryItem), new FrameworkPropertyMetadata(default(string)));

    /// <summary>
    /// Identifies the <see cref="Visible"/> dependency property.
    /// Determines whether the summary item is visible in the DataGrid.
    /// </summary>
    public static readonly DependencyProperty VisibleProperty =
        DependencyProperty.Register(nameof(Visible), typeof(bool), typeof(DataGridSummaryItem), new FrameworkPropertyMetadata(true));

    /// <summary>
    /// Identifies the <see cref="Style"/> dependency property.
    /// Specifies the style applied to the summary item when it is displayed in the DataGrid.
    /// </summary>
    public static readonly DependencyProperty StyleProperty =
        DependencyProperty.Register(nameof(Style), typeof(Style), typeof(DataGridSummaryItem), new FrameworkPropertyMetadata(default(Style)));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the custom callback function to compute the summary value for the custom summary type.
    /// The callback receives the <see cref="DataTable"/> and the field name as arguments.
    /// </summary>
    public Func<DataTable, string, object>? CustomCallback
    {
        get => (Func<DataTable, string, object>?)GetValue(CustomCallbackProperty);
        set => SetValue(CustomCallbackProperty, value);
    }

    /// <summary>
    /// Gets or sets a custom tag object associated with the summary item.
    /// </summary>
    public object Tag
    {
        get => GetValue(TagProperty);
        set => SetValue(TagProperty, value);
    }

    /// <summary>
    /// Gets or sets the type of summary calculation to perform (e.g., Sum, Average).
    /// </summary>
    public DataGridSummaryItemType SummaryType
    {
        get => (DataGridSummaryItemType)GetValue(SummaryTypeProperty);
        set => SetValue(SummaryTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets the name of the field for which the summary is calculated.
    /// </summary>
    public string FieldName
    {
        get => (string)GetValue(FieldNameProperty);
        set => SetValue(FieldNameProperty, value);
    }

    /// <summary>
    /// Gets or sets the format string used to display the summary value.
    /// </summary>
    public string DisplayFormat
    {
        get => (string)GetValue(DisplayFormatProperty);
        set => SetValue(DisplayFormatProperty, value);
    }

    /// <summary>
    /// Gets or sets the column name where the summary value should be displayed.
    /// </summary>
    public string ShowInColumn
    {
        get => (string)GetValue(ShowInColumnProperty);
        set => SetValue(ShowInColumnProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the summary item is visible in the DataGrid.
    /// </summary>
    public bool Visible
    {
        get => (bool)GetValue(VisibleProperty);
        set => SetValue(VisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets the style applied to the summary item when it is displayed in the DataGrid.
    /// </summary>
    public Style Style
    {
        get => (Style)GetValue(StyleProperty);
        set => SetValue(StyleProperty, value);
    }

    #endregion
}