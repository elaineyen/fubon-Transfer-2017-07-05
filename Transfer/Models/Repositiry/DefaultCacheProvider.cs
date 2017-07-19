using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using Transfer.Models.Interface;

namespace Transfer.Models.Repositiry
{
    public class DefaultCacheProvider : ICacheProvider
    {
        public ObjectCache Cache { get { return MemoryCache.Default; } }

        public object Get(string key)
        {
            return Cache[key];
        }

        public void Invalidate(string key)
        {
            Cache.Remove(key);
        }

        public bool IsSet(string key)
        {
            return (Cache[key] != null);
        }

        public void Set(string key, object data, int cacheTime)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheTime);
            Cache.Add(new CacheItem(key, data), policy);
        }
    }
}