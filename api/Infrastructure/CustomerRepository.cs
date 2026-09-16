using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ObservabilityDemo.OrderService.Domain;

namespace ObservabilityDemo.OrderService.Infrastructure;

public sealed class CustomerRepository(
    AppDbContext db, 
    ILogger<CustomerRepository> logger,
    IConfiguration configuration)
{
    private readonly ActivitySource _activitySource = 
        new(configuration["Telemetry:ServiceName"] ?? "ObservabilityDemo.OrderService");

    public async Task<IReadOnlyList<CustomerProfile>> GetAllAsync(int take, CancellationToken ct = default)
    {
        return await db.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.Id)
            .Take(take)
            .Select(customer => new CustomerProfile(
                customer.Id,
                customer.DisplayName,
                customer.Email,
                customer.Region,
                customer.LoyaltyTier,
                customer.UpdatedUtc))
            .ToListAsync(ct);
    }

    public async Task<CustomerProfile?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var customer = await db.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Email == email, ct);

        if (customer is null)
        {
            return null;
        }

        return new CustomerProfile(
            customer.Id,
            customer.DisplayName,
            customer.Email,
            customer.Region,
            customer.LoyaltyTier,
            customer.UpdatedUtc);
    }

    public async Task<CustomerProfile?> GetByIdAsync(string customerId, CancellationToken ct = default)
    {
        var customer = await db.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == customerId, ct);

        if (customer is null)
        {
            return null;
        }

        logger.LogDebug("Loaded customer profile {CustomerId}", customerId);

        // [DEMO-BLOCK: tracing-extras] Customer mapping span intentionally disabled for the M2C5 demo flow.
        // using var mapActivity = _activitySource.StartActivity(
        //     "customer.result.map",
        //     ActivityKind.Internal);
        // mapActivity?.SetTag("customer.id", customerId);

        return new CustomerProfile(
            customer.Id,
            customer.DisplayName,
            customer.Email,
            customer.Region,
            customer.LoyaltyTier,
            customer.UpdatedUtc);
    }

    public async Task<CustomerProfile?> UpdateDisplayNameAsync(
        string customerId,
        string displayName,
        bool simulateConflict,
        CancellationToken ct = default)
    {
        if (simulateConflict)
        {
            throw new InvalidOperationException($"Concurrent customer update detected for {customerId}");
        }

        var customer = await db.Customers.FirstOrDefaultAsync(item => item.Id == customerId, ct);
        if (customer is null)
        {
            return null;
        }

        customer.DisplayName = displayName;
        customer.UpdatedUtc = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogDebug("Updated customer profile {CustomerId}", customerId);

        return new CustomerProfile(
            customer.Id,
            customer.DisplayName,
            customer.Email,
            customer.Region,
            customer.LoyaltyTier,
            customer.UpdatedUtc);
    }
}
