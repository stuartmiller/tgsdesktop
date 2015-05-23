using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.infrastructure {
    public class Cache : ICacheProvider {

        protected MemoryCache _cache = new MemoryCache("CachingProvider");
        static readonly object padlock = new object();

        public void AddItem(model.CacheItem item) {
            lock (padlock) {
                _cache.Add(item.Key, item.Value, DateTime.Now.Add(item.Expires));
            }
        }

        public void AddItems(IEnumerable<model.CacheItem> items) {
            lock (padlock) {
                foreach (var item in items)
                    this.AddItem(item);
            }
        }

        public void RemoveItem(string key) {
            lock (padlock) {
                _cache.Remove(key);
            }
        }


        public T GetItem<T>(string key, bool remove = false) {
            lock (padlock) {
                var res = _cache[key];

                if (res != null && remove)
                    _cache.Remove(key);

                return res == null ? default(T) : (T)res;
            }
        }
        public List<T> GetItems<T>(IEnumerable<string> keys) {
            lock (padlock) {
                var retVal = _cache.GetValues(keys);

                return retVal == null ? new List<T>() : retVal.Select(x => (T)x.Value).ToList();
            }
        }

    }
}
