using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.models {
    public class Product {

        public int? ProductId { get; set; }
        public int? ItemId { get; set; }
        public string Name { get; set; }
        public decimal? Cost { get; set; }
        public decimal? Price { get; set; }
        public decimal OldPrice { get; set; }
        public decimal? SpecialPrice { get; set; }
        public bool IsTaxable { get; set; }

    }
}
