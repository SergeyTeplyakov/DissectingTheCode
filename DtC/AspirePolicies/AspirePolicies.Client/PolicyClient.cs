using System.Net.Http.Json;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace AspirePolicies.Web;

public class PolicyClient
{
    private readonly int _id;
    private readonly HttpClient _httpClient;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy = 
        GetRetryPolicy();

    public PolicyClient(int id, HttpClient httpClient)
    {
        _id = id;
        _httpClient = httpClient;
    }

    public async Task<Policy[]?> GetPoliciesAsync()
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            WriteLine($"Getting policies for {_id}.");
            return _httpClient.GetAsync("/policies");
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Policy[]>();

        WriteLine($"Got policies for {_id}.");
        return result;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetExponentialBackoffRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        // From Polly.Contrib.WaitAndRetry
        var delay = Backoff.DecorrelatedJitterBackoffV2(
            medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 5);

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(delay);
    }

    static void WriteLine(string message)
    {
        System.Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {message}");
    }

    public record Policy
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
    }
}


