using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MteDemoTest.Helpers
{
    /// <summary>
    /// Class Cache.
    /// </summary>
    /// <typeparam name="TKey">The type of the t key.</typeparam>
    /// <typeparam name="TValue">The type of the t value.</typeparam>
    public class Cache<TKey, TValue>
    {
        private readonly Dictionary<TKey, CacheItem<TValue>> _cache = new Dictionary<TKey, CacheItem<TValue>>();

        #region Store
        /// <summary>
        /// Stores the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="expiresAfter">The expires after.</param>
        public void Store(TKey key, TValue value, TimeSpan expiresAfter)
        {
            _cache[key] = new CacheItem<TValue>(value, expiresAfter);
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
                _cache.Remove(key);
                return default;
            }
            return cached.Value;
        }
        #endregion
    }
}
