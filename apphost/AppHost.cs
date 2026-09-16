var builder = DistributedApplication.CreateBuilder(args);

// Add API service
var api = builder.AddProject<Projects.ObservabilityDemo_OrderService>("api");

builder.Build().Run();
