using ObservabilityDemo.OrderService.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (observability, health checks, resilience)
builder.AddServiceDefaults();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseHttpsRedirection();

// Map default endpoints (health checks)
app.MapDefaultEndpoints();

// Map API endpoints
app.MapCustomerEndpoints();
app.MapOrderEndpoints();
app.MapDemoApiEndpoints();
app.MapCatalogEndpoints();

app.Run();
