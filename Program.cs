using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using WeatherAggregator;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    // 1. Give the client a NAME. Now it returns the Builder!
    services.AddHttpClient("WeatherClient")
            .AddTransientHttpErrorPolicy(policyBuilder => 
                policyBuilder.WaitAndRetryAsync(
                    3, 
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"[WARNING] Network error. Retrying in {timespan.TotalSeconds}s...");
                    }
                )); 
    
    services.AddTransient<IWeatherProvider, OpenMeteoProvider>();
    services.AddTransient<IWeatherProvider, WttrInProvider>();
    services.AddTransient<IWeatherProvider, MetNoProvider>();

    services.AddTransient<AggregatorEngine>();
});


using var host = builder.Build();

var engine = host.Services.GetRequiredService<AggregatorEngine>();

Console.Write("Enter a city name: ");
var city = Console.ReadLine();

if (!string.IsNullOrWhiteSpace(city))
{
    await engine.ExecuteAsync(city);
}