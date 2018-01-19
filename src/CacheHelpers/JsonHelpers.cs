using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CacheHelpers
{
    public static class JsonHelpers
    {
        /// <summary>
        /// Adds an object to the <see cref="IDistributedCache"/> instance after serializing it to Json and storing its UTF8.GetBytes.
        /// </summary>
        /// <typeparam name="T">The Type of the object to persist.</typeparam>
        /// <param name="cache">The instance of the cache object to perform the operation on.</param>
        /// <param name="key">The key to store this object under.</param>
        /// <param name="data">The actual object to persist in the cache.</param>
        public static Task SetAsync<T>(this IDistributedCache cache, string key, T data)
        {
            return cache.SetAsync(key, data, new DistributedCacheEntryOptions());
        }

        /// <summary>
        /// Adds an object to the <see cref="IDistributedCache"/> instance after serializing it to Json and storing its UTF8.GetBytes.
        /// </summary>
        /// <typeparam name="T">The Type of the object to persist.</typeparam>
        /// <param name="cache">The instance of the cache object to perform the operation on.</param>
        /// <param name="key">The key to store this object under.</param>
        /// <param name="data">The actual object to persist in the cache.</param>
        /// <param name="expireFromNow">Timnespan expressing the amount of time in the future this item expires.</param>
        public static Task SetAsync<T>(this IDistributedCache cache, string key, T data, TimeSpan expireFromNow)
        {
            return cache.SetAsync(key, data, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = expireFromNow });
        }

        /// <summary>
        /// Adds an object to the <see cref="IDistributedCache"/> instance after serializing it to Json and storing its UTF8.GetBytes.
        /// </summary>
        /// <typeparam name="T">The Type of the object to persist.</typeparam>
        /// <param name="cache">The instance of the cache object to perform the operation on.</param>
        /// <param name="key">The key to store this object under.</param>
        /// <param name="data">The actual object to persist in the cache.</param>
        /// <param name="options">Distributed cache options to specify on this entry.</param>
        public static Task SetAsync<T>(this IDistributedCache cache, string key, T data, DistributedCacheEntryOptions options)
        {
            var jsonString = JsonConvert.SerializeObject(data);
            var jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            return cache.SetAsync(key, jsonBytes, options);
        }

        /// <summary>
        /// Get an entry from the cache and converts its byte[] json back to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to return from the cache.</typeparam>
        /// <param name="cache">The cache instance to pull data from.</param>
        /// <param name="key">The key to pull back.</param>
        /// <returns>An instance of the type of object returned or 'default(T).</returns>
        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key)
        {
            var jsonBytes = await cache.GetAsync(key);
            if (jsonBytes == null) return default(T);
            var jsonString = Encoding.UTF8.GetString(jsonBytes);
            if (string.IsNullOrEmpty(jsonString)) return default(T);
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        /// <summary>
        /// Gets an instance from the cache, or uses the provided factory to load the instance, store it in the cache and return the value.
        /// </summary>
        /// <typeparam name="T">Type of object to pull from the cache.</typeparam>
        /// <param name="cache">The instance of the cache object to pull from.</param>
        /// <param name="key">The key to pull.</param>
        /// <param name="factory">The factory function used to load the value if it isn't already in the cache.</param>
        /// <returns>An instance of the type of object.</returns>
        public static async Task<T> GetOrCreateAsync<T>(this IDistributedCache cache, string key, Func<T> factory)
        {
            var cached = await cache.GetAsync<T>(key);
            if (cache == null)
            {
                cached = factory();
                await cache.SetAsync(key, cached);
            }
            return cached;
        }

        /// <summary>
        /// Gets an instance from the cache, or uses the provided factory to load the instance, store it in the cache and return the value.
        /// </summary>
        /// <typeparam name="T">Type of object to pull from the cache.</typeparam>
        /// <param name="cache">The instance of the cache object to pull from.</param>
        /// <param name="key">The key to pull.</param>
        /// <param name="factory">The factory function used to load the value if it isn't already in the cache.</param>
        /// <returns>An instance of the type of object.</returns>
        public static async Task<T> GetOrCreateAsync<T>(this IDistributedCache cache, string key, Func<Task<T>> factory)
        {
            var cached = await cache.GetAsync<T>(key);
            if(cache == null)
            {
                cached = await factory();
                await cache.SetAsync(key, cached);
            }
            return cached;
        }
    }
}