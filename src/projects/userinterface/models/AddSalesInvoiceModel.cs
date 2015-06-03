using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.models {

    public class AddSalesInvoiceModel {
        public AddSalesInvoiceModel() {
            this.Payments = new List<Payment>();
            this.AccountPayments = new List<AccountPayment>();
            this.Items = new List<Item>();
        }

        public string InvoiceNumber { get; set; }
        public int? SeasonId { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string TxnMemo { get; set; }
        public models.Person Person { get; set; }
        public decimal SalesTax { get; set; }
        public List<Payment> Payments { get; private set; }
        public List<AccountPayment> AccountPayments { get; set; }
        public List<Item> Items { get; private set; }

        public class Item {
            public string Description { get; set; }
            public int? ProductId { get; set; }
            public int? ItemId { get; set; }
            public decimal Price { get; set; }
            public decimal? Cost { get; set; }
            public int Quantity { get; set; }
            public bool IsTaxable { get; set; }
            public decimal Discount { get; set; }
        }
        public class Payment {
            public models.transaction.PaymentMethod Method { get; set; }
            public decimal Amount { get; set; }
            public string CheckNumber { get; set; }
        }
        public class AccountPayment {
            public int PersonId { get; set; }
            public decimal Amount { get; set; }
        }

    }
}
