namespace ObservabilityDemo.OrderService.Domain;

public sealed record CustomerProfile(
    string CustomerId,
    string DisplayName,
    string Email,
    string Region,
    string LoyaltyTier,
    DateTimeOffset UpdatedUtc);
