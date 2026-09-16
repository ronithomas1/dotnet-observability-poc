# AppHost Project Conventions

## Project: .NET Aspire AppHost

### Purpose

The AppHost is the orchestration layer for the distributed application. It:
- Manages service lifecycle and dependencies
- Provides service discovery configuration
- Hosts the Aspire developer dashboard
- Configures environment variables and secrets
- Defines deployment topology

### AppHost.cs Pattern

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Define resources (databases, caches, message queues, etc.)
var cache = builder.AddRedis("cache");
var db = builder.AddPostgres("db");

// Define services and their dependencies
var api = builder.AddProject<Projects.ObservabilityDemo_OrderService>("api")
    .WithReference(cache)
    .WithReference(db);

var frontend = builder.AddProject<Projects.Frontend>("frontend")
    .WithReference(api);

builder.Build().Run();
```

### Adding Services

**Project References:**
```csharp
builder.AddProject<Projects.MyService>("service-name")
    .WithHttpEndpoint(port: 5001)
    .WithHttpsEndpoint(port: 5002);
```

**External Services:**
```csharp
// Containerized dependencies
builder.AddRedis("cache");
builder.AddPostgres("postgres");
builder.AddRabbitMQ("messaging");
builder.AddSqlServer("sqlserver");

// External endpoints
builder.AddConnectionString("external-api", "https://api.example.com");
```

### Service References

Services can reference other services for dependency injection:

```csharp
var cache = builder.AddRedis("cache");
var api = builder.AddProject<Projects.Api>("api")
    .WithReference(cache); // Injects connection string as environment variable
```

This makes the cache available to the API via service discovery.

### Environment Variables

**Setting Variables:**
```csharp
builder.AddProject<Projects.Api>("api")
    .WithEnvironment("FEATURE_FLAG_ENABLED", "true")
    .WithEnvironment("LOG_LEVEL", "Debug");
```

**Using Secrets (Development):**
User secrets are configured via `UserSecretsId` in the csproj:
```xml
<UserSecretsId>fb3d8176-15bd-45dd-bac9-32cbf2c3579f</UserSecretsId>
```

Access secrets in AppHost:
```csharp
var apiKey = builder.Configuration["ExternalApi:ApiKey"];
```

### Port Configuration

**Explicit Ports:**
```csharp
builder.AddProject<Projects.Api>("api")
    .WithHttpEndpoint(port: 5000, name: "http")
    .WithHttpsEndpoint(port: 5001, name: "https");
```

**Dynamic Ports (Recommended for Development):**
```csharp
builder.AddProject<Projects.Api>("api")
    .WithHttpEndpoint() // Aspire assigns available port
    .WithHttpsEndpoint();
```

### Health Checks

Services with health check endpoints are monitored by Aspire:

```csharp
builder.AddProject<Projects.Api>("api")
    .WithHealthCheck("/health"); // Monitor this endpoint
```

Dashboard displays health status for all services.

### Replicas and Scaling

```csharp
builder.AddProject<Projects.Api>("api")
    .WithReplicas(3); // Run 3 instances
```

### Launch Profiles

The AppHost determines which services to start:

**All Services (Default):**
```csharp
builder.Build().Run();
```

**Conditional Services:**
```csharp
var isDevelopment = builder.Environment.IsDevelopment();
if (isDevelopment)
{
    builder.AddProject<Projects.DebugService>("debug");
}
```

### Service Discovery Configuration

Services automatically discover each other via names:

```csharp
// In AppHost
var api = builder.AddProject<Projects.Api>("api");
var worker = builder.AddProject<Projects.Worker>("worker")
    .WithReference(api); // Worker can call http://api
```

In the worker service, use `HttpClient` with service name:
```csharp
var response = await httpClient.GetAsync("http://api/endpoint");
```

### Aspire Dashboard

Access the dashboard at the URL shown in the console when running AppHost.

**Dashboard Features:**
- View all services and their status
- Monitor logs from all services
- View distributed traces across services
- Inspect metrics and health checks
- View environment variables and configuration

### Configuration Files

**appsettings.json:**
- Logging configuration for the AppHost itself
- Aspire-specific settings

**aspire.config.json:**
- Aspire CLI configuration
- Deployment settings

### Running the AppHost

**Development:**
```bash
# From apphost directory
dotnet run

# Or from solution root
dotnet run --project apphost
```

**With Watch (Auto-reload):**
```bash
dotnet watch --project apphost
```

### Adding New Services to AppHost

1. Add project reference if needed in csproj
2. Add service in `AppHost.cs` using `builder.AddProject<>()`
3. Configure service references and environment variables
4. Run AppHost to verify service registration
5. Check Aspire dashboard for service status

### Deployment Considerations

**Container Deployment:**
```csharp
builder.AddProject<Projects.Api>("api")
    .WithDockerfile("../api/Dockerfile");
```

**Azure Container Apps:**
Aspire can generate deployment manifests:
```bash
dotnet publish --configuration Release
```

### Best Practices

1. **Use Service References**: Let Aspire handle service discovery instead of hardcoding URLs
2. **Dynamic Ports in Development**: Avoid port conflicts
3. **Health Checks**: Configure health endpoints for all services
4. **Secrets Management**: Use user secrets in development, proper secret stores in production
5. **Logging**: Keep AppHost logging minimal; detailed logs belong in services
6. **Resource Naming**: Use descriptive names for services and resources
