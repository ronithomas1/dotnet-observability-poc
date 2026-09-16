# Project Structure

## Repository Organization

This is a multi-workspace .NET Aspire solution with the following top-level directories:

```
dotnet-observability-poc/
├── api/              # Order Service API
├── apphost/          # .NET Aspire orchestrator
├── frontend/         # Frontend project (empty, reserved for future use)
├── serverless/       # Serverless functions (empty, reserved for future use)
└── shared/           # Service Defaults (shared library)
```

## Project Details

### `/api` - ObservabilityDemo.OrderService
The main REST API service demonstrating observable patterns.

**Structure:**
```
api/
├── Endpoints/                    # Endpoint route definitions
│   ├── CatalogEndpoints.cs
│   ├── CustomerEndpoints.cs
│   ├── DemoApiEndpoints.cs
│   └── OrderEndpoints.cs
├── Properties/                   # Launch settings
├── appsettings.json             # Service configuration (includes Telemetry config)
├── appsettings.Development.json # Development overrides
├── Program.cs                   # Application entry point
└── ObservabilityDemo.OrderService.csproj
```

**Key Patterns:**
- Minimal APIs with endpoint route mapping
- Endpoints organized by feature into static extension classes
- Each endpoint class exposes a `Map{Feature}Endpoints(IEndpointRouteBuilder)` method
- Endpoints grouped under `/api/{resource}` with tags for OpenAPI
- Service defaults added via `builder.AddServiceDefaults()`

### `/apphost` - App Host
.NET Aspire orchestrator for local development and deployment.

**Structure:**
```
apphost/
├── Properties/                   # Launch settings
├── appsettings.json             # Host configuration
├── appsettings.Development.json # Development overrides
├── aspire.config.json           # Aspire-specific settings
├── AppHost.cs                   # Distributed application builder
└── AppHost.csproj
```

**Purpose:**
- Orchestrates all services in the solution
- Provides developer dashboard
- Manages service discovery and configuration

### `/shared` - ServiceDefaults
Shared library providing common observability and resilience configuration.

**Structure:**
```
shared/
├── Extensions.cs                # Service defaults extension methods
└── ServiceDefaults.csproj
```

**Exports:**
- `AddServiceDefaults<TBuilder>()`: Configures OpenTelemetry, health checks, service discovery, and resilience
- `ConfigureOpenTelemetry<TBuilder>()`: Sets up tracing, metrics, and logging
- `AddDefaultHealthChecks<TBuilder>()`: Adds standard health/liveness checks
- `MapDefaultEndpoints(WebApplication)`: Maps `/health` and `/alive` endpoints (dev only)

**Design Pattern:**
- Generic extension methods on `IHostApplicationBuilder`
- Centralized configuration for consistent observability across services
- All services reference this project to inherit telemetry standards

## Workspace Configuration

Multi-root workspace defined in `shared/dotnet-observability-poc.code-workspace`

## Naming Conventions

- **Namespaces**: Follow folder structure (e.g., `ObservabilityDemo.OrderService.Endpoints`)
- **Endpoint Classes**: `{Feature}Endpoints` (static classes)
- **Extension Methods**: `Map{Feature}Endpoints` for route mapping
- **Project Names**: `{Company}.{Service}` or descriptive purpose (e.g., `ServiceDefaults`)

## Configuration Files

- **appsettings.json**: Base configuration for all environments
- **appsettings.Development.json**: Development-specific overrides
- **Telemetry Section**: Required in service appsettings.json with `ServiceName` and `ServiceVersion`

## Adding New Services

When adding a new service:

1. Create project in its own directory
2. Reference `../shared/ServiceDefaults.csproj`
3. Call `builder.AddServiceDefaults()` in Program.cs
4. Add telemetry configuration to appsettings.json
5. Call `app.MapDefaultEndpoints()` before running
6. Register with AppHost for orchestration

## Adding New Endpoints

When adding endpoints to the API:

1. Create `{Feature}Endpoints.cs` in `/api/Endpoints/`
2. Implement static `Map{Feature}Endpoints(IEndpointRouteBuilder)` method
3. Use `MapGroup("/api/{resource}").WithTags("{Feature}")`
4. Register in `Program.cs`: `app.Map{Feature}Endpoints()`
