# API Project Conventions

## Project: ObservabilityDemo.OrderService

### Endpoint Organization

All API endpoints are organized in the `/Endpoints` directory as static classes:

```csharp
namespace ObservabilityDemo.OrderService.Endpoints;

public static class {Feature}Endpoints
{
    public static IEndpointRouteBuilder Map{Feature}Endpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/{resource}").WithTags("{Feature}");
        
        // Define endpoints here
        
        return app;
    }
}
```

### Endpoint Patterns

**Route Structure:**
- Base path: `/api/{resource}`
- Use plural resource names (e.g., `/api/orders`, `/api/customers`)
- Group related endpoints using `MapGroup()`
- Always use `.WithTags()` for OpenAPI documentation

**HTTP Methods:**
- GET for retrieval: `group.MapGet("/{id}", handler)`
- POST for creation: `group.MapPost("/", handler)`
- PUT for full updates: `group.MapPut("/{id}", handler)`
- PATCH for partial updates: `group.MapPatch("/{id}", handler)`
- DELETE for removal: `group.MapDelete("/{id}", handler)`

**Example Endpoint Definition:**
```csharp
public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/api/orders").WithTags("Orders");

    group.MapGet("/", GetAllOrders)
        .WithName("GetOrders")
        .WithOpenApi();

    group.MapGet("/{id}", GetOrderById)
        .WithName("GetOrderById")
        .WithOpenApi();

    group.MapPost("/", CreateOrder)
        .WithName("CreateOrder")
        .WithOpenApi();

    return app;
}
```

### Handler Patterns

**Inline Handlers (Simple):**
```csharp
group.MapGet("/", () => Results.Ok(new { message = "Hello" }));
```

**Local Functions (Moderate Complexity):**
```csharp
group.MapGet("/{id}", GetById);

static IResult GetById(int id)
{
    // Implementation
    return Results.Ok();
}
```

**Separate Classes (Complex Logic):**
For complex handlers, create separate handler classes in `/Endpoints/Handlers/`.

### Registration in Program.cs

All endpoint extension methods must be registered in `Program.cs`:

```csharp
// Map API endpoints
app.MapCustomerEndpoints();
app.MapOrderEndpoints();
app.MapDemoApiEndpoints();
app.MapCatalogEndpoints();
```

### Observability Integration

**Automatic Tracing:**
- All HTTP requests are automatically traced via OpenTelemetry
- No manual instrumentation needed for basic request/response tracking

**Manual Spans (for complex operations):**
```csharp
using System.Diagnostics;

private static readonly ActivitySource ActivitySource = new("ObservabilityDemo.OrderService");

group.MapPost("/", async (Order order) =>
{
    using var activity = ActivitySource.StartActivity("ProcessOrder");
    activity?.SetTag("order.id", order.Id);
    
    // Business logic
    
    return Results.Created($"/api/orders/{order.Id}", order);
});
```

**Logging:**
```csharp
group.MapGet("/{id}", (int id, ILogger<Program> logger) =>
{
    logger.LogInformation("Retrieving order {OrderId}", id);
    // Implementation
});
```

### Error Handling

Use Results API for consistent responses:

```csharp
// Success
return Results.Ok(data);
return Results.Created($"/api/resource/{id}", data);
return Results.NoContent();

// Client Errors
return Results.NotFound();
return Results.BadRequest(new { error = "Invalid data" });
return Results.ValidationProblem(errors);

// Server Errors
return Results.Problem("Internal error", statusCode: 500);
```

### Health Checks

Health check endpoints are provided by `MapDefaultEndpoints()`:
- `/health` - Overall health status
- `/alive` - Liveness check

**Available in Development Only** for security.

### Configuration

Service configuration in `appsettings.json`:

```json
{
  "Telemetry": {
    "ServiceName": "ObservabilityDemo.OrderService",
    "ServiceVersion": "1.0.0"
  }
}
```

Update `ServiceName` if creating a new service.

### Adding New Endpoint Groups

1. Create `{Feature}Endpoints.cs` in `/Endpoints/`
2. Implement `Map{Feature}Endpoints` extension method
3. Define routes using `MapGroup("/api/{resource}")`
4. Add `.WithTags("{Feature}")` for OpenAPI
5. Register in `Program.cs`: `app.Map{Feature}Endpoints()`

### Testing Endpoints

Use the `.http` file for manual testing:
- File: `ObservabilityDemo.OrderService.http`
- Use VS Code REST Client or similar tools
- Add new requests as endpoints are created
