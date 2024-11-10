using System;
using System.Windows;
using System.Windows.Controls;
using DevToolbox.Wpf.Controls;
using DevToolbox.Wpf.Interop;

namespace DevToolbox.Wpf.Windows;

public class DocumentWindow : DockManagerWindow
{
    #region Properties

    protected internal DocumentControl? HostControl => Content as DocumentControl;

    #endregion

    static DocumentWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DocumentWindow), new FrameworkPropertyMetadata(typeof(DocumentWindow)));
    }

    #region Overrides

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        if (oldContent is not null)
            throw new InvalidOperationException($"Cannot change {nameof(Content)} of {typeof(DockableWindow)}");
        if (newContent is not DocumentControl content)
            throw new InvalidOperationException($"{nameof(Content)} of {typeof(DocumentWindow)} must be {typeof(DocumentControl)} type");

        content.StateChanged += OnStateChanged;

        base.OnContentChanged(oldContent, newContent);
    }

    protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        base.WndProc(hwnd, msg, wParam, lParam, ref handled);

        switch (msg)
        {
            case (int)WM.SIZE:
            case (int)WM.MOVE:
                //HostControl.SaveWindowSizeAndPosition(this);
                break;
            case (int)WM.NCLBUTTONDOWN:
                if (wParam.ToInt32() == (int)HT.CAPTION)
                {
                    var x = NativeMethods.GET_X_LPARAM(lParam);
                    var y = NativeMethods.GET_Y_LPARAM(lParam);

                    handled = HostControl?.DockManager?.Drag(this, new Point(x, y), new Point(x - Left, y - Top)) ?? false;
                }
                break;
        }

        return IntPtr.Zero;
    }

    protected internal override void OnDrop(IDropSurface control, DockingPosition btnDock)
    {
        if (HostControl is null)
        {
            return;
        }

        if (btnDock == DockingPosition.PaneInto && control is DocumentControl documentControl)
        {
            HostControl.MoveInto(documentControl);
            return;
        }
        else if (control is DockManager)
        {
            HostControl.MoveInto();
            return;
        }

        var dock = btnDock == DockingPosition.PaneBottom
            ? Dock.Bottom
            : btnDock == DockingPosition.PaneLeft
            ? Dock.Left
            : btnDock == DockingPosition.PaneRight
            ? Dock.Right
            : btnDock == DockingPosition.PaneTop
            ? Dock.Top
            : throw new InvalidOperationException($"{btnDock} DockingPosition not supported for {typeof(DocumentControl)}");

        HostControl.MoveTo((DocumentControl)control, dock);
        HostControl.State = State.Document;
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