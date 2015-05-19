using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.infrastructure {
    public interface IGlobalSettingsAccessor {

        int CurrentSeasonId { get; set; }
        decimal SalesTaxRate { get; set; }

        System.Windows.Controls.Control ActiveControl { get; set; }

    }
}
