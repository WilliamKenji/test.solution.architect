using consumer.financial.transactions.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace consumer.financial.transactions.Infrastructure.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            var cachedData = await _cache.GetStringAsync(key, ct);
            if (cachedData == null)
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(cachedData);
            }
            catch (JsonException)
            {
                await RemoveAsync(key, ct);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
        {
            if (value == null)
                return;

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromDays(1)
            };

            var jsonData = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, jsonData, options, ct);
        }

        public async Task RemoveAsync(string key, CancellationToken ct = default)
        {
            await _cache.RemoveAsync(key, ct);
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
        {
            return await _cache.GetAsync(key, ct) != null;
        }
    }
}
