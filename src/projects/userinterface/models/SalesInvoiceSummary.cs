using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.models {
    public class SalesInvoiceSummary {

        public SalesInvoiceSummary() {
            this.Items = new List<SalesInvoiceItem>();
        }

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

        public List<SalesInvoiceItem> Items { get; private set; }

        public class SalesInvoiceItem {
            public int Id { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public int Refunded { get; set; }
        }
    }
}
