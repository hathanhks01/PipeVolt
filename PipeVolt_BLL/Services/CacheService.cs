using Microsoft.Extensions.Caching.Memory;
using PipeVolt_BLL.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_BLL.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILoggerService _logger;

        // Thời gian mặc định nếu không truyền vào
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(10);

        // Lưu danh sách key đang có trong cache để RemoveByPrefix
        private readonly HashSet<string> _cacheKeys = new();
        private readonly object _lock = new();

        public CacheService(IMemoryCache cache, ILoggerService logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public T? Get<T>(string key)
        {
            _cache.TryGetValue(key, out T? value);
            return value;
        }

        public void Set<T>(string key, T value, TimeSpan? duration = null)
        {
            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(duration ?? DefaultExpiration)
                .RegisterPostEvictionCallback((k, v, reason, state) =>
                {
                    // Tự động xóa key khỏi danh sách khi cache expire
                    lock (_lock) { _cacheKeys.Remove(k.ToString()!); }
                });

            _cache.Set(key, value, options);

            lock (_lock) { _cacheKeys.Add(key); }
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
            lock (_lock) { _cacheKeys.Remove(key); }
            _logger.LogInformation($"[Cache] Removed key: {key}");
        }

        public void RemoveByPrefix(string prefix)
        {
            List<string> keysToRemove;
            lock (_lock)
            {
                keysToRemove = _cacheKeys
                    .Where(k => k.StartsWith(prefix))
                    .ToList();
            }

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                lock (_lock) { _cacheKeys.Remove(key); }
            }

            _logger.LogInformation($"[Cache] Removed {keysToRemove.Count} keys with prefix: {prefix}");
        }

        // Hàm quan trọng nhất: Get nếu có, nếu không thì query rồi Set
        public async Task<T> GetOrSetAsync<T>(
            string key,
            Func<Task<T>> factory,
            TimeSpan? duration = null)
        {
            if (_cache.TryGetValue(key, out T? cached) && cached != null)
            {
                _logger.LogInformation($"[Cache] HIT: {key}");
                return cached;
            }

            _logger.LogInformation($"[Cache] MISS: {key} → querying DB");
            var result = await factory();
            Set(key, result, duration);
            return result;
        }
    }
}
