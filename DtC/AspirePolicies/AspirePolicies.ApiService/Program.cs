using AspirePolicies.ApiService;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddOpenTelemetry().WithMetrics(metrics =>
{
    metrics.AddMeter(PolicyService.MeterName);
});

var app = builder.Build();

var service = new PolicyService();
app.MapGet("/policies", () => service.GetPoliciesAsync()).WithName("GetPolicies");

app.MapDefaultEndpoints();

app.Run();

