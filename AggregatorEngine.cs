using System;

namespace WeatherAggregator;

public class AggregatorEngine
{

    private readonly IEnumerable<IWeatherProvider> _providers;

    //The container automatically finds every IWeatherProvider implementation and gives them to us as a collection
    public AggregatorEngine(IEnumerable<IWeatherProvider> providers)
    {
        _providers = providers;
    }

    public async Task ExecuteAsync(string city)
    {
        Console.WriteLine($"Fetching weather for {city} from {_providers.Count()} providers...");

        // Create a CancellationTokenSource that automatically cancels after 2 seconds
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        //Send all networks requests simultaneously
        var tasks = _providers.Select(provider => provider.GetForecastAsync(city, cts.Token));

        var results = await Task.WhenAll(tasks);

        //Output successes
        var successes = results.Where(result => result.IsSuccess).ToList();
        foreach (var success in successes)
        {
            Console.WriteLine($"[SUCCESS] {success.ProviderName}: {success.TemperatureCelsius}°C");
        }

        var failures = results.Where(result => !result.IsSuccess).ToList();
        foreach (var failure in failures)
        {
            Console.WriteLine($"[FAILED] {failure.ProviderName}: {failure.ErrorMessage}");
        }

        //Calculate the final consensus
        if (successes.Any())
        {
            var average = successes.Average(result => result.TemperatureCelsius);
            Console.WriteLine($"[CONSENSUS] Average temperature for {city}: {average}°C");

        }
        else
        {
            Console.WriteLine("No provider returned a result for this city");
        }
    }

}
