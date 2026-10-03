using System.Net.Http.Json;
using System.Text.Json.Nodes;
using WeatherAggregator;

public class MetNoProvider : IWeatherProvider
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MetNoProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<WeatherResult> GetForecastAsync(string city, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient("WeatherClient");
            
            // Translate the city into Latitude and Longitude
            var geoUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={city}&count=1";
            var geoResponse = await client.GetFromJsonAsync<JsonNode>(geoUrl, cancellationToken);
            
            var results = geoResponse?["results"];
            if (results == null) 
            {
                return new WeatherResult("Met.no", null, false, "Coordinates not found.");
            }

            var lat = results[0]["latitude"]?.ToString();
            var lon = results[0]["longitude"]?.ToString();

            // Call Met.no Weather API
            var weatherUrl = $"https://api.met.no/weatherapi/locationforecast/2.0/compact?lat={lat}&lon={lon}";
            
            // Instead of GetFromJsonAsync, we build an HttpRequestMessage manually.
            // This allows us to inject the mandatory User-Agent header into the request.
            using var request = new HttpRequestMessage(HttpMethod.Get, weatherUrl);
            request.Headers.Add("User-Agent", "WeatherAggregatorLab/1.0");
            
            // Send the customized request
            using var response = await client.SendAsync(request, cancellationToken);
            
            // If they reject us (e.g., 403 Forbidden), this throws an exception to trigger our catch block
            response.EnsureSuccessStatusCode(); 
            
            // Parse the response
            var weatherJson = await response.Content.ReadFromJsonAsync<JsonNode>(cancellationToken: cancellationToken);
            
            // Navigate the deeply nested JSON tree that Met.no returns
            var currentData = weatherJson?["properties"]?["timeseries"]?[0]?["data"]?["instant"]?["details"];
            if (currentData == null)
            {
                return new WeatherResult("Met.no", null, false, "JSON structure changed.");
            }

            var temp = (double)currentData["air_temperature"];

            return new WeatherResult("Met.no", temp, true);
        }
        catch (OperationCanceledException)
        {
            return new WeatherResult("Met.no", null, false, "Request timed out.");
        }
        catch (Exception ex)
        {
            return new WeatherResult("Met.no", null, false, ex.Message);
        }
    }
}
