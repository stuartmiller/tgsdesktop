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
        public decimal DiscountPercentage { get; set; }
        public int? SeasonId { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string TxnMemo { get; set; }
        public int? PersonId { get; set; }
        public decimal SalesTax { get; set; }
        public List<Payment> Payments { get; private set; }
        public List<AccountPayment> AccountPayments { get; set; }
        public List<Item> Items { get; private set; }

        public class Item {

            public Item(models.Product product = null) {
                if (product != null) {
                    this.Description = product.Name;
                    this.ProductId = product.ProductId;
                    this.ItemId = product.ItemId;
                    this.Price = product.Price.HasValue ? product.Price.Value : 0m;
                    this.Cost = product.Cost;
                    IsTaxable = product.IsTaxable; 
                }
            }

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
