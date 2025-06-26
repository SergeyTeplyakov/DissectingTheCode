using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Register the background metric emitter service
builder.Services.AddHostedService<MetricEmitterService>();

var app = builder.Build();

app.Run();

// BackgroundService implementation
public class MetricEmitterService : BackgroundService
{
    private readonly Meter _meter = new("MyAspireApp.AppHost.Metrics", "1.0.0");
    private readonly Counter<long> _counter;

    public MetricEmitterService()
    {
        _counter = _meter.CreateCounter<long>("background_task_ticks");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("Emitting a metric...");
            _counter.Add(1);
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
