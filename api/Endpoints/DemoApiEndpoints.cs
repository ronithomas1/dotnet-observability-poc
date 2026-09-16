namespace ObservabilityDemo.OrderService.Endpoints;

public static class DemoApiEndpoints
{
    public static IEndpointRouteBuilder MapDemoApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/demo").WithTags("Demo");

        // TODO: Add demo endpoints

        return app;
    }
}
