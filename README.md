# Multi-Source Weather Aggregator

This tool fetches real-time weather data concurrently from multiple public APIs, handles network failures gracefully, and calculates a consensus forecast.

## Integrated APIs

* **Open-Meteo:** (via Geocoding translation)
* **wttr.in:** (Direct URL parsing)
* **Met.no (Yr.no):** (Demonstrates custom HTTP Headers and `User-Agent` requirements)

## Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/download) or newer.

## How to Run

1. Clone or download the repository.
2. Open a terminal and navigate to the `WeatherAggregator` folder.
3. Run the application:
   ```bash
   dotnet run
   ```
4. Enter a city name when prompted (e.g., `London`, `Tokyo`, `New York`).

## Architecture Overview

The core engine (`AggregatorEngine.cs`) never knows *which* APIs it is calling. It simply requests an `IEnumerable<IWeatherProvider>` from the DI container. To add a new weather API, you only need to create a class that implements `IWeatherProvider` and register it in `Program.cs`. The engine will automatically pick it up, execute it concurrently, and include it in the final calculation without a single line of core logic needing to change.