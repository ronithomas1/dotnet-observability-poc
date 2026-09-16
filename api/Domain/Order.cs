namespace ObservabilityDemo.OrderService.Domain;

public record Order(
    string OrderId,
    string CustomerId,
    string CustomerEmail,  // PII — used in Demo 4 to show PII masking
    string ProductSku,
    int Quantity);
