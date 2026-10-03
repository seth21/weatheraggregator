using System;

namespace WeatherAggregator;

public interface IWeatherProvider
{
    //A contract stating that any provider should take a city name and asynchronously return a WeatherResult
    Task<WeatherResult> GetForecastAsync(string city, CancellationToken cancellationToken = default);
}
