using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AspirePolicies.Web;

public class WeatherApiClient
{
    private readonly HttpClient _httpClient;

    public WeatherApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WeatherForecast[]?> GetWeatherForecastAsync()
    {
        return await _httpClient.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast");
    }
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
