namespace WeatherAggregator;

public record WeatherResult
(
    string ProviderName,
    double? TemperatureCelsius,
    bool IsSuccess,
    string? ErrorMessage = null
);
