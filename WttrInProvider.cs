using System;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace WeatherAggregator;

public class WttrInProvider : IWeatherProvider
{
    private readonly IHttpClientFactory _httpClientFactory;

    public WttrInProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<WeatherResult> GetForecastAsync(string city, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient("WeatherClient");
            
            // wttr.in takes the city directly in the URL
            var url = $"https://wttr.in/{city}?format=j1";
            
            var response = await client.GetFromJsonAsync<JsonNode>(url, cancellationToken);
            
            var currentCondition = response?["current_condition"]?[0];
            if (currentCondition == null)
            {
                return new WeatherResult("wttr.in", null, false, "Could not find current condition data.");
            }

            // wttr.in returns the temperature as a string (e.g. "15"), so we parse it
            var tempString = currentCondition["temp_C"]?.ToString();
            if (double.TryParse(tempString, out double temp))
            {
                return new WeatherResult("wttr.in", temp, true);
            }

            return new WeatherResult("wttr.in", null, false, "Failed to parse temperature value.");
        }
        catch (OperationCanceledException) // Catch the specific timeout exception
        {
            return new WeatherResult("wttr.in", null, false, "Request timed out.");
        }
        catch (Exception ex)
        {
            return new WeatherResult("wttr.in", null, false, ex.Message);
        }
    }
}