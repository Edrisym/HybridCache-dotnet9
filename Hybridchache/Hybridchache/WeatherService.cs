using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Hybridchache;

public class WeatherService(IHttpClientFactory httpClientFactory, IDistributedCache distributedCache)
{
    public async Task<WeatherResponse?> GetCurrentWeatherAsync(string city)
    {
        var cacheKey = GetCacheKey(city);

        var cachedWeather = await GetWeatherFromCacheAsync(cacheKey);
        if (cachedWeather != null) return cachedWeather;

        var weatherResponse = await GetWeatherFromApiAsync(city);
        await CacheWeatherAsync(cacheKey, weatherResponse);

        return weatherResponse;
    }

    private string GetCacheKey(string city) => $"weather:{city}";

    private async Task<WeatherResponse?> GetWeatherFromCacheAsync(string cacheKey)
    {
        var cachedWeatherJson = await distributedCache.GetAsync(cacheKey);
        return cachedWeatherJson != null
            ? JsonSerializer.Deserialize<WeatherResponse>(cachedWeatherJson)
            : null;
    }

    private async Task CacheWeatherAsync(string cacheKey, WeatherResponse weatherResponse)
    {
        var value = JsonSerializer.Serialize(weatherResponse);
        await distributedCache.SetStringAsync(cacheKey,
            value,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
            });
    }

    private async Task<WeatherResponse?> GetWeatherFromApiAsync(string cityName)
    {
        var apiKey = "2e73decf9ba4a924a2a7cfd3c2c68ef8";
        var url = $"https://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={apiKey}";

        var httpClient = httpClientFactory.CreateClient();
        var weatherResponse = await httpClient.GetAsync(url);

        if (weatherResponse.StatusCode == HttpStatusCode.NotFound)
        {
            // Handle case where weather data couldn't be fetched
            return null;
        }

        return await weatherResponse.Content.ReadFromJsonAsync<WeatherResponse?>();
    }
}