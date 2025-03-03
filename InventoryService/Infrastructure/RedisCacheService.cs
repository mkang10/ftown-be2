using Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IDatabase = StackExchange.Redis.IDatabase;

namespace Infrastructure
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _cacheDb;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _cacheDb = redis.GetDatabase();
        }

        public async Task<T?> GetCacheAsync<T>(string key)
        {
            var cachedData = await _cacheDb.StringGetAsync(key);
            if (!cachedData.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<T>(cachedData);
            }
            return default;
        }

        public async Task SetCacheAsync<T>(string key, T value, TimeSpan expiration)
        {
            var jsonData = JsonSerializer.Serialize(value);
            await _cacheDb.StringSetAsync(key, jsonData, expiration);
        }

        public async Task RemoveCacheAsync(string key)
        {
            await _cacheDb.KeyDeleteAsync(key);
        }
    }
}
