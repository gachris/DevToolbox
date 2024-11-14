using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevToolbox.Wpf.Controls;
using DevToolbox.Wpf.Windows;

namespace DevToolbox.Wpf.Helpers;

/// <summary>
/// Provides helper methods, commands, and attached properties for enhancing the behavior of <see cref="DataGrid"/> controls.
/// </summary>
public class DataGridHelper
{
    #region Fields/Consts

    /// <summary>
    /// Command to clear row filters in the <see cref="DataGrid"/>.
    /// </summary>
    private static readonly RoutedUICommand _clearRowFilterCommand = new(
        nameof(ClearRowFilterCommand),
        nameof(ClearRowFilterCommand),
        typeof(DataGrid));

    /// <summary>
    /// Command to open the column chooser window for the <see cref="DataGrid"/>.
    /// </summary>
    private static readonly RoutedUICommand _openColumnChooserCommand = new(
        nameof(OpenColumnChooserCommand),
        nameof(OpenColumnChooserCommand),
        typeof(DataGrid));

    /// <summary>
    /// Attached property to show or hide row numbers in the <see cref="DataGrid"/>.
    /// </summary>
    public static readonly DependencyProperty ShowRowNumberProperty =
        DependencyProperty.RegisterAttached(
            "ShowRowNumber",
            typeof(bool),
            typeof(DataGridHelper),
            new FrameworkPropertyMetadata(false, OnShowRowNumberChanged));

    /// <summary>
    /// Attached property to allow or disallow copying data from the <see cref="DataGrid"/>.
    /// </summary>
    public static readonly DependencyProperty AllowUserToCopyProperty =
        DependencyProperty.RegisterAttached(
            "AllowUserToCopy",
            typeof(bool),
            typeof(DataGridHelper),
            new FrameworkPropertyMetadata(true, OnAllowUserToCopyChanged));

    /// <summary>
    /// Attached property to enable or disable the column chooser feature in the <see cref="DataGrid"/>.
    /// </summary>
    public static readonly DependencyProperty AllowColumnChooserProperty =
        DependencyProperty.RegisterAttached(
            "AllowColumnChooser",
            typeof(bool),
            typeof(DataGridHelper),
            new FrameworkPropertyMetadata(true, OnAllowColumnChooserChanged));

    /// <summary>
    /// Attached property to store the reference to the column chooser window for the <see cref="DataGrid"/>.
    /// </summary>
    private static readonly DependencyProperty ColumnChooserWindowProperty =
        DependencyProperty.RegisterAttached(
            "ColumnChooserWindow",
            typeof(WindowEx),
            typeof(DataGridHelper),
            new PropertyMetadata(null));

    #endregion

    #region Properties

    /// <summary>
    /// Gets the command used for pasting clipboard data into a <see cref="DataGrid"/>.
    /// </summary>
    public static RoutedUICommand PasteCommand => ApplicationCommands.Paste;

    /// <summary>
    /// Gets the command used for clearing row filters in a <see cref="DataGrid"/>.
    /// </summary>
    public static RoutedUICommand ClearRowFilterCommand => _clearRowFilterCommand;

    /// <summary>
    /// Gets the command used for opening the column chooser in a <see cref="DataGrid"/>.
    /// </summary>
    public static RoutedUICommand OpenColumnChooserCommand => _openColumnChooserCommand;

    #endregion

    static DataGridHelper()
    {
        CommandManager.RegisterClassCommandBinding(typeof(DataGrid), new CommandBinding(PasteCommand, OnPasteExecuted, OnPasteCanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(DataGrid), new CommandBinding(ClearRowFilterCommand, OnClearRowFilterExecuted, OnClearRowFilterCanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(DataGrid), new CommandBinding(OpenColumnChooserCommand, OpenColumnChooserExecuted, OpenColumnChooserCanExecute));
    }

    #region Methods

    /// <summary>
    /// Gets the value of the ShowRowNumber attached property.
    /// </summary>
    public static bool GetShowRowNumber(DependencyObject obj)
    {
        return (bool)obj.GetValue(ShowRowNumberProperty);
    }

    /// <summary>
    /// Sets the value of the ShowRowNumber attached property.
    /// </summary>
    public static void SetShowRowNumber(DependencyObject obj, bool value)
    {
        obj.SetValue(ShowRowNumberProperty, value);
    }

    /// <summary>
    /// Gets the value of the AllowColumnChooser attached property.
    /// </summary>
    public static bool GetAllowColumnChooser(DependencyObject obj)
    {
        return (bool)obj.GetValue(AllowColumnChooserProperty);
    }

    /// <summary>
    /// Sets the value of the AllowColumnChooser attached property.
    /// </summary>
    public static void SetAllowColumnChooser(DependencyObject obj, bool value)
    {
        obj.SetValue(AllowColumnChooserProperty, value);
    }

    /// <summary>
    /// Gets the value of the AllowUserToCopy attached property.
    /// </summary>
    public static bool GetAllowUserToCopy(DependencyObject obj)
    {
        return (bool)obj.GetValue(AllowUserToCopyProperty);
    }

    /// <summary>
    /// Sets the value of the AllowUserToCopy attached property.
    /// </summary>
    public static void SetAllowUserToCopy(DependencyObject obj, bool value)
    {
        obj.SetValue(AllowUserToCopyProperty, value);
    }

    /// <summary>
    /// Gets the current instance of <see cref="WindowEx"/> used for the column chooser window in the specified <see cref="DependencyObject"/>.
    /// </summary>
    private static WindowEx? GetColumnChooserWindow(DependencyObject obj)
    {
        return obj.GetValue(ColumnChooserWindowProperty) as WindowEx;
    }

    /// <summary>
    /// Sets the instance of <see cref="WindowEx"/> used for the column chooser window in the specified <see cref="DependencyObject"/>.
    /// </summary>
    private static void SetColumnChooserWindow(DependencyObject obj, WindowEx? value)
    {
        obj.SetValue(ColumnChooserWindowProperty, value);
    }
    /// <summary>
    /// Handles changes to the <see cref="ShowRowNumberProperty"/> attached property.
    /// Updates the row number visibility in the <see cref="DataGrid"/> based on the new property value.
    /// </summary>
    /// <param name="d">The <see cref="DependencyObject"/> where the property is changed.</param>
    /// <param name="e">Provides data for the <see cref="DependencyPropertyChangedEventArgs"/> event.</param>
    private static void OnShowRowNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid dataGrid)
            return;

        var showRowNumber = (bool)e.NewValue;

        // Subscribe or unsubscribe from row events based on the new value.
        if (showRowNumber)
        {
            dataGrid.LoadingRow += DataGrid_LoadingRow;
            dataGrid.UnloadingRow += DataGrid_UnloadingRow;
        }
        else
        {
            dataGrid.LoadingRow -= DataGrid_LoadingRow;
            dataGrid.UnloadingRow -= DataGrid_UnloadingRow;
        }

        // Update the row numbers immediately when the property changes.
        UpdateRowNumbers(dataGrid, showRowNumber);
    }

    /// <summary>
    /// Handles changes to the <see cref="AllowUserToCopyProperty"/> attached property.
    /// Enables or disables the user's ability to copy data from the <see cref="DataGrid"/> based on the new property value.
    /// </summary>
    /// <param name="d">The <see cref="DependencyObject"/> where the property is changed.</param>
    /// <param name="e">Provides data for the <see cref="DependencyPropertyChangedEventArgs"/> event.</param>
    private static void OnAllowUserToCopyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid dataGrid)
            return;

        var allowUserToCopy = (bool)e.NewValue;

        // Attach or detach the PreviewKeyDown event handler based on the new value.
        if (allowUserToCopy)
        {
            dataGrid.PreviewKeyDown -= DataGrid_PreviewKeyDown;
        }
        else
        {
            dataGrid.PreviewKeyDown += DataGrid_PreviewKeyDown;
        }
    }

    /// <summary>
    /// Handles changes to the <see cref="AllowColumnChooserProperty"/> attached property.
    /// Manages the visibility of the column chooser window based on the new property value.
    /// </summary>
    /// <param name="d">The <see cref="DependencyObject"/> where the property is changed.</param>
    /// <param name="e">Provides data for the <see cref="DependencyPropertyChangedEventArgs"/> event.</param>
    private static void OnAllowColumnChooserChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid dataGrid)
            return;

        var allowColumnChooser = (bool)e.NewValue;
        var columnChooserWindow = GetColumnChooserWindow(dataGrid);

        // Close and clear the column chooser window if the feature is disabled.
        if (!allowColumnChooser && columnChooserWindow != null)
        {
            try
            {
                columnChooserWindow.Close();
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed.
                Console.WriteLine($"Error closing column chooser window: {ex.Message}");
            }
            finally
            {
                SetColumnChooserWindow(dataGrid, null);
            }
        }
    }

    /// <summary>
    /// Updates row numbers in the <see cref="DataGrid"/> based on the ShowRowNumber property.
    /// </summary>
    private static void UpdateRowNumbers(DataGrid dataGrid, bool show)
    {
        foreach (var item in dataGrid.Items)
        {
            if (dataGrid.ItemContainerGenerator.ContainerFromItem(item) is DataGridRow row && row.GetIndex() != -1)
            {
                row.Header = show ? (row.GetIndex() + 1).ToString() : null;
            }
        }
    }

    #endregion

    #region Commands

    /// <summary>
    /// Executes the paste command for a <see cref="DataGrid"/>.
    /// </summary>
    private static void OnPasteExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var data = ClipboardHelper.GetData();

        if (data is null)
        {
            return;
        }

        var dataGrid = (DataGrid)sender;
        var minRowIndex = dataGrid.Items.IndexOf(dataGrid.CurrentItem);
        var maxRowIndex = dataGrid.Items.Count - 1;
        var minColumnDisplayIndex = (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow) ? dataGrid.Columns.IndexOf(dataGrid.CurrentColumn) : 0;
        var maxColumnDisplayIndex = dataGrid.Columns.Count - 1;
        var rowDataIndex = 0;

        for (var i = minRowIndex; i <= maxRowIndex && rowDataIndex < data.Count; i++, rowDataIndex++)
        {
            var columnDataIndex = 0;
            for (var j = minColumnDisplayIndex;
                 j <= maxColumnDisplayIndex && columnDataIndex < data[rowDataIndex].Length;
                 j++, columnDataIndex++)
            {
                DataGridColumn column = dataGrid.ColumnFromDisplayIndex(j);
                column.OnPastingCellClipboardContent(dataGrid.Items[i], data[rowDataIndex][columnDataIndex]);
            }
        }
    }

    /// <summary>
    /// Determines if the paste command can be executed.
    /// </summary>
    private static void OnPasteCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is DataGrid;
    }

    /// <summary>
    /// Executes the ClearRowFilter command, removing all filters from the columns of the <see cref="DataGrid"/>.
    /// </summary>
    /// <param name="sender">The <see cref="DataGrid"/> that the command is executed on.</param>
    /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the event data.</param>
    private static void OnClearRowFilterExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var dataGrid = (DataGrid)sender;

        foreach (var dataGridColumnHeader in dataGrid.Columns)
        {
            var autoFilter = AutoFilter.GetAutoFilter(dataGrid);
            autoFilter?.RemoveAllFilter(dataGridColumnHeader.SortMemberPath);
        }
    }

    /// <summary>
    /// Determines whether the ClearRowFilter command can execute.
    /// </summary>
    /// <param name="sender">The <see cref="DataGrid"/> that the command is executed on.</param>
    /// <param name="e">The <see cref="CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
    private static void OnClearRowFilterCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is DataGrid;
    }

    /// <summary>
    /// Executes the OpenColumnChooser command, displaying a window that allows the user to select which columns are visible in the <see cref="DataGrid"/>.
    /// </summary>
    /// <param name="sender">The <see cref="DataGrid"/> that the command is executed on.</param>
    /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the event data.</param>
    private static void OpenColumnChooserExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var dataGrid = (DataGrid)sender;
        var columnChooserWindow = GetColumnChooserWindow(dataGrid);

        if (GetAllowColumnChooser(dataGrid) && columnChooserWindow == null)
        {
            columnChooserWindow = new WindowEx
            {
                Width = 160,
                Topmost = true,
                SizeToContent = SizeToContent.Height,
                MaxHeight = dataGrid.ActualHeight,
                Title = "Column Chooser",
                WindowStyle = WindowStyle.ToolWindow,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Content = new DataGridColumnChooser(dataGrid),
            };

            SetColumnChooserWindow(dataGrid, columnChooserWindow);

            columnChooserWindow.SourceInitialized += (sender, e) =>
            {
                var locationFromWindow = dataGrid.TranslatePoint(new Point(0, 0), dataGrid);
                var locationFromScreen = dataGrid.PointToScreen(locationFromWindow);

                var left = locationFromScreen.X + dataGrid.ActualWidth - columnChooserWindow.Width;
                var top = locationFromScreen.Y + dataGrid.ActualHeight - columnChooserWindow.Height;

                var screenWidth = SystemParameters.WorkArea.Width;
                var screenHeight = SystemParameters.WorkArea.Height;

                columnChooserWindow.Left = Math.Min(left, screenWidth - columnChooserWindow.Width);
                columnChooserWindow.Top = Math.Min(top, screenHeight - columnChooserWindow.Height);
            };

            columnChooserWindow.Show();

            columnChooserWindow.Closed += (sender, e) =>
            {
                SetColumnChooserWindow(dataGrid, null);
            };
        }
    }

    /// <summary>
    /// Determines whether the OpenColumnChooser command can execute.
    /// </summary>
    /// <param name="sender">The <see cref="DataGrid"/> that the command is executed on.</param>
    /// <param name="e">The <see cref="CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
    private static void OpenColumnChooserCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        var dataGrid = (DataGrid)sender;
        e.CanExecute = GetAllowColumnChooser(dataGrid);
    }

    #endregion

    #region Events Subscriptions

    /// <summary>
    /// Handles the <see cref="UIElement.PreviewKeyDown"/> event for the <see cref="DataGrid"/>.
    /// Suppresses the default copy action (Ctrl + C) if the <see cref="AllowUserToCopyProperty"/> is set to <c>false</c>.
    /// </summary>
    /// <param name="sender">The <see cref="DataGrid"/> that raised the event.</param>
    /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
    private static void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            e.Handled = true;
        }
    }

    /// <summary>
    /// Handles the LoadingRow event to update row numbers.
    /// </summary>
    private static void DataGrid_LoadingRow(object? sender, DataGridRowEventArgs args)
    {
        if (sender is not DataGrid dataGrid)
        {
            return;
        }

        UpdateRowNumbers(dataGrid, true);
    }

    /// <summary>
    /// Handles the UnloadingRow event to update row numbers.
    /// </summary>
    private static void DataGrid_UnloadingRow(object? sender, DataGridRowEventArgs args)
    {
        if (sender is not DataGrid dataGrid)
        {
            return;
        }

        UpdateRowNumbers(dataGrid, false);
    }

    #endregion
}