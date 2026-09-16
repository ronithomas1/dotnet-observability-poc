using System.Diagnostics;
using ObservabilityDemo.OrderService.Demo;
using ObservabilityDemo.OrderService.Domain;

namespace ObservabilityDemo.OrderService.Infrastructure;

/// <summary>
/// Simulates an outbound HTTP call to an Inventory service.
/// Respects DemoSettings.SlowInventory and DemoSettings.FailInventory
/// so fault injection is controlled from the live workbench toggles.
///
/// The <c>inventory.partner.call</c> span is intentionally kept as a grandchild
/// of <c>order.process</c> (via <c>inventory.check</c>) so the M2C5 demo can show
/// a parent → child → grandchild trace tree.
/// </summary>
public sealed class InventoryClient(
#pragma warning disable IDE0060 // http is present for when a real inventory URL is wired in
    HttpClient http,
#pragma warning restore IDE0060
    DemoSettings settings,
    ILogger<InventoryClient> logger,
    IConfiguration configuration)
{
    private readonly DemoSettings _settings = settings;
    private readonly ActivitySource _activitySource = 
        new(configuration["Telemetry:ServiceName"] ?? "ObservabilityDemo.OrderService");

    public async Task CheckStockAsync(
        string sku,
        int qty,
        int availableStock,
        RequestSimulationOptions simulation,
        CancellationToken ct = default)
    {
        var shouldSlow = simulation.ForceSlowInventory
            || (!simulation.IgnoreDemoSettings && _settings.SlowInventory);
        var shouldFail = simulation.ForceInventoryFailure
            || (!simulation.IgnoreDemoSettings && _settings.FailInventory);

        // [DEMO-BLOCK: tracing-partner-call] Uncomment in M2C5 to reveal the inventory.partner.call grandchild span.
        // using (var partnerCallActivity = _activitySource.StartActivity(
        //            "inventory.partner.call",
        //            ActivityKind.Client))
        // {
            // partnerCallActivity?.SetTag("inventory.sku", sku);
            // partnerCallActivity?.SetTag("inventory.qty", qty);
            // partnerCallActivity?.SetTag("inventory.slow_path", shouldSlow);

            if (shouldSlow)
            {
                // SlowInventoryDelayMs = 0 means use the default 2000 ms path.
                // Degrading Load ramps this value through bucket boundaries over time.
                var delayMs = simulation.SlowInventoryDelayMs > 0
                    ? simulation.SlowInventoryDelayMs
                    : 2000;
                await Task.Delay(delayMs, ct);
            }
            else
            {
                await Task.Delay(Random.Shared.Next(12, 40), ct);
            }

            if (shouldFail || simulation.SimulateFailure)
            {
                throw new HttpRequestException(
                    $"Inventory service unavailable for SKU {sku}. " +
                    $"Attempted GET /stock/{sku}?qty={qty}");
            }

            if (simulation.ForceInventoryTransportFailure)
            {
                var connectionFailure = new TimeoutException(
                    $"Connection to inventory-svc:8443 timed out after 5000ms while requesting SKU {sku}.");
                throw new HttpRequestException(
                    $"Inventory service unavailable for SKU {sku}. " +
                    $"Attempted GET /stock/{sku}?qty={qty}",
                    connectionFailure);
            }
        // }

        if (availableStock < qty)
        {
            throw new InvalidOperationException(
                $"SKU {sku} has insufficient stock. Requested {qty}, available {availableStock}.");
        }

        logger.LogDebug("Inventory check for SKU {Sku} qty {Qty}: in-stock", sku, qty);
    }
}
