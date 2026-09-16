namespace ObservabilityDemo.OrderService.Demo;

/// <summary>
/// Controls live demo behaviours from the workbench.
/// All flags start off disabled and can be toggled at runtime with no restart.
///
/// Fault injection:
///   SlowInventory   → inventory call takes ~2 s  (shows slow dependency in trace)
///   FailInventory   → inventory call throws       (shows error span + correlated log)
///   HighLoad        → load generator fires ~20 req/s (stresses metrics histograms)
///   DegradingLoad   → load generator ramps slow-order percentage from 5% → 65% over ~30 s,
///                     producing a climbing P95/P99 curve while P50 stays flat — ideal for
///                     recording the "tail latency rising" demo beat.
/// </summary>
public sealed class DemoSettings
{
    /// <summary>Raised whenever any property changes, so Blazor components can re-render.</summary>
    public event Action? Changed;

    private bool _slowInventory;
    private bool _failInventory;
    private bool _highLoad;
    private bool _degradingLoad;

    public bool SlowInventory
    {
        get => _slowInventory;
        set { _slowInventory = value; Changed?.Invoke(); }
    }

    public bool FailInventory
    {
        get => _failInventory;
        set { _failInventory = value; Changed?.Invoke(); }
    }

    public bool HighLoad
    {
        get => _highLoad;
        set { _highLoad = value; Changed?.Invoke(); }
    }

    public bool DegradingLoad
    {
        get => _degradingLoad;
        set { _degradingLoad = value; Changed?.Invoke(); }
    }
}

