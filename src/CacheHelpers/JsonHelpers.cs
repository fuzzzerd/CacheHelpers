using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CacheHelpers
{
    public static class JsonHelpers
    {
        public static async Task SetAsync<T>(this IDistributedCache cache, string key, T data)
        {
            var jsonString = JsonConvert.SerializeObject(data);
            var jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            await cache.SetAsync(key, jsonBytes);
        }
       
        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key)
        {
            var jsonBytes = await cache.GetAsync(key);
            var jsonString = Encoding.UTF8.GetString(jsonBytes);
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
}