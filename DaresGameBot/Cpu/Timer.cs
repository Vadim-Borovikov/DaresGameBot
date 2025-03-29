using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace DaresGameBot.Cpu;

[PublicAPI]
public sealed class Timer
{
    public void Start()
    {
        _start = Process.GetCurrentProcess().TotalProcessorTime;
        _lastSnapshot = _start;
        _isMeasuring = true;
    }

    public Result Stop()
    {
        Result snapshot = Snapshot();
        _isMeasuring = false;
        return snapshot;
    }

    public Result Snapshot()
    {
        TimeSpan now = Process.GetCurrentProcess().TotalProcessorTime;

        if (!_isMeasuring)
        {
            throw new InvalidOperationException("Measurement not started.");
        }

        TimeSpan total = now - _start;
        TimeSpan sinceLastSnapshot = now - _lastSnapshot;
        _lastSnapshot = now;
        return new Result(total.TotalMilliseconds, sinceLastSnapshot.TotalMilliseconds);
    }

    private TimeSpan _start;
    private TimeSpan _lastSnapshot;
    private bool _isMeasuring;
}