using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.models {
    public class Settings {

        public int CurrentSeasonId { get; set; }

        public decimal SalesTaxRate { get; set; }

        public DateTime LastCmRefresh { get; set; }
    }
}
