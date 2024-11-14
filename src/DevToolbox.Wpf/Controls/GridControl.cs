using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;
using DevToolbox.Wpf.Data;
using DevToolbox.Wpf.Extensions;

namespace DevToolbox.Wpf.Controls;

/// <summary>
/// GridControl is a custom DataGrid control that provides additional features such as grouping, sorting, frozen rows, summaries, and saving/restoring column layout.
/// </summary>
public class GridControl : DataGrid
{
    #region Fields/Members

    /// <summary>
    /// List of columns that are currently used for grouping in the grid.
    /// </summary>
    private readonly List<DataGridColumn> _columnsInGroupBy = [];

    /// <summary>
    /// Collection that stores data for each DataGrid column, including layout and sorting information.
    /// </summary>
    private ObservableCollection<DataGridColumnData> _columnDataCollection;

    /// <summary>
    /// Indicates if there is an issue with the current frozen row count.
    /// </summary>
    private bool _problemWithFrozenRowCount;

    /// <summary>
    /// Indicates if a column width change operation is currently in progress.
    /// </summary>
    private bool _inWidthChange;

    /// <summary>
    /// Indicates if column information is currently being updated.
    /// </summary>
    private bool _updatingColumnInfo;

    /// <summary>
    /// Stores the last known frozen row count.
    /// </summary>
    private int? _lastFrozenRowCount;

    /// <summary>
    /// Reference to the scroll viewer of the DataGrid.
    /// </summary>
    private ScrollViewer? _scrollViewer;

    /// <summary>
    /// Reference to the summary table of the DataGrid.
    /// </summary>
    private DataGridSummaryTable? _summariesTable;

    /// <summary>
    /// Command for adding a column to the group by collection.
    /// </summary>
    private static readonly RoutedUICommand _addGroupByColumn = new RoutedUICommand(nameof(AddGroupByColumnCommand), nameof(AddGroupByColumnCommand), typeof(GridControl));

    /// <summary>
    /// Command for clearing all groupings.
    /// </summary>
    private static readonly RoutedUICommand _clearGroupCommand = new RoutedUICommand(nameof(ClearGroupCommand), nameof(ClearGroupCommand), typeof(GridControl));

    /// <summary>
    /// Command for removing a column from the group by collection.
    /// </summary>
    private static readonly RoutedUICommand _removeGroupByColumnCommand = new RoutedUICommand(nameof(RemoveGroupByColumnCommand), nameof(RemoveGroupByColumnCommand), typeof(GridControl));

    /// <summary>
    /// Command for sorting by group columns.
    /// </summary>
    private static readonly RoutedUICommand _sortByGroupColumnCommand = new RoutedUICommand(nameof(SortByGroupColumnCommand), nameof(SortByGroupColumnCommand), typeof(GridControl));

    /// <summary>
    /// DependencyPropertyKey for managing the vertical offset of non-frozen rows.
    /// </summary>
    private static readonly DependencyPropertyKey NonFrozenRowsViewportVerticalOffsetPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(NonFrozenRowsViewportVerticalOffset), typeof(double), typeof(GridControl), new FrameworkPropertyMetadata(0.0));

    /// <summary>
    /// DependencyProperty to get the vertical offset of non-frozen rows.
    /// </summary>
    public static readonly DependencyProperty NonFrozenRowsViewportVerticalOffsetProperty = NonFrozenRowsViewportVerticalOffsetPropertyKey.DependencyProperty;

    /// <summary>
    /// DependencyProperty for the number of frozen rows in the DataGrid.
    /// </summary>
    public static readonly DependencyProperty FrozenRowCountProperty =
        DependencyProperty.Register(nameof(FrozenRowCount), typeof(int), typeof(GridControl), new FrameworkPropertyMetadata(0, OnFrozenRowCountPropertyChanged), ValidateFrozenRowCount);

    /// <summary>
    /// DependencyProperty to manage if only two-way sorting should be allowed.
    /// </summary>
    public static readonly DependencyProperty OnlyTwoWaySortingProperty =
        DependencyProperty.Register(nameof(OnlyTwoWaySorting), typeof(bool), typeof(GridControl), new FrameworkPropertyMetadata(false));

    /// <summary>
    /// DependencyProperty to control the visibility of the group panel in the DataGrid.
    /// </summary>
    public static readonly DependencyProperty ShowGroupPanelProperty =
        DependencyProperty.Register(nameof(ShowGroupPanel), typeof(bool), typeof(GridControl), new FrameworkPropertyMetadata(true));

    /// <summary>
    /// DependencyProperty to control whether row summaries are displayed.
    /// </summary>
    public static readonly DependencyProperty ShowSummariesProperty =
        DependencyProperty.Register(nameof(ShowSummaries), typeof(bool), typeof(GridControl), new FrameworkPropertyMetadata(true));

    /// <summary>
    /// DependencyProperty to set the template for the footer in the DataGrid.
    /// </summary>
    public static readonly DependencyProperty FooterDataTemplateProperty =
        DependencyProperty.Register(nameof(FooterDataTemplate), typeof(DataTemplate), typeof(GridControl), new FrameworkPropertyMetadata(default));

    /// <summary>
    /// Event that is fired when a column is added to the group by collection.
    /// </summary>
    public event EventHandler? GroupByColumnAdded;

    /// <summary>
    /// Event that is fired when a column is removed from the group by collection.
    /// </summary>
    public event EventHandler? GroupByColumnRemoved;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the list of columns currently grouped by.
    /// </summary>
    internal List<DataGridColumn> ColumnsInGroupBy => _columnsInGroupBy;

    /// <summary>
    /// Gets the scroll viewer used in the DataGrid.
    /// </summary>
    internal ScrollViewer? ScrollViewer => _scrollViewer;

    /// <summary>
    /// Command to add a column to group by.
    /// </summary>
    public static RoutedUICommand AddGroupByColumnCommand => _addGroupByColumn;

    /// <summary>
    /// Command to clear all groups.
    /// </summary>
    public static RoutedUICommand ClearGroupCommand => _clearGroupCommand;

    /// <summary>
    /// Command to remove a column from group by.
    /// </summary>
    public static RoutedUICommand RemoveGroupByColumnCommand => _removeGroupByColumnCommand;

    /// <summary>
    /// Command to sort a grouped column.
    /// </summary>
    public static RoutedUICommand SortByGroupColumnCommand => _sortByGroupColumnCommand;

    /// <summary>
    /// Collection representing total summaries for the grid.
    /// </summary>
    public ObservableCollection<DataGridSummaryItem> TotalSummary { get; }

    /// <summary>
    /// Collection representing columns currently grouped by.
    /// </summary>
    public ObservableCollection<DataGirdColumnGroup> GroupByCollection { get; }

    /// <summary>
    /// Gets or sets the start y coordinate of non-frozen rows in the viewport.
    /// </summary>
    public double NonFrozenRowsViewportVerticalOffset
    {
        get => (double)GetValue(NonFrozenRowsViewportVerticalOffsetProperty);
        internal set => SetValue(NonFrozenRowsViewportVerticalOffsetPropertyKey, value);
    }

    /// <summary>
    /// Property which determines the number of rows that are frozen from the beginning in order of display.
    /// </summary>
    public int? FrozenRowCount
    {
        get => (int)GetValue(FrozenRowCountProperty);
        set
        {
            if ((EnableColumnVirtualization || EnableRowVirtualization) && value != null)
            {
                if (_problemWithFrozenRowCount)
                    return;
                _problemWithFrozenRowCount = true;
                throw new Exception("Both EnableColumnVirtualization and EnableRowVirtualization should be set to False to use frozen rows.");
            }

            var cvs = CollectionViewSource.GetDefaultView(ItemsSource);
            if (cvs.GroupDescriptions.Count > 0)
            {
                if (_problemWithFrozenRowCount)
                    return;
                _problemWithFrozenRowCount = true;
                throw new Exception("FrozenRowsCount cannot be set if grouping is applied.");
            }

            SetValue(FrozenRowCountProperty, value);
        }
    }

    /// <summary>
    /// If set to true, will remove 3-state sorting and revert to normal two-state sorting.
    /// </summary>
    public bool OnlyTwoWaySorting
    {
        get => (bool)GetValue(OnlyTwoWaySortingProperty);
        set => SetValue(OnlyTwoWaySortingProperty, value);
    }

    /// <summary>
    /// Controls the visibility of the Group Panel.
    /// </summary>
    public bool ShowGroupPanel
    {
        get => (bool)GetValue(ShowGroupPanelProperty);
        set => SetValue(ShowGroupPanelProperty, value);
    }

    /// <summary>
    /// Controls whether row summaries are displayed.
    /// </summary>
    public bool ShowSummaries
    {
        get => (bool)GetValue(ShowSummariesProperty);
        set => SetValue(ShowSummariesProperty, value);
    }

    /// <summary>
    /// Gets or sets the template for the footer.
    /// </summary>
    public DataTemplate FooterDataTemplate
    {
        get => (DataTemplate)GetValue(FooterDataTemplateProperty);
        set => SetValue(FooterDataTemplateProperty, value);
    }

    #endregion

    static GridControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(GridControl), new FrameworkPropertyMetadata(typeof(GridControl)));
        ItemsPanelProperty.OverrideMetadata(typeof(GridControl), new FrameworkPropertyMetadata(new ItemsPanelTemplate(new FrameworkElementFactory(typeof(GridControlRowsPresenter)))));

        CommandManager.RegisterClassCommandBinding(typeof(GridControl), new CommandBinding(AddGroupByColumnCommand, OnAddGroupByColumnExecuted, OnAddGroupByColumnCanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(GridControl), new CommandBinding(ClearGroupCommand, OnClearGroupExecuted, OnClearGroupCanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(GridControl), new CommandBinding(RemoveGroupByColumnCommand, RemoveGroupByColumnExecuted, RemoveGroupByColumnCanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(GridControl), new CommandBinding(SortByGroupColumnCommand, SortByGroupColumnExecuted, SortByGroupColumnCanExecute));
    }

    /// <summary>
    /// Constructor for initializing collections, attaching events, and setting up filtering.
    /// </summary>
    public GridControl()
    {
        TotalSummary = [];
        GroupByCollection = [];
        _columnDataCollection = [];

        Loaded += GridControl_Loaded;
        Sorting += GridControl_Sorting;

        AutoFilter.SetAutoFilter(this, new AutoFilter(this));
    }

    #region Methods Override

    /// <summary>
    /// Applies the template for the GridControl.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _scrollViewer = Template.FindName("DG_ScrollViewer", this) as ScrollViewer;
        _summariesTable = Template.FindName("PART_DataGridSummaryTable", this) as DataGridSummaryTable;
    }

    /// <summary>
    /// Handles the event when the edit is committed.
    /// </summary>
    protected override void OnExecutedCommitEdit(ExecutedRoutedEventArgs e)
    {
        base.OnExecutedCommitEdit(e);

        if (e.OriginalSource == null)
            return;

        _summariesTable?.CalculateSummaryValues();
    }

    /// <summary>
    /// Handles the event when a column is reordered.
    /// </summary>
    protected override void OnColumnReordered(DataGridColumnEventArgs e)
    {
        RefreshColumnDataCollection();

        base.OnColumnReordered(e);
    }

    /// <summary>
    /// Initializes the GridControl and attaches event handlers for sorting and column changes.
    /// </summary>
    protected override void OnInitialized(EventArgs e)
    {
        void sortDirectionChangedHandler(object? sender, EventArgs x) => RefreshColumnDataCollection();
        void widthPropertyChangedHandler(object? sender, EventArgs x) => _inWidthChange = true;

        var sortDirectionPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.SortDirectionProperty, typeof(DataGridColumn));
        var widthPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));

        Loaded += (sender, x) =>
        {
            foreach (var column in Columns)
            {
                sortDirectionPropertyDescriptor.AddValueChanged(column, sortDirectionChangedHandler);
                widthPropertyDescriptor.AddValueChanged(column, widthPropertyChangedHandler);
            }
        };

        Unloaded += (sender, x) =>
        {
            foreach (var column in Columns)
            {
                sortDirectionPropertyDescriptor.RemoveValueChanged(column, sortDirectionChangedHandler);
                widthPropertyDescriptor.RemoveValueChanged(column, widthPropertyChangedHandler);
            }
        };

        base.OnInitialized(e);
    }

    /// <summary>
    /// Handles changes to the ItemsSource property and updates the summaries and layout accordingly.
    /// </summary>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        base.OnItemsSourceChanged(oldValue, newValue);

        CommitEdit();

        _summariesTable?.CalculateSummaryValues();
        _scrollViewer?.ScrollToLeftEnd();

        InvalidateArrange();
    }

    /// <summary>
    /// Handles the event when the mouse button is released after a change in column width.
    /// </summary>
    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (_inWidthChange)
        {
            _inWidthChange = false;
            RefreshColumnDataCollection();
        }

        base.OnPreviewMouseLeftButtonUp(e);
    }

    #endregion

    #region Methods

    private static void OnFrozenRowCountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var gridControl = (GridControl)d;
        gridControl.OnFrozenRowCountPropertyChanged((int)e.OldValue, (int)e.NewValue);
    }

    private static bool ValidateFrozenRowCount(object value)
    {
        var frozenCount = (int)value;
        return frozenCount >= 0;
    }

    private void OnFrozenRowCountPropertyChanged(int oldValue, int newValue)
    {
        RefreshDataGridRowsPresenter();
    }

    /// <summary>
    /// Updates the DataGrid rows presenter for frozen row updates.
    /// </summary>
    private void RefreshDataGridRowsPresenter()
    {
        var panel = this.FindVisualChild<GridControlRowsPresenter>();

        if (panel is null)
        {
            var ownerType = typeof(GridControl);
            var propertyMetadata = ItemsPanelProperty.GetMetadata(ownerType);

            if (((FrameworkTemplate)propertyMetadata.DefaultValue).VisualTree.Type != typeof(GridControlRowsPresenter))
            {
                ItemsPanelProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(new ItemsPanelTemplate(new FrameworkElementFactory(typeof(GridControlRowsPresenter)))));
            }

            panel = this.FindVisualChild<GridControlRowsPresenter>();
        }

        UpdateLayout();

        panel?.InvalidateArrange();
    }

    /// <summary>
    /// Updates column data collection to reflect changes made by the user.
    /// </summary>
    private void RefreshColumnDataCollection()
    {
        if (_updatingColumnInfo)
        {
            return;
        }

        _updatingColumnInfo = true;

        _columnDataCollection = new ObservableCollection<DataGridColumnData>(Columns.Select(x => new DataGridColumnData(x)));

        Items.SortDescriptions.Clear();

        foreach (var column in _columnDataCollection)
        {
            var realColumn = Columns.FirstOrDefault(x => column.SortMemberPath.Equals(x.SortMemberPath));
            if (realColumn == null)
            {
                continue;
            }
            column.Apply(realColumn, Columns.Count, Items.SortDescriptions);
        }

        _updatingColumnInfo = false;
    }

    /// <summary>
    /// Handles sorting for the specified DataGridColumn.
    /// </summary>
    /// <param name="dataGridColumn">The column to sort.</param>
    private bool Sort(DataGridColumn dataGridColumn)
    {
        if (OnlyTwoWaySorting)
        {
            return false;
        }

        var sortPropertyName = dataGridColumn.SortMemberPath ?? string.Empty;

        if (dataGridColumn is DataGridBoundColumn boundColumn)
        {
            sortPropertyName = (boundColumn.Binding as Binding)?.Path?.Path ?? string.Empty;
        }

        if (string.IsNullOrEmpty(sortPropertyName))
        {
            return false;
        }

        if (dataGridColumn.SortDirection == ListSortDirection.Descending)
        {
            if (!RemoveSortDescription(sortPropertyName))
            {
                return false;
            }

            dataGridColumn.SortDirection = null;

            if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
            {
                Items.SortDescriptions.Clear();
                Items.Refresh();
            }

            return true;
        }

        return false;
    }

    private bool RemoveSortDescription(string fieldName)
    {
        var index = -1;
        var i = 0;

        foreach (var sortDesc in Items.SortDescriptions)
        {
            if (string.CompareOrdinal(sortDesc.PropertyName, fieldName) == 0)
            {
                index = i;
                break;
            }

            i++;
        }

        if (index != -1)
        {
            Items.SortDescriptions.RemoveAt(index);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void RemoveGroupByColumn(DataGridColumnHeader dataGridColumnHeader)
    {
        var groupByData = dataGridColumnHeader?.DataContext as DataGirdColumnGroup;
        var fieldName = groupByData?.SortMemberPath;

        var column = ColumnsInGroupBy.FirstOrDefault(gc => gc.SortMemberPath == fieldName);
        if (groupByData == null || column == null)
        {
            return;
        }

        GroupByCollection.Remove(groupByData);
        ColumnsInGroupBy.Remove(column);
        //Columns.Add(column);
        var enableColumnVirtualization = EnableColumnVirtualization;
        EnableColumnVirtualization = false;
        column.Visibility = Visibility.Visible;
        EnableColumnVirtualization = enableColumnVirtualization;

        if (ColumnsInGroupBy.Count == 0)
        {
            if (_lastFrozenRowCount is not null and not 0)
                FrozenRowCount = _lastFrozenRowCount;
        }

        if (Items != null)
        {
            var cvs = CollectionViewSource.GetDefaultView(Items);
            if (cvs != null)
            {
                var propertyGroupDescription = cvs.GroupDescriptions.Where(gd => ((PropertyGroupDescription)(gd)).PropertyName == fieldName).Cast<PropertyGroupDescription>().FirstOrDefault();
                if (propertyGroupDescription != null)
                    cvs.GroupDescriptions.Remove(propertyGroupDescription);
                cvs.Refresh();
            }
        }

        GroupByColumnRemoved?.Invoke(this, new DataGridColumnGroupByEventArgs(column));
    }

    private void AddGroupByColumn(DataGridColumnHeader dataGridColumnHeader)
    {
        if (dataGridColumnHeader.Column == null)
            return;

        var dataGridColumn = dataGridColumnHeader.Column;
        var header = dataGridColumn.Header;
        if (GroupByCollection.Count(gc => Equals(gc.SortMemberPath, header)) == 0)
        {
            var groupByData = new DataGirdColumnGroup
            {
                ColumnName = (string)header,
                Index = GroupByCollection.Count,
                SortDirection = dataGridColumn.SortDirection,
                SortMemberPath = dataGridColumn.SortMemberPath
            };

            if (GroupByCollection.Count == 0)
            {
                if (FrozenRowCount is not null and not 0)
                {
                    _lastFrozenRowCount = FrozenRowCount;
                    FrozenRowCount = 0;
                }
            }

            GroupByCollection.Add(groupByData);
            ColumnsInGroupBy.Add(dataGridColumn);
            //Columns.Remove(column);
            var enableColumnVirtualization = EnableColumnVirtualization;
            EnableColumnVirtualization = false;
            dataGridColumn.Visibility = Visibility.Hidden;
            EnableColumnVirtualization = enableColumnVirtualization;

            try
            {
                if (Items != null)
                {
                    var cvs = CollectionViewSource.GetDefaultView(Items);
                    if (cvs != null)
                    {
                        cvs.GroupDescriptions.Add(new PropertyGroupDescription(dataGridColumn.SortMemberPath));
                        Items.Refresh();
                    }
                }
                GroupByColumnAdded?.Invoke(this, new DataGridColumnGroupByEventArgs(dataGridColumn));
            }
            catch (Exception)
            {
                GroupByCollection.Remove(groupByData);
            }
        }
    }

    private void ClearGroup()
    {
        if (Items == null)
            return;

        var collectionView = CollectionViewSource.GetDefaultView(Items);

        if (collectionView == null)
            return;

        collectionView.GroupDescriptions.Clear();
        GroupByCollection.Clear();
        var columns = ColumnsInGroupBy.ToList();
        ColumnsInGroupBy.Clear();
        foreach (var column in Columns)
        {
            var enableColumnVirtualization = EnableColumnVirtualization;
            EnableColumnVirtualization = false;
            column.Visibility = Visibility.Visible;
            EnableColumnVirtualization = enableColumnVirtualization;
        }
        columns.Clear();
        collectionView.Refresh();
    }

    /// <summary>
    /// Get the column information in xml (string) basically used to save data
    /// </summary>
    /// <returns>xml representation of column information in string</returns>
    public string SerializeLayout()
    {
        RefreshColumnDataCollection();

        var xs = new XmlSerializer(_columnDataCollection.GetType());
        using var stringWriter = new StringWriter();
        xs.Serialize(stringWriter, _columnDataCollection);
        return stringWriter.ToString();
    }

    /// <summary>
    /// This is used to apply the column information last saved
    /// </summary>
    /// <param name="xml">xml representation of column information</param>
    /// <returns></returns>
    public bool DeserializeLayout(string xml)
    {
        try
        {
            var ser = new XmlSerializer(typeof(ObservableCollection<DataGridColumnData>));
            using var tr = new StringReader(xml);
            _columnDataCollection = (ObservableCollection<DataGridColumnData>?)ser.Deserialize(tr) ?? [];
        }
        catch
        {
            return false;
        }

        return true;
    }

    #endregion

    #region Commands

    private static void SortByGroupColumnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var gridControl = (GridControl)sender;
        var dataGridColumnHeader = (DataGridColumnHeader)e.OriginalSource;
        var sortMemberPath = ((DataGirdColumnGroup)dataGridColumnHeader.DataContext).SortMemberPath;

        gridControl.Sort(gridControl.Columns.First(c => c.SortMemberPath == sortMemberPath));
    }

    private static void SortByGroupColumnCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is GridControl && e.OriginalSource is DataGridColumnHeader;
    }

    private static void OnAddGroupByColumnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var gridControl = (GridControl)sender;
        var dataGridColumnHeader = (DataGridColumnHeader)e.OriginalSource;

        gridControl.AddGroupByColumn(dataGridColumnHeader);
    }

    private static void OnAddGroupByColumnCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is not GridControl gridControl)
        {
            e.CanExecute = false;
            return;
        }

        e.CanExecute = e.OriginalSource is DataGridColumnHeader dataGridColumnHeader
                       && dataGridColumnHeader.Column != null
                       && gridControl.Columns.Count(x => x.Visibility == Visibility.Visible) > 1;
    }

    private static void RemoveGroupByColumnExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var gridControl = (GridControl)sender;
        var dataGridColumnHeader = (DataGridColumnHeader)e.OriginalSource;

        gridControl.RemoveGroupByColumn(dataGridColumnHeader);
    }

    private static void RemoveGroupByColumnCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is GridControl
                       && e.OriginalSource is DataGridColumnHeader dataGridColumnHeader
                       && dataGridColumnHeader.DataContext is DataGirdColumnGroup;
    }

    private static void OnClearGroupExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var gridControl = (GridControl)sender;

        gridControl.ClearGroup();
    }

    private static void OnClearGroupCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is GridControl;
    }

    #endregion

    #region Events Subscriptions

    private void GridControl_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshDataGridRowsPresenter();
    }

    private void GridControl_Sorting(object sender, DataGridSortingEventArgs e)
    {
        if (Sort(e.Column))
        {
            e.Handled = true;
        }
    }

    #endregion
}
