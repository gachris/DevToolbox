using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System;
using System.Linq;
using System.Threading.Tasks;
using DevToolbox.Core.Contracts;
using DevToolbox.WinUI.Contracts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using NavigationEventArgs = Microsoft.UI.Xaml.Navigation.NavigationEventArgs;

namespace DevToolbox.WinUI.Services;

/// <summary>
/// Provides an implementation of <see cref="INavigationService"/> for navigating between pages in a WPF application.
/// </summary>
public class NavigationService : INavigationService
{
    #region Fields/Consts

    private readonly IPageService _pageService;
    private readonly IAppWindowService _appWindowService;
    private readonly List<FrameworkElement> _lastViews = [];
    private Frame? _frame;

    /// <summary>
    /// Occurs when navigation to a new content fragment is completed.
    /// </summary>
    public event EventHandler<Core.Contracts.NavigationEventArgs>? Navigated;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the <see cref="Frame"/> used for navigation.
    /// </summary>
    public object? Frame
    {
        get
        {
            if (_frame == null)
            {
                _frame = _appWindowService.MainWindow.Content as Frame;
                RegisterFrameEvents();
            }

            return _frame;
        }
        set
        {
            UnregisterFrameEvents();
            _frame = value as Frame;
            RegisterFrameEvents();
        }
    }

    /// <summary>
    /// Gets a value indicating whether the navigation service can navigate back.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Frame), nameof(_frame))]
    public bool CanGoBack => _frame?.CanGoBack == true;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationService"/> class.
    /// </summary>
    /// <param name="pageService">Service that resolves page types from keys.</param>
    /// <param name="appWindowService">AppWindow Service for resolving window instances.</param>
    public NavigationService(IPageService pageService, IAppWindowService appWindowService)
    {
        _pageService = pageService;
        _appWindowService = appWindowService;
    }

    #region Methods

    /// <summary>
    /// Registers navigation event handlers for the current <see cref="Frame"/>.
    /// </summary>
    private void RegisterFrameEvents()
    {
        if (_frame != null)
        {
            _frame.Navigating += Frame_Navigating;
            _frame.Navigated += Frame_Navigated;
        }
    }

    /// <summary>
    /// Unregisters navigation event handlers from the current <see cref="Frame"/>.
    /// </summary>
    private void UnregisterFrameEvents()
    {
        if (_frame != null)
        {
            _frame.Navigating -= Frame_Navigating;
            _frame.Navigated -= Frame_Navigated;
        }
    }

    /// <summary>
    /// Adds a back entry to the navigation stack using a page key.
    /// </summary>
    /// <param name="pageKey">The key identifying the page.</param>
    /// <returns>True if the back entry was added; otherwise, false.</returns>
    public async Task<bool> AddBackEntryAsync(string pageKey)
    {
        if (_frame is null)
            return await Task.FromResult(false);

        var pageType = _pageService.GetPageType(pageKey);

        var entry = new PageStackEntry(pageType, null, null);

        _frame.BackStack.Add(entry);
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Navigates to the previous page in the navigation stack.
    /// </summary>
    /// <returns>True if navigation was successful; otherwise, false.</returns>
    public async Task<bool> GoBackAsync()
    {
        if (!CanGoBack)
            return await Task.FromResult(false);

        _frame.GoBack();

        return await Task.FromResult(true);
    }

    /// <summary>
    /// Navigates to the specified page using a page key, but skips navigation
    /// if the frame is already displaying that page type.
    /// </summary>
    /// <param name="pageKey">The key identifying the page to navigate to.</param>
    /// <param name="parameter">An optional parameter to pass to the page.</param>
    /// <param name="clearNavigation">Indicates whether the navigation history should be cleared.</param>
    /// <returns>True if navigation was performed (or you were already on that page); false if unable to navigate.</returns>
    public async Task<bool> NavigateToAsync(string pageKey, object? parameter = null, bool clearNavigation = false)
    {
        var pageType = _pageService.GetPageType(pageKey);

        if (_frame != null && _frame.Content?.GetType() != pageType)
        {
            _frame.Tag = clearNavigation;
            return await Task.FromResult(_frame.Navigate(pageType, parameter));
        }

        return await Task.FromResult(true);
    }

    #endregion

    #region Events Subscriptions

    /// <summary>
    /// Handles the <see cref="Frame.Navigating"/> event to notify view models of navigation away.
    /// </summary>
    private void Frame_Navigating(object sender, NavigatingCancelEventArgs e)
    {
        var lastView = _lastViews.LastOrDefault();
        if (lastView?.DataContext is INavigationViewModelAware vm)
        {
            vm.OnNavigatedAway();
        }

        if (lastView is not null)
        {
            _lastViews.Remove(lastView);
        }
    }

    /// <summary>
    /// Handles the <see cref="Frame.Navigated"/> event to notify view models of navigation and update view tracking.
    /// </summary>
    private void Frame_Navigated(object sender, NavigationEventArgs e)
    {
        if (_frame is null)
            return;

        var clearNavigation = (bool)_frame.Tag;
        if (clearNavigation)
        {
            _frame.BackStack.Clear();
        }

        if (e.Content is FrameworkElement fe)
        {
            if (fe.DataContext is INavigationViewModelAware vm)
            {
                vm.OnNavigated();
            }

            _lastViews.Add(fe);
        }

        Navigated?.Invoke(sender, new Core.Contracts.NavigationEventArgs(e.Content));
    }

    #endregion
}