using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ObservabilityDemo.OrderService.Demo;
using ObservabilityDemo.OrderService.Endpoints;
using ObservabilityDemo.OrderService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (observability, health checks, resilience)
builder.AddServiceDefaults();

// Configure SQLite in-memory database
const string SqliteConnectionString = "Data Source=observability-demo;Mode=Memory;Cache=Shared";

builder.Services.AddSingleton(_ =>
{
    var keepAliveConnection = new SqliteConnection(SqliteConnectionString);
    keepAliveConnection.Open();
    return keepAliveConnection;
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(SqliteConnectionString));

// Configure demo settings
builder.Services.AddSingleton<DemoSettings>();

// Register HttpClient for InventoryClient
builder.Services.AddHttpClient<InventoryClient>();

// Register repositories
builder.Services.AddScoped<CatalogRepository>();
builder.Services.AddScoped<CustomerRepository>();
builder.Services.AddScoped<OrderRepository>();

// Register infrastructure services
builder.Services.AddScoped<InventoryClient>();

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
