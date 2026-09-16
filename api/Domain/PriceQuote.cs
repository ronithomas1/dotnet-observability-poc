namespace ObservabilityDemo.OrderService.Domain;

public sealed record PriceQuote(
    decimal UnitPrice,
    decimal Discount,
    decimal Shipping,
    decimal Tax,
    decimal Total);
