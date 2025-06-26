using System.Diagnostics.Metrics;

namespace AspirePolicies.ApiService;

public record Policy
{
    public required string Name { get; init; }
    public string? Action { get; init; }
}

public class PolicyService
{
    public const string MeterName = "aspire_policies.api";
    private static readonly Meter meter = new(MeterName);
    private static readonly UpDownCounter<int> PendingGetPoliciesRequests = 
        meter.CreateUpDownCounter<int>("aspire_policies.api.pending.policies.requests");

    private long pendingRequests;

    public async Task<Policy[]> GetPoliciesAsync()
    {
        try
        {
            var atStart = Interlocked.Increment(ref pendingRequests);
            PendingGetPoliciesRequests.Add(1);

            await Task.Delay(500);
            var pending = Interlocked.Read(ref pendingRequests);
            await Task.Delay(100);
            
            if (pending >= 5)
            {
                throw new InvalidOperationException($"The backend crashes due to too many requests. Pending: {pending}.");
            }

            return
            [
                new Policy() { Name = "Policy1", Action = "The first policy" },
                new Policy() { Name = "Policy2", Action = "The second policy" }
            ];
        }
        finally
        {
            PendingGetPoliciesRequests.Add(-1);
            Interlocked.Decrement(ref pendingRequests);
        }
    }
}