using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.models {
    public class Transaction {

        public int? Id { get; set; }
        public DateTime EffectiveDate { get; set; }
        public int LineItemId { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal NonTaxableAmount { get; set; }
        public string Memo { get; set; }

    }
}
