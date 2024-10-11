using System;
using System.Windows.Threading;

namespace DevToolbox.Wpf.Controls;

internal class KeepAliveTimer
{
    #region Methods

    private readonly DispatcherTimer _timer;
    private DateTime _startTime;
    private TimeSpan? _runTime;

    #endregion

    #region Properties

    public TimeSpan Time { get; }

    public Action Action { get; }

    public bool Running { get; private set; }

    #endregion

    #region Constructor

    public KeepAliveTimer(TimeSpan time, Action action)
    {
        Time = time;
        Action = action;
        _timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle) { Interval = time };
        _timer.Tick += TimerExpired;
    }

    #endregion

    #region Methods

    private void TimerExpired(object? sender, EventArgs e)
    {
        lock (_timer)
        {
            Running = false;
            _timer.Stop();
            _runTime = DateTime.UtcNow.Subtract(_startTime);
            Action();
        }
    }

    public void Nudge()
    {
        lock (_timer)
        {
            if (!Running)
            {
                _startTime = DateTime.UtcNow;
                _runTime = null;
                _timer.Start();
                Running = true;
            }
            else
            {
                //Reset the timer
                _timer.Stop();
                _timer.Start();
            }
        }
    }

    public TimeSpan GetTimeSpan() => _runTime ?? DateTime.UtcNow.Subtract(_startTime);

    #endregion
}