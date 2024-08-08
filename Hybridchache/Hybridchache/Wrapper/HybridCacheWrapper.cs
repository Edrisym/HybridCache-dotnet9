using Microsoft.Extensions.Caching.Hybrid;

namespace Hybridchache.Wrapper;

public class HybridCacheWrapper : IHybridCacheWrapper
{
    private readonly HybridCache _hybridCache;

    public HybridCacheWrapper(HybridCache hybridCache)
    {
        _hybridCache = hybridCache;
    }

    public ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryOptions? options = null,
        IReadOnlyCollection<string>? tags = null,
        CancellationToken token = default)
    {
        return _hybridCache.GetOrCreateAsync(key, factory, options, tags, token);
    }
}