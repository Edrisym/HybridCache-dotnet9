using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


var summary = BenchmarkRunner.Run<CachingBenchMark>();

public class CachingBenchMark
{
    public const string CacheKey = $"cacheKey:info";

    private readonly IDistributedCache _distributedCache;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMemoryCache _memoryCache;
    private readonly HybridCache _hybridCache;
    private readonly IConfiguration _configuration;

    public CachingBenchMark()
    {
        // var configuration = new ConfigurationBuilder()
        //     .AddJsonFile("appsettings.json")
        //     .Build();

        // Set up dependency injection
        var serviceProvider = new ServiceCollection();

        //
        // serviceProvider.AddStackExchangeRedisCache(config =>
        // {
        //     config.Configuration = configuration.GetConnectionString("Redis");
        // });
        serviceProvider.AddDistributedMemoryCache();
        serviceProvider.AddMemoryCache();
        serviceProvider.AddHybridCache();

        _serviceProvider = serviceProvider.BuildServiceProvider();

        _distributedCache = _serviceProvider.GetService<IDistributedCache>();
        _memoryCache = _serviceProvider.GetService<IMemoryCache>();
        _hybridCache = _serviceProvider.GetService<HybridCache>();
        // _configuration = _serviceProvider.GetService<IConfiguration>();
    }

    public async Task<string> GetDataAsync()
    {
        await Task.Delay(50);
        return "SomeValue";
    }

    [Benchmark]
    public async Task<string> NoCache()
    {
        return await GetDataAsync();
    }

    [Benchmark]
    public async Task<string> DistributedCache()
    {
        var cachedData = await _distributedCache.GetStringAsync(CacheKey);
        if (cachedData is not null)
        {
            return cachedData;
        }

        var data = await GetDataAsync();
        await _distributedCache.SetStringAsync(CacheKey, data);
        return data;
    }

    [Benchmark]
    public async Task<string> MemoryCache()
    {
        if (!_memoryCache.TryGetValue(CacheKey, out string data))
        {
            data = await GetDataAsync();
            _memoryCache.Set(CacheKey, data);
        }

        return data;
    }

    [Benchmark]
    public async Task<string> HybridCache()
    {
        return await _hybridCache.GetOrCreateAsync(CacheKey,
            async _ => await GetDataAsync(),
            token: default);
    }
}