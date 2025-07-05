using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevToolbox.Wpf.Controls;
using DevToolbox.Wpf.Interop;

namespace DevToolbox.Wpf.Windows;

/// <summary>
/// A floating window that hosts a single <see cref="DockableControl"/>.
/// Handles native window messages for resizing, moving, and dragging,
/// and delegates drop operations back to the host control.
/// </summary>
public class DockableWindow : DockManagerWindow
{
    #region Properties

    /// <summary>
    /// Gets the contained <see cref="DockableControl"/> instance, or null if none.
    /// </summary>
    [Bindable(true)]
    protected internal DockableControl? HostControl => Content as DockableControl;

    #endregion

    static DockableWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DockableWindow),
            new FrameworkPropertyMetadata(typeof(DockableWindow)));

        WidthProperty.OverrideMetadata(typeof(DockableWindow), new FrameworkPropertyMetadata(300.0));
        HeightProperty.OverrideMetadata(typeof(DockableWindow), new FrameworkPropertyMetadata(300.0));

        ShowInTaskbarProperty.OverrideMetadata(
            typeof(DockableWindow),
            new FrameworkPropertyMetadata(false));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockableWindow"/> class,
    /// setting up default window chrome and behaviors for dockable windows.
    /// </summary>
    public DockableWindow()
    {
        Chrome.CaptionHeight = 22;
    }

    #region Methods Overrides

    /// <inheritdoc/>
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        if (oldContent != null)
            throw new InvalidOperationException($"Cannot change Content of {nameof(DockableWindow)}");

        if (newContent is not DockableControl content)
            throw new InvalidOperationException($"Content of {nameof(DockableWindow)} must be {nameof(DockableControl)}");

        // Listen for when the state exits Window so we can close
        content.StateChanged += OnStateChanged;

        base.OnContentChanged(oldContent, newContent);

        // Ensure command bindings update for new content
        CommandManager.InvalidateRequerySuggested();
    }

    /// <inheritdoc/>
    protected override IntPtr WndProc(
        IntPtr hwnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled)
    {
        // Let base handle SCROLL/MOVE hooks
        base.WndProc(hwnd, msg, wParam, lParam, ref handled);

        switch (msg)
        {
            case (int)WM.SIZE:
            case (int)WM.MOVE:
                HostControl?.SaveWindowSizeAndPosition(this);
                break;
            case (int)WM.NCLBUTTONDOWN:
                if (wParam.ToInt32() == (int)HT.CAPTION)
                {
                    int x = NativeMethods.GET_X_LPARAM(lParam);
                    int y = NativeMethods.GET_Y_LPARAM(lParam);
                    handled = HostControl?.DockManager?.Drag(
                        this,
                        new Point(x, y),
                        new Point(x - Left, y - Top)) ?? false;
                }
                break;
        }

        return IntPtr.Zero;
    }

    /// <inheritdoc/>
    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        // If the window is being canceled, save its last size/pos
        if (!e.Cancel)
            HostControl?.SaveWindowSizeAndPosition(this);
    }

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected internal override void OnDrop(IDropSurface control, DockingPosition btnDock)
    {
        if (HostControl is null) return;

        if (btnDock == DockingPosition.PaneInto)
        {
            if (control is DockableControl dockableControl)
            {
                HostControl.MoveInto(dockableControl);
            }
            else if (control is DocumentControl documentControl)
            {
                HostControl.MoveInto(documentControl);
            }
            return;
        }

        Dock dock = default;

        if (btnDock == DockingPosition.Bottom)
            HostControl.Dock = Dock.Bottom;
        else if (btnDock == DockingPosition.Left)
            HostControl.Dock = Dock.Left;
        else if (btnDock == DockingPosition.Right)
            HostControl.Dock = Dock.Right;
        else if (btnDock == DockingPosition.Top)
            HostControl.Dock = Dock.Top;
        else
        {
            if (btnDock == DockingPosition.PaneTop)
                dock = Dock.Top;
            else if (btnDock == DockingPosition.PaneBottom)
                dock = Dock.Bottom;
            else if (btnDock == DockingPosition.PaneLeft)
                dock = Dock.Left;
            else if (btnDock == DockingPosition.PaneRight)
                dock = Dock.Right;

            if (control is DockableControl dockableControl)
            {
                HostControl.MoveTo(dockableControl, dock);
            }
            else if (control is DocumentControl documentControl)
            {
                HostControl.MoveTo(documentControl, dock);
            }
        }

        HostControl.State = State.Docking;
    }

    /// <summary>
    /// Handles when the hosted control transitions out of Window state,
    /// closing this floating window.
    /// </summary>
    private void OnStateChanged(object? sender, StateChangedEventArgs e)
    {
        if (e.NewValue != State.Window)
            Close();
    }

    #endregion
}
