using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.models {
    public class SalesInvoice {

        public SalesInvoice() {
            this.Items = new List<SalesInvoiceItem>();
            this.ReturnItems = new List<SalesInvoiceItemReturn>();
        }

        public int Id { get; set; }
        public int TransactionId { get; set; }
        public string InvoiceNumber { get; set; }
        public int? PersonId { get; set; }
        public int? SeasonId { get; set; }
        public decimal SalesTaxRate { get; set; }
        public decimal DiscountPercentage { get; set; }

        public List<SalesInvoiceItem> Items { get; private set; }
        public List<SalesInvoiceItemReturn> ReturnItems { get; private set; }

        public class SalesInvoiceItem {
            public int Id { get; set; }
            public int InvoiceId { get; set; }
            public int? ProductId { get; set; }
            public int? ItemId { get; set; }
            public bool IsTaxable { get; set; }
            public string Description { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal? Cost { get; set; }
            public decimal Discount { get; set; }
        }

        public class SalesInvoiceItemReturn {
            public int Id { get; set; }
            public int InvoiceId { get; set; }
            public int TxnId { get; set; }
            public int ItemId { get; set; }
            public int Quantity { get; set; }
        }
    }
}
