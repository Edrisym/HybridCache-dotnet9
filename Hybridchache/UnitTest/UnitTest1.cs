using System.Net;
using Hybridchache;
using Hybridchache.Wrapper;
using Microsoft.Extensions.Caching.Hybrid;
using Moq;
using Moq.Protected;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

public class WeatherServiceTests
{
    private readonly Mock<IHybridCacheWrapper> _hybridCacheMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly WeatherService _weatherService;

    public WeatherServiceTests()
    {
        _hybridCacheMock = new Mock<IHybridCacheWrapper>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _weatherService = new WeatherService(_hybridCacheMock.Object, _httpClientFactoryMock.Object);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_ShouldReturnWeatherResponse_WhenCityExists()
    {
        var city = "Tehran";
        var cacheKey = $"weather:{city}";

        var weatherResponse = new WeatherResponse
        {
            Id = 1,
            Name = city,
            Weather = [new Weather { Id = 800, Main = "Clear", Description = "clear sky", Icon = "01d" }],
            Base = "stations",
            Visibility = 10000,
            Timezone = 16200
        };

        _hybridCacheMock.Setup(x => x.GetOrCreateAsync(
            It.Is<string>(k => k == cacheKey),
            It.IsAny<Func<CancellationToken, ValueTask<WeatherResponse?>>>(),
            It.IsAny<HybridCacheEntryOptions>(),
            It.IsAny<IReadOnlyCollection<string>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(weatherResponse);

        var result = await _weatherService.GetCurrentWeatherAsync(city);

        Assert.IsNotNull(result);
        Assert.AreEqual(city, result.Name);
    }

    [Fact]
    public async Task GetWeatherAsync_ShouldReturnNull_WhenCityDoesNotExist()
    {
        // Arrange
        var city = "InvalidCity";
        var apiKey = "2e73decf9ba4a924a2a7cfd3c2c68ef8";
        var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}";

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsNull<HttpRequestMessage>(),
                ItExpr.IsNull<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);

        _httpClientFactoryMock.Setup(x => x.CreateClient(
                It.IsNotNull<string>()))
            .Returns(httpClient);

        // Act
        var result = await _weatherService.GetCurrentWeatherAsync(city);

        // Assert
        Assert.IsNull(result);
    }
}