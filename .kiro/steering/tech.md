# Technology Stack

## Framework & Runtime

- **.NET 10.0**: Target framework for all projects
- **ASP.NET Core**: Web API framework
- **.NET Aspire 13.5.3**: Cloud-native orchestration and observability platform

## Language Features

- **C# with Implicit Usings**: Enabled across all projects
- **Nullable Reference Types**: Enabled for null safety

## Observability Stack

### OpenTelemetry
- **OpenTelemetry.Exporter.OpenTelemetryProtocol** (1.15.3): OTLP export
- **OpenTelemetry.Extensions.Hosting** (1.15.3): .NET hosting integration
- **OpenTelemetry.Instrumentation.AspNetCore** (1.15.2): HTTP request tracing
- **OpenTelemetry.Instrumentation.Http** (1.15.1): HTTP client tracing
- **OpenTelemetry.Instrumentation.Runtime** (1.15.1): Runtime metrics

### Telemetry Configuration
- Service name and version configured in `appsettings.json` under `Telemetry` section
- Automatic enrichment with environment, host, and deployment metadata
- Health check endpoints excluded from tracing (`/health`, `/alive`)

## Resilience & Service Discovery

- **Microsoft.Extensions.Http.Resilience** (10.8.0): Retry, circuit breaker, timeout patterns
- **Microsoft.Extensions.ServiceDiscovery** (10.8.0): Dynamic service resolution

## Build System

### Projects Structure
- **AppHost.csproj**: Uses `Aspire.AppHost.Sdk` (13.5.3)
- **OrderService.csproj**: Standard `Microsoft.NET.Sdk.Web`
- **ServiceDefaults.csproj**: Shared library marked with `IsAspireSharedProject`

### Common Commands

```bash
# Build the solution
dotnet build

# Run the AppHost (starts all services)
dotnet run --project apphost

# Run the API service directly
dotnet run --project api

# Restore dependencies
dotnet restore

# Clean build artifacts
dotnet clean

# Run tests (when added)
dotnet test
```

## Development Environment

- **User Secrets**: Configured for AppHost (ID: fb3d8176-15bd-45dd-bac9-32cbf2c3579f)
- **HTTPS Redirection**: Enabled by default
- **Development-only Health Endpoints**: `/health` and `/alive` only available in development

## Exporter Configuration

Configure OTLP exporter via environment variable:
```
OTEL_EXPORTER_OTLP_ENDPOINT=http://your-collector:4318
```

Optional Azure Monitor support (commented out, requires package installation):
```
APPLICATIONINSIGHTS_CONNECTION_STRING=your-connection-string
```
