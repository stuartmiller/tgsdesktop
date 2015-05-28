using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.models {
    public class SalesInvoiceSummary {

        public int Id { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string InvoiceNumber { get; set; }
        public int? PersonId { get; set; }
        public string PersonName { get; set; }
        public decimal TaxableSales { get; set; }
        public decimal NonTaxableSales { get; set; }
        public decimal Discounts { get; set; }
        public decimal SalesTax { get; set; }
        public decimal Total { get; set; }
        public decimal Refunded { get; set; }
    }
}
