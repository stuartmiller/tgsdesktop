using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.infrastructure.model {
    public class CacheItem {

        public CacheItem() {
            this.Expires = new TimeSpan(1, 0, 0);
        }

        public string Key { get; set; }
        public object Value { get; set; }
        public TimeSpan Expires { get; set; }
    }
}
