using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.viewmodels {

    public interface ISalesTransactionsViewModel : IRoutableViewModel {

    }

    public class SalesTransactionsViewModel : ViewModelBase, ISalesTransactionsViewModel {

        public SalesTransactionsViewModel(IScreen screen)
            : base(screen) {

            var svc = tgsdesktop.infrastructure.IocContainer.Resolve<infrastructure.ISalesInvoiceService>();
            this.SalesInvoices = new ReactiveList<SalesInvoiceSummaryViewModel>(svc.GetTransactionSummaries().Select(x => new SalesInvoiceSummaryViewModel(x)));
        }

        public ReactiveList<SalesInvoiceSummaryViewModel> SalesInvoices { get; private set; }

        public class SalesInvoiceSummaryViewModel {
            public SalesInvoiceSummaryViewModel(models.SalesInvoiceSummary invoice) {
                this.Id = invoice.Id;
                this.InvoiceNumber = invoice.InvoiceNumber;
                this.EffectiveDate = invoice.EffectiveDate;
                this.CustomerName = invoice.PersonName;
                this.TaxableSales = invoice.TaxableSales;
                this.NonTaxableSales = invoice.NonTaxableSales;
                this.Discount = invoice.Discounts;
                this.SalesTax = invoice.SalesTax;
                this.Total = invoice.Total;
                this.Refunded = invoice.Refunded;
            }

            public int Id { get; set; }
            public DateTime EffectiveDate { get; set; }
            public string InvoiceNumber { get; set; }
            public string CustomerName { get; set; }
            public decimal TaxableSales { get; set; }
            public decimal NonTaxableSales { get; set; }
            public decimal Discount { get; set; }
            public decimal SalesTax { get; set; }
            public decimal Total { get; set; }
            public decimal Refunded { get; set; }
        }
    }
}
