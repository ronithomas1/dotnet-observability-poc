# Service Defaults Conventions

## Project: ServiceDefaults (Shared Library)

### Purpose

The ServiceDefaults project is a shared library that provides common configuration for observability, resilience, and health checks across all services in the solution.

**Key Principle:** Every service should reference this project and call `builder.AddServiceDefaults()` to inherit standard telemetry configuration.

### Project Structure

```
shared/
├── Extensions.cs                # All extension methods
└── ServiceDefaults.csproj      # Shared project configuration
```

### Core Extension Methods

#### 1. AddServiceDefaults()

Primary entry point that configures all common services:

```csharp
builder.AddServiceDefaults();
```

**What it does:**
- Configures OpenTelemetry (tracing, metrics, logging)
- Adds default health checks (self + live)
- Enables service discovery
- Configures HTTP client defaults with resilience and service discovery

**Usage in service Program.cs:**
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults(); // Always first after builder creation
```

#### 2. ConfigureOpenTelemetry()

Sets up comprehensive OpenTelemetry instrumentation:

```csharp
builder.ConfigureOpenTelemetry();
```

**Configures:**
- **Resource attributes**: Service name, version, environment, host
- **Tracing**: ASP.NET Core requests, HTTP client calls, custom activity sources
- **Metrics**: ASP.NET Core metrics, HTTP client metrics, runtime metrics
- **Logging**: Structured logging with OpenTelemetry format

**Resource Attributes:**
- `service.name`: From `Telemetry:ServiceName` config or app name
- `service.version`: From `Telemetry:ServiceVersion` config or "1.0.0"
- `deployment.environment`: From `ASPNETCORE_ENVIRONMENT`
- `host.name`: Machine name

**Trace Filtering:**
Health check endpoints are excluded from tracing:
- `/health`
- `/alive`

#### 3. AddDefaultHealthChecks()

Adds standard health and liveness checks:

```csharp
builder.AddDefaultHealthChecks();
```

**Checks:**
- `self`: Basic liveness check (always healthy)
- Additional checks can be added by services

**Tags:**
- `live`: Used for Kubernetes liveness probes

#### 4. MapDefaultEndpoints()

Maps health check endpoints (development only):

```csharp
app.MapDefaultEndpoints();
```

**Endpoints:**
- `GET /health`: All health checks must pass (readiness)
- `GET /alive`: Only "live" tagged checks must pass (liveness)

**Security:** Only available in `IsDevelopment()` to prevent information disclosure in production.

### Configuration Requirements

Each service must include telemetry configuration in `appsettings.json`:

```json
{
  "Telemetry": {
    "ServiceName": "YourService.Name",
    "ServiceVersion": "1.0.0"
  }
}
```

**Naming Convention:**
- Use fully qualified service names: `{Company}.{Product}.{Service}`
- Example: `ObservabilityDemo.OrderService`

### HTTP Client Configuration

All `HttpClient` instances automatically get:

**Resilience:**
- Retry with exponential backoff
- Circuit breaker
- Timeout policies

**Service Discovery:**
- Resolve service names to endpoints
- Works with Aspire service references

**Example:**
```csharp
// In a service
services.AddHttpClient<IOrderClient, OrderClient>(client =>
{
    client.BaseAddress = new Uri("http://order-service"); // Service discovery resolves this
});
```

### OpenTelemetry Exporters

**OTLP Exporter (Default):**
Enabled when environment variable is set:
```
OTEL_EXPORTER_OTLP_ENDPOINT=http://collector:4318
```

**Azure Monitor (Optional):**
Uncomment in code and set connection string:
```
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=...
```

### Extending Service Defaults

#### Adding Custom Instrumentation

Services can add their own `ActivitySource` for custom tracing:

```csharp
// In your service
public static class Telemetry
{
    public static readonly ActivitySource ActivitySource = new("YourService.Name");
}

// Register in Program.cs after AddServiceDefaults()
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Telemetry.ActivitySource.Name));
```

#### Adding Custom Health Checks

After calling `AddDefaultHealthChecks()`:

```csharp
builder.AddDefaultHealthChecks();

// Add custom checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck<CacheHealthCheck>("cache", tags: new[] { "live" });
```

#### Customizing HTTP Client Resilience

Override defaults for specific clients:

```csharp
services.AddHttpClient<IExternalApi, ExternalApiClient>()
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 5;
        options.CircuitBreaker.FailureRatio = 0.5;
    });
```

### Instrumentation Details

**Automatic Instrumentation:**
- All incoming HTTP requests (ASP.NET Core)
- All outgoing HTTP requests (HttpClient)
- Runtime metrics (GC, thread pool, etc.)

**Manual Instrumentation:**
Use `ActivitySource` for business logic:

```csharp
using var activity = ActivitySource.StartActivity("OperationName");
activity?.SetTag("custom.property", value);
activity?.AddEvent(new ActivityEvent("Something happened"));
// Business logic
```

### Metrics Collected

**ASP.NET Core:**
- Request duration
- Request count by status code
- Active requests

**HTTP Client:**
- Request duration
- Request count by status code
- Active requests

**Runtime:**
- GC collection counts and duration
- Thread pool size and queue length
- Exception counts

### Logging Integration

**Structured Logging:**
- All logs automatically include trace context
- Scopes are included in telemetry
- Formatted messages included

**Best Practices:**
```csharp
logger.LogInformation("Processing order {OrderId} for customer {CustomerId}", 
    orderId, customerId);
```

Avoid string interpolation in log messages; use structured parameters.

### Testing Without Observability

For unit tests, service defaults can be skipped or mocked:

```csharp
// In test setup
var builder = WebApplication.CreateBuilder();
// Don't call AddServiceDefaults() in tests
```

### Modifying Service Defaults

When updating this library:

1. **Ensure backward compatibility**: Services depend on these APIs
2. **Update all services**: If changing required configuration
3. **Document breaking changes**: Update this guide
4. **Version appropriately**: Consider semantic versioning

### Common Patterns

**New Service Setup:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Step 1: Add service defaults (observability, resilience, health)
builder.AddServiceDefaults();

// Step 2: Add service-specific dependencies
builder.Services.AddScoped<IMyService, MyService>();

var app = builder.Build();

// Step 3: Map default endpoints (health checks)
app.MapDefaultEndpoints();

// Step 4: Map service-specific endpoints
app.MapMyEndpoints();

app.Run();
```

**Configuration File Template:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Telemetry": {
    "ServiceName": "YourService.Name",
    "ServiceVersion": "1.0.0"
  }
}
```

### Troubleshooting

**Traces Not Appearing:**
1. Check `OTEL_EXPORTER_OTLP_ENDPOINT` is set
2. Verify collector is running and reachable
3. Check service name in configuration

**Health Checks Not Working:**
1. Ensure `MapDefaultEndpoints()` is called
2. Verify running in Development environment
3. Check endpoint paths: `/health` and `/alive`

**Service Discovery Failing:**
1. Ensure services are registered in AppHost
2. Verify service names match between AppHost and HttpClient calls
3. Check Aspire dashboard for service registration
