using System.Runtime.Caching;

namespace CRM.Web
{
    public static class CacheManager
    {
        private static MemoryCache MemoryCacheObj
        {
            get
            {
                return MemoryCache.Default;
            }
        }
        private static CacheItemPolicy DefaultPolicy
        {
            get
            {
                return new CacheItemPolicy
                {
                    SlidingExpiration = MemoryCache.NoSlidingExpiration
                };
            }
        }
        public static bool IsCacheKeyExist(string key)
        {
            return MemoryCacheObj.Contains(key);
        }
        public static void AddEntry<T>(string key, T entry)
        {
            if (IsCacheKeyExist(key))
            {
                if (entry == null)
                {
                    MemoryCacheObj.Remove(key);
                }
                else
                {
                    MemoryCacheObj.Set(key, entry, DefaultPolicy);
                }
            }
            else if (entry != null)
            {
                MemoryCacheObj.Set(key, entry, DefaultPolicy);
            }
        }

        public static T GetEntry<T>(string key)
        {
            T cachedObject = (T)MemoryCacheObj.Get(key);
            return cachedObject;

        }

        public static void RemoveEntry(string key)
        {
            MemoryCacheObj.Remove(key);
        }

    }
}