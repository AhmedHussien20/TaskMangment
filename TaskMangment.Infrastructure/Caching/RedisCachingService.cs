using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using TaskMangment.Application.Common.Interfaces;

namespace TaskMangment.Infrastructure.Caching
{

    public class RedisCachingService : ICachingService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisCachingService> _logger;


        public RedisCachingService(IDistributedCache cache, ILogger<RedisCachingService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {

            try
            {
                var data = await _cache.GetStringAsync(key);
                return data is null ? default : JsonSerializer.Deserialize<T>(data);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogWarning(ex, "Redis down while GET key={Key}", key);
                return default; 
            }
            catch (TimeoutException ex)
            {
                _logger.LogWarning(ex, "Redis timeout while GET key={Key}", key);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis error while GET key={Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                };

                var json = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, json, options);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogWarning(ex, "Redis down while SET key={Key}", key);
            }
            catch (TimeoutException ex)
            {
                _logger.LogWarning(ex, "Redis timeout while SET key={Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis error while SET key={Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogWarning(ex, "Redis down while REMOVE key={Key}", key);
            }
            catch (TimeoutException ex)
            {
                _logger.LogWarning(ex, "Redis timeout while REMOVE key={Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis error while REMOVE key={Key}", key);
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration)
        {
            try
            {
                var cached = await GetAsync<T>(key);
                if (cached is not null) return cached;

               var lockKey = key + ":lock";

                if (await _cache.GetStringAsync(lockKey) is null)
                {
                    await _cache.SetStringAsync(
                        lockKey,
                        "1",
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
                        });

                    try
                    {
                        cached = await GetAsync<T>(key);
                        if (cached is not null) return cached;

                        var value = await factory();
                        await SetAsync(key, value, expiration);
                        return value;
                    }
                    finally
                    {
                        try { await _cache.RemoveAsync(lockKey); } catch { }
                    }
                }

                // someone else is building it => wait a bit and retry (polling)
                var start = DateTime.UtcNow;
                var timeout = TimeSpan.FromSeconds(2);

                while (DateTime.UtcNow - start < timeout)
                {
                    await Task.Delay(50);

                    cached = await GetAsync<T>(key);
                    if (cached is not null) return cached;

                    // lock removed? break and fallback
                    try
                    {
                        if (await _cache.GetStringAsync(lockKey) is null)
                            break;
                    }
                    catch
                    {
                        // if redis died while waiting -> fallback to DB
                        break;
                    }
                }

                // fallback
                var result = await factory();
                await SetAsync(key, result, expiration);
                return result;
            }
            catch
            {
                // ✅ أهم سطر: لو أي Redis exception حصلت، نروح DB مباشرة
                return await factory();
            }
        }

    }
}
