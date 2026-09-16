using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ObservabilityDemo.OrderService.Domain;

namespace ObservabilityDemo.OrderService.Infrastructure;

public sealed class CatalogRepository(
    AppDbContext db, 
    ILogger<CatalogRepository> logger,
    IConfiguration configuration)
{
    private readonly ActivitySource _activitySource = 
        new(configuration["Telemetry:ServiceName"] ?? "ObservabilityDemo.OrderService");

    public async Task<IReadOnlyList<CatalogItem>> GetProductsAsync(int take, CancellationToken ct = default)
    {
        return await db.Products
            .AsNoTracking()
            .OrderBy(product => product.Sku)
            .Take(take)
            .Select(product => new CatalogItem(
                product.Sku,
                product.Name,
                product.Category,
                product.UnitPrice,
                product.AvailableStock))
            .ToListAsync(ct);
    }

    public async Task<CatalogItem?> GetBySkuAsync(string sku, CancellationToken ct = default)
    {
        var product = await db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Sku == sku, ct);

        if (product is null)
        {
            return null;
        }

        logger.LogDebug("Loaded catalog item {Sku}", sku);

        // [DEMO-BLOCK: tracing-extras] Catalog mapping span intentionally disabled for the M2C5 demo flow.
        // using var mapActivity = _activitySource.StartActivity(
        //     "catalog.result.map",
        //     ActivityKind.Internal);
        // mapActivity?.SetTag("product.sku", sku);

        return new CatalogItem(
            product.Sku,
            product.Name,
            product.Category,
            product.UnitPrice,
            product.AvailableStock);
    }
}
