using System;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace WeatherAggregator;

public class OpenMeteoProvider : IWeatherProvider
{
    private readonly IHttpClientFactory _httpClientFactory;

    //Constructor Injection
    public OpenMeteoProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }


    public async Task<WeatherResult> GetForecastAsync(string city, CancellationToken cancellationToken = default)
    {
        try
        {
            //Get a HTTP client
            using var client = _httpClientFactory.CreateClient("WeatherClient");

            //Convert the city name to latitude/longitude with geocoding API
            var geoURL = $"https://geocoding-api.open-meteo.com/v1/search?name={city}&count=1";

            //Wait the network call. GetFromJsonAsync automatically parses the JSON received
            var geoResponse = await client.GetFromJsonAsync<JsonNode>(geoURL, cancellationToken);

            var results = geoResponse?["results"];

            if (results == null)
            {
                return new WeatherResult("Open-Meteo", null, false, "Invalid city name: " + city);
            }

            var latitude = results[0]?["latitude"]?.ToString();
            var longitude = results[0]?["longitude"]?.ToString();

            //Contact the weather API
            var weatherURL = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current_weather=true";

            var weatherResponse = await client.GetFromJsonAsync<JsonNode>(weatherURL);
            var currentWeather = weatherResponse?["current_weather"];

            //Extract the temperature
            double temp = (double)currentWeather["temperature"];

            //Return a weather record
            return new WeatherResult("Open-Meteo", temp, true);
        }
        catch (OperationCanceledException) // Catch the specific timeout exception
        {
            return new WeatherResult("Open-Meteo", null, false, "Request timed out.");
        }
        catch (Exception ex)
        {
            return new WeatherResult("Open-Meteo", null, false, ex.Message);
        }

    }
}
