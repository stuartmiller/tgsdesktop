using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.infrastructure {
    public interface ICacheProvider {

        void AddItem(model.CacheItem item);
        void AddItems(IEnumerable<model.CacheItem> items);
        T GetItem<T>(string key, bool remove = false);
        List<T> GetItems<T>(IEnumerable<string> keys);
        void RemoveItem(string key);

    }
}
