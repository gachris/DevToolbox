using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using DevToolbox.Wpf.Interop;

namespace DevToolbox.Wpf.Windows.Snap;

/// <summary>
/// Handles window snapping functionality for WPF windows. 
/// This class monitors the window state and detects when the window is snapped 
/// to the edges of the screen or other windows.
/// </summary>
public class WindowSnap : IDisposable
{
    private static readonly Size NoSize = new(0, 0);

    private readonly Func<bool> CanSnap;
    private readonly Window _window;
    private IntPtr _hWnd = IntPtr.Zero;
    private bool _snapped = false;
    private Size? _offset = null;

    /// <summary>
    /// Gets the current window state.
    /// </summary>
    private WindowState ActualState => GetActualState(_hWnd);

    /// <summary>
    /// Determines if the window is in the process of unsnapping.
    /// </summary>
    private bool Unsnapping => _snapped && SizeRestored(_window);

    /// <summary>
    /// Gets the offset for the snapping operation.
    /// </summary>
    private Size Offset => _offset != null ? (Size)_offset : NoSize;

    // Events        
    /// <summary>
    /// Occurs when the window has been snapped.
    /// </summary>
    public event EventHandler<SnapResultEventArgs>? Snapped;

    /// <summary>
    /// Occurs when the window has been unsnapped.
    /// </summary>
    public event EventHandler<SnapResultEventArgs>? Unsnapped;

    /// <summary>
    /// Occurs when the edge offset changes.
    /// </summary>
    public event EventHandler<EdgeOffsetChangedEventArgs>? EdgeOffsetChanged;

    /// <summary>
    /// Gets a value indicating whether the window is currently snapped.
    /// </summary>
    public bool IsSnapped => _snapped;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowSnap"/> class.
    /// </summary>
    /// <param name="window">The WPF window to monitor for snapping.</param>
    /// <exception cref="ArgumentNullException">Thrown when the window is null.</exception>
    public WindowSnap(Window window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window), "The window cannot be null.");
        _hWnd = new WindowInteropHelper(window).Handle;

        HwndSource.FromHwnd(_hWnd).AddHook(WindowProc);

        CanSnap = () => SnapSettings.WindowArranging &&
                       (SnapSettings.DockMoving || SnapSettings.SnapSizing);
    }

    /// <summary>
    /// Processes window messages to handle snapping logic.
    /// </summary>
    /// <param name="hWnd">The handle of the window.</param>
    /// <param name="msg">The message identifier.</param>
    /// <param name="wParam">Additional message-specific information.</param>
    /// <param name="lParam">Additional message-specific information.</param>
    /// <param name="handled">A value indicating whether the message was handled.</param>
    /// <returns>The result of the message processing.</returns>
    public virtual IntPtr WindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        var message = (WM)msg;
        switch (message)
        {
            case WM.ACTIVATE:
                {
                    // Initialize offset on window first activation    
                    // Note: this only applies for non-layered style window
                    if (!_window.AllowsTransparency &&
                         _window.WindowStyle != WindowStyle.None &&
                         _offset == null)
                    {
                        var offset = GetEdgeOffset();

                        _offset = offset.Value;

                        EdgeOffsetChanged?.Invoke(this, new EdgeOffsetChangedEventArgs(offset));
                    }

                    break;
                }
            case WM.WINDOWPOSCHANGING:
                {
                    var windowPosNullable = (WINDOWPOS?)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

                    if (windowPosNullable.HasValue && WindowChanged(windowPosNullable.Value))
                    {
                        var windowPos = windowPosNullable.Value;
                        switch (ActualState)
                        {
                            case WindowState.Minimized:
                                break;

                            case WindowState.Maximized:
                                Unsnap();
                                break;

                            default:
                                // Proceed to snap detection if the current system settings
                                // allow snapping or if the window is currently snapped
                                if (CanSnap() || _snapped)
                                {
                                    var rect = new RECT
                                    {
                                        Left = windowPos.x,
                                        Top = windowPos.y,
                                        Right = windowPos.x + windowPos.cx,
                                        Bottom = windowPos.y + windowPos.cy
                                    };

                                    var location = new Rect(Dpi.ToLogicalX(windowPos.x),
                                                             Dpi.ToLogicalY(windowPos.y),
                                                             Dpi.ToLogicalX(windowPos.cx),
                                                             Dpi.ToLogicalY(windowPos.cy));

                                    // Detecting snapping condition in the WM_WINDOWPOSCHANGING message event
                                    // may not be accurate if using the WINDOWPOS values: when the window is 
                                    // quickly being snapped, the dimension of the window may not always be 
                                    // specified (i.e. cx = cy = 0), in this case the SWP_NOSIZE flag is set
                                    // and we must obtain the window location explicitly instead.

                                    if ((windowPos.flags & SWP.NOSIZE) == SWP.NOSIZE)
                                    {
                                        User32.GetWindowRect(hWnd, ref rect);

                                        location = new Rect(Dpi.ToLogicalX(rect.Left),
                                                            Dpi.ToLogicalY(rect.Top),
                                                            Dpi.ToLogicalX(rect.Right - rect.Left),
                                                            Dpi.ToLogicalY(rect.Bottom - rect.Top));
                                    }

                                    // Get the list of monitors that intersect with the window area
                                    Screen monitors = Screen.FromRectangle(new System.Drawing.Rectangle(rect.X, rect.Y, rect.Width, rect.Height));

                                    SnapResult snapResult = DetectSnap.IsSnapped(ref location, new() { monitors }, Offset);

                                    if (snapResult.IsSnapped)
                                    {
                                        Snap(snapResult);
                                    }
                                    else if (Unsnapping)
                                    {
                                        Unsnap();
                                    }
                                }

                                break;
                        }
                    }

                    break;
                }
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Snaps the window based on the provided snapping result.
    /// </summary>
    /// <param name="snapResult">The result containing snap details.</param>
    private void Snap(SnapResult snapResult)
    {
        if (!_snapped)
        {
            _snapped = true;
            Snapped?.Invoke(this, new SnapResultEventArgs(snapResult.IsSnapped, snapResult.SnapBounds));
        }
    }

    /// <summary>
    /// Unsnaps the window if it is currently snapped.
    /// </summary>
    private void Unsnap()
    {
        if (_snapped)
        {
            _snapped = false;
            Unsnapped?.Invoke(this, new SnapResultEventArgs(false, SnapBounds.None));
        }
    }

    /// <summary>
    /// Obtains the edge vertical and horizontal offset based on the current type of window.
    /// </summary>
    /// <returns>The width and height offsets for the window's edges.</returns>
    private EdgeOffset GetEdgeOffset()
    {
        var edgeOffset = new EdgeOffset();
        var ws_ex = NativeMethods.GetWindowStyleEx(_hWnd);
        var ws = NativeMethods.GetWindowStyle(_hWnd);

        if ((ws_ex & WS_EX.WINDOWEDGE) == WS_EX.WINDOWEDGE)
        {
            edgeOffset.FixedFrame = new Size()
            {
                Width = SystemParameters.FixedFrameVerticalBorderWidth,
                Height = SystemParameters.FixedFrameHorizontalBorderHeight
            };
        }

        if ((ws_ex & WS_EX.CLIENTEDGE) == WS_EX.CLIENTEDGE)
        {
            edgeOffset.ThickBorder = new Size()
            {
                Width = SystemParameters.ThickVerticalBorderWidth,
                Height = SystemParameters.ThickHorizontalBorderHeight
            };
        }

        if ((ws_ex & WS_EX.TOOLWINDOW) == WS_EX.TOOLWINDOW)
        {
            edgeOffset.ThinBorder = new Size()
            {
                Width = SystemParameters.ThinVerticalBorderWidth,
                Height = SystemParameters.ThinHorizontalBorderHeight
            };
        }

        if ((ws & WS.THICKFRAME) == WS.THICKFRAME)
        {
            edgeOffset.ResizeFrame = new Size()
            {
                Width = SystemParameters.ResizeFrameVerticalBorderWidth,
                Height = SystemParameters.ResizeFrameHorizontalBorderHeight
            };
        }

        return edgeOffset;
    }

    /// <summary>
    /// Checks if the window's position has changed based on the given <see cref="WINDOWPOS"/> structure.
    /// </summary>
    internal static Func<WINDOWPOS, bool> WindowChanged = (windowPos) =>
    {
        return (windowPos.flags & SWP.NOMOVE) != SWP.NOMOVE ||
               (windowPos.flags & SWP.NOSIZE) != SWP.NOSIZE;
    };

    /// <summary>
    /// Determines if the window has been restored to its original size and position.
    /// </summary>
    internal static Func<Window, bool> SizeRestored = (window) =>
    {
        return window.Left == window.RestoreBounds.Left &&
               window.Top == window.RestoreBounds.Top &&
               window.Width == window.RestoreBounds.Width &&
               window.Height == window.RestoreBounds.Height;
    };

    /// <summary>
    /// Gets the actual state of the window based on its handle.
    /// </summary>
    /// <param name="hWnd">The handle of the window.</param>
    /// <returns>The actual <see cref="WindowState"/> of the window.</returns>
    internal static WindowState GetActualState(IntPtr hWnd)
    {
        var state = WindowState.Normal;
        var ws = NativeMethods.GetWindowStyle(hWnd);

        if ((ws & WS.MINIMIZE) == WS.MINIMIZE)
        {
            state = WindowState.Minimized;
        }
        else if ((ws & WS.MAXIMIZE) == WS.MAXIMIZE)
        {
            state = WindowState.Maximized;
        }

        return state;
    }

    #region IDisposable

    private bool _disposed = false;

    /// <summary>
    /// Finalizer for the <see cref="WindowSnap"/> class.
    /// </summary>
    ~WindowSnap()
    {
        Dispose(false);
    }

    /// <summary>
    /// Releases the resources used by the <see cref="WindowSnap"/> class.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Free other managed objects that 
            // implement IDisposable  
            if (_hWnd != IntPtr.Zero)
            {
                HwndSource.FromHwnd(_hWnd).RemoveHook(WindowProc);
                _hWnd = IntPtr.Zero;
            }
        }

        // Release any unmanaged objects
        // set the object references to null

        _disposed = true;
    }

    /// <summary>
    /// Disposes of the resources used by the <see cref="WindowSnap"/> class.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion 
}
