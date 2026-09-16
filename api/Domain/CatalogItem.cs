namespace ObservabilityDemo.OrderService.Domain;

public sealed record CatalogItem(
    string Sku,
    string Name,
    string Category,
    decimal UnitPrice,
    int AvailableStock);
