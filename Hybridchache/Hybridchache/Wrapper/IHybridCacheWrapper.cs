using Microsoft.Extensions.Caching.Hybrid;

namespace Hybridchache.Wrapper;

// public interface IHybridCacheWrapper
// {
//     ValueTask<T> GetOrCreateAsync<T>(
//         string key,
//         Func<CancellationToken, ValueTask<T>> factory,
//         HybridCacheEntryOptions? options = null,
//         IReadOnlyCollection<string>? tags = null,
//         CancellationToken token = default);
// }