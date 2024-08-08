using System.Globalization;
using System.Net;
using Hybridchache.Wrapper;
using Microsoft.Extensions.Caching.Hybrid;

namespace Hybridchache;

public class WeatherService
{
    private readonly IHybridCacheWrapper _hybridCache;
    private readonly IHttpClientFactory _httpClientFactory;

    public WeatherService(IHybridCacheWrapper hybridCache, IHttpClientFactory httpClientFactory)
    {
        _hybridCache = hybridCache;
        _httpClientFactory = httpClientFactory;
    }


    public async Task<WeatherResponse?> GetCurrentWeatherAsync(string city)
    {
        var cacheKey = $"weather:{city}";

        var weatherJson = await _hybridCache.GetOrCreateAsync<WeatherResponse?>(cacheKey,
            async _ => await GetWeatherAsync(city),
            options: new HybridCacheEntryOptions
            {
                Flags = HybridCacheEntryFlags.DisableDistributedCache,
                Expiration = TimeSpan.FromMinutes(1)
            });

        return weatherJson;
    }

    private async Task<WeatherResponse?> GetWeatherAsync(string cityName)
    {
        var apiKey = "2e73decf9ba4a924a2a7cfd3c2c68ef8";
        var url = $"https://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={apiKey}";

        var httpClient = _httpClientFactory.CreateClient();
        var weatherResponse = await httpClient.GetAsync(url);

        if (weatherResponse.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        return await weatherResponse.Content.ReadFromJsonAsync<WeatherResponse?>();
    }
}