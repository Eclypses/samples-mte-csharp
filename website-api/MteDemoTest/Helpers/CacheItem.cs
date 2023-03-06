using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MteDemoTest.Helpers
{
    public class CacheItem<T>
    {
        /// <summary>
        /// The cache Item 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="expiresAfter"></param>
        public CacheItem(T value, TimeSpan expiresAfter)
        {
            Value = value;
            ExpiresAfter = expiresAfter;
        }
        public T Value { get; }
        internal DateTimeOffset Created { get; } = DateTimeOffset.Now;
        internal TimeSpan ExpiresAfter { get; }
    }
}
