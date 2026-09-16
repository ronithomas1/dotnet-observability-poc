namespace ObservabilityDemo.OrderService.Domain;

public sealed record RequestSimulationOptions(
    bool IgnoreDemoSettings = false,
    bool ForceSlowInventory = false,
    bool ForceInventoryFailure = false,
    bool ForceInventoryTransportFailure = false,
    bool ForcePaymentFailure = false,
    bool ForceDbFailure = false,
    bool ForceCustomerConflict = false,
    bool SimulateFailure = false,
    string Scenario = "normal",
    int SlowInventoryDelayMs = 0);   // 0 = use default 2000 ms slow path
