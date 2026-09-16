using Microsoft.EntityFrameworkCore;

namespace ObservabilityDemo.OrderService.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CustomerEntity> Customers => Set<CustomerEntity>();
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerEntity>().HasKey(customer => customer.Id);
        modelBuilder.Entity<ProductEntity>().HasKey(product => product.Sku);
        modelBuilder.Entity<OrderEntity>().HasKey(order => order.Id);
    }
}

public sealed class CustomerEntity
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string LoyaltyTier { get; set; } = string.Empty;
    public DateTimeOffset UpdatedUtc { get; set; }
}

public sealed class ProductEntity
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int AvailableStock { get; set; }
}

public sealed class OrderEntity
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Shipping { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedUtc { get; set; }
}

public static class DemoDatabaseInitializer
{
    public static async Task InitializeAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.Customers.AnyAsync())
        {
            return;
        }

        db.Customers.AddRange(
            new CustomerEntity
            {
                Id = "cust-001",
                DisplayName = "John Doe",
                Email = "john.doe@globomantics.com",
                Region = "SE",
                LoyaltyTier = "gold",
                UpdatedUtc = DateTimeOffset.UtcNow.AddDays(-2),
            },
            new CustomerEntity
            {
                Id = "cust-002",
                DisplayName = "Jane Smith",
                Email = "jane.smith@globomantics.com",
                Region = "NO",
                LoyaltyTier = "silver",
                UpdatedUtc = DateTimeOffset.UtcNow.AddDays(-1),
            },
            new CustomerEntity
            {
                Id = "cust-003",
                DisplayName = "Michael Johnson",
                Email = "michael.johnson@globomantics.com",
                Region = "DK",
                LoyaltyTier = "standard",
                UpdatedUtc = DateTimeOffset.UtcNow.AddDays(-4),
            },
            new CustomerEntity
            {
                Id = "cust-42",
                DisplayName = "Emily Williams",
                Email = "emily.williams@globomantics.com",
                Region = "SE",
                LoyaltyTier = "standard",
                UpdatedUtc = DateTimeOffset.UtcNow.AddDays(-3),
            },
            new CustomerEntity
            {
                Id = "cust-099",
                DisplayName = "James Brown",
                Email = "james.brown@globomantics.com",
                Region = "FI",
                LoyaltyTier = "gold",
                UpdatedUtc = DateTimeOffset.UtcNow.AddHours(-12),
            });

        db.Products.AddRange(
            new ProductEntity { Sku = "SKU-A1", Name = "Telemetry Mug", Category = "swag", UnitPrice = 12.90m, AvailableStock = 24000000 },
            new ProductEntity { Sku = "SKU-B2", Name = "Tracing T-Shirt", Category = "apparel", UnitPrice = 28.00m, AvailableStock = 18000000 },
            new ProductEntity { Sku = "SKU-C3", Name = "Metrics Notebook", Category = "stationery", UnitPrice = 8.40m, AvailableStock = 3200000 },
            new ProductEntity { Sku = "SKU-D4", Name = "Logs Hoodie", Category = "apparel", UnitPrice = 44.00m, AvailableStock = 900000 });

        await db.SaveChangesAsync();
    }
}
