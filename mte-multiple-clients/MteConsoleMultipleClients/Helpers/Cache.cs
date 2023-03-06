
using System;
using System.Collections.Concurrent;

namespace MteConsoleMultipleClients.Helpers
{
    /// <summary>
    /// Class Cache.
    /// </summary>
    /// <typeparam name="TKey">The type of the t key.</typeparam>
    /// <typeparam name="TValue">The type of the t value.</typeparam>
    public class Cache<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, CacheItem<TValue>> _cache = new ConcurrentDictionary<TKey, CacheItem<TValue>>();

        #region Store
        /// <summary>
        /// Stores the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="expiresAfter">The expires after.</param>
        public void Store(TKey key, TValue value, TimeSpan expiresAfter)
        {
            if (_cache.ContainsKey(key))
            {
                if (!_cache.TryUpdate(key, new CacheItem<TValue>(value, expiresAfter), _cache[key]))
                {
                    throw new ApplicationException($"Could not update MTE state for {key}");
                }
            }
            else
            {
                if (!_cache.TryAdd(key, new CacheItem<TValue>(value, expiresAfter)))
                {
                    throw new ApplicationException($"Could not add MTE state for {key}");
                }
            }   
        }
        #endregion

        #region Get
        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>TValue.</returns>
        public TValue Get(TKey key)
        {
            if (!_cache.ContainsKey(key)) return default;
            var cached = _cache[key];
            if (DateTimeOffset.Now - cached.Created >= cached.ExpiresAfter)
            {
                _cache.TryRemove(key, out CacheItem<TValue> removedValue);
                return default;
            }
            return cached.Value;
        }
        #endregion
    }
}
