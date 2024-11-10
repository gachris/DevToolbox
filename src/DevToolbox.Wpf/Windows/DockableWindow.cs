using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevToolbox.Wpf.Controls;
using DevToolbox.Wpf.Interop;

namespace DevToolbox.Wpf.Windows;

public class DockableWindow : DockManagerWindow
{
    #region Fields/Consts

    #endregion

    #region Properties

    [Bindable(true)]
    protected internal DockableControl? HostControl => Content as DockableControl;

    #endregion

    static DockableWindow() => DefaultStyleKeyProperty.OverrideMetadata(typeof(DockableWindow), new FrameworkPropertyMetadata(typeof(DockableWindow)));

    #region Overrides

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        if (oldContent is not null)
            throw new InvalidOperationException($"Cannot change {nameof(Content)} of {typeof(DockableWindow)}");
        if (newContent is not DockableControl content)
            throw new InvalidOperationException($"{nameof(Content)} of {typeof(DockableWindow)} must be {typeof(DockableControl)} type");

        content.StateChanged += OnStateChanged;

        base.OnContentChanged(oldContent, newContent);

        CommandManager.InvalidateRequerySuggested();
    }

    protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
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

                    handled = HostControl?.DockManager?.Drag(this, new Point(x, y), new Point(x - Left, y - Top)) ?? false;
                }
                break;
            case (int)WM.NCRBUTTONUP:
                if (wParam.ToInt32() == (int)HT.CAPTION)
                {
                    NativeMethods.RaiseMouseMessage(hwnd, WM.RBUTTONUP);
                    handled = true;
                }
                break;
        }

        return IntPtr.Zero;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        if (e.Cancel) HostControl?.SaveWindowSizeAndPosition(this);
    }

    #endregion

    #region Methods

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

    private void OnStateChanged(object? sender, StateChangedEventArgs e)
    {
        if (e.NewValue != State.Window)
        {
            Close();
        }
    }

    #endregion
}