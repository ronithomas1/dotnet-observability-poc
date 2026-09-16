namespace ObservabilityDemo.OrderService.Domain;

public sealed record OrderListItem(
    string OrderId,
    string CustomerId,
    string ProductSku,
    int Quantity,
    decimal Total,
    string Status,
    DateTimeOffset CreatedUtc);
