using Microsoft.Extensions.Caching.Memory;

namespace MarbleWebProject.Helper
{
    public class CacheHelper
    {
        private static IMemoryCache _memoryCache;
        private static Dictionary<string, bool> _allKeys = new Dictionary<string, bool>();
        public CacheHelper(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;

        }
        static IMemoryCache MCache
        {
            get
            {

                return _memoryCache;
            }
            set
            {
                _memoryCache = value;
            }
        }

        public static void SetIMemoryCache(IMemoryCache cache)
        {
            MCache = cache;
        }
        public void SetCache(string key, object value)
        {
            _allKeys.Add(key, true);
            _memoryCache.Set(key, value, new DateTimeOffset(
               DateTime.Now.AddHours(24)));
        }
        public void RemoveCahce()
        {
            var ListKey = new List<string>() { "SERVICEFE", "CONTRACTE", "BLACKLIST" };
            foreach (var item in _allKeys)
            {
                if (ListKey.Contains(item.Key.Substring(0, 9)))
                {
                    _memoryCache.Remove(item.Key);
                    _allKeys.Remove(item.Key);
                }
            }
        }
        public void RemoveCahceAll()
        {
            foreach (var item in _allKeys)
            {
                _memoryCache.Remove(item.Key);
                _allKeys.Remove(item.Key);
            }
        }
        public object GetCache(string key)
        {
            return _memoryCache.Get(key);
        }
        public bool IfCache(string Key)
        {
            bool IfReturn = false;
            var Exsist = _memoryCache.Get(Key);
            if (Exsist != null)
            {
                IfReturn = true;
            }
            return IfReturn;
        }
        public T GetOrCreate<T>(string Key, Func<T> action)
        {
            return _memoryCache.GetOrCreate<T>(Key, entry =>
            {
                _allKeys.Add(Key, true);
                entry.SetAbsoluteExpiration(TimeSpan.FromHours(24));
                return action();
            });
        }
    }
}
