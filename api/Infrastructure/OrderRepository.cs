using Microsoft.EntityFrameworkCore;
using ObservabilityDemo.OrderService.Domain;

namespace ObservabilityDemo.OrderService.Infrastructure;

public sealed class OrderRepository(AppDbContext db, ILogger<OrderRepository> logger)
{
    public async Task SaveAsync(
        Order order,
        CatalogItem product,
        PriceQuote quote,
        RequestSimulationOptions simulation,
        CancellationToken ct = default)
    {
        await Task.Delay(Random.Shared.Next(6, 24), ct);

        if (simulation.ForceDbFailure)
        {
            throw new InvalidOperationException(
                "Failed to open connection: Server=prod-db-01;Database=orders;" +
                "User Id=svc_orders;Password=Sup3rSecret!;Port=5432");
        }

        var productRow = await db.Products.FirstAsync(item => item.Sku == product.Sku, ct);
        productRow.AvailableStock = Math.Max(0, productRow.AvailableStock - order.Quantity);

        db.Orders.Add(new OrderEntity
        {
            Id = order.OrderId,
            CustomerId = order.CustomerId,
            ProductSku = order.ProductSku,
            Quantity = order.Quantity,
            UnitPrice = quote.UnitPrice,
            Shipping = quote.Shipping,
            Tax = quote.Tax,
            Total = quote.Total,
            Status = "accepted",
            CreatedUtc = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync(ct);

        logger.LogDebug("Order {OrderId} saved to SQLite", order.OrderId);
    }

    public async Task<IReadOnlyList<OrderListItem>> GetRecentAsync(int take, CancellationToken ct = default)
    {
        var orders = await db.Orders
            .AsNoTracking()
            .ToListAsync(ct);

        return orders
            .OrderByDescending(order => order.CreatedUtc)
            .Take(take)
            .Select(order => new OrderListItem(
                order.Id,
                order.CustomerId,
                order.ProductSku,
                order.Quantity,
                order.Total,
                order.Status,
                order.CreatedUtc))
            .ToList();
    }
}
