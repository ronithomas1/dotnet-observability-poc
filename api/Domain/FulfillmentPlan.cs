namespace ObservabilityDemo.OrderService.Domain;

public sealed record FulfillmentPlan(
    string Warehouse,
    int EstimatedDeliveryDays);
