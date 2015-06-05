using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.models {
    public class CustomerTransactionSummary {

        public int PersonId { get; set; }
        public int TransactionId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsCredit { get; set; }
        public string Memo { get; set; }

    }

    public class CustomerInvoiceSummary : CustomerTransactionSummary {

        public CustomerInvoiceSummary()
            : base() {
            this.Items = new List<Item>();
        }

        public int Id { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal NontaxableAmount { get; set; }
        public int DiscountPercentage { get; set; }
        public decimal SalesTax { get; set; }

        public List<Item> Items { get; private set; }

        public class Item {
            public int Id { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public decimal Discount { get; set; }
            public bool IsTaxable { get; set; }
            public decimal Total { get; set; }
        }
    }
}
