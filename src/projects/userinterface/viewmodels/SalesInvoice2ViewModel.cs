using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace tgsdesktop.viewmodels {

    public interface ISalesInvoice2ViewModel {
        ReactiveCommand<object> SaveTransaction { get; }
    }

    public class SalesInvoice2ViewModel : ViewModelBase, ISalesInvoice2ViewModel {


        public SalesInvoice2ViewModel(IScreen screen)
            : base(screen) {

            this.InvoiceDate = DateTime.Now;
            this.InvoiceNumber = DateTime.Now.ToString("Hmmssff");

            var salesTaxRate = tgsdesktop.infrastructure.IocContainer.Resolve<infrastructure.IGlobalSettingsAccessor>().SalesTaxRate;

            this.Payments = new ReactiveList<transaction.PaymentViewModel>();
            Payments.Add(new transaction.PaymentViewModel());
            this.Payments.ChangeTrackingEnabled = true;
            this.WhenAnyObservable(vm => vm.Payments.ItemChanged)
                .Select(_ => this.Payments.Where(p => p.IsValid).Sum(p => p.Amount.Value))
                .ToProperty(this, vm => vm.TotalPayments, out _totalPayments);

            // get the stamps item for calculating price
            this.Stamps = tgsdesktop.infrastructure.IocContainer.Resolve<infrastructure.ISalesInvoiceService>().GetItem(100);
            this.WhenAny(
                vm => vm.TaxableAmt,
                vm => vm.StampsQty,
                (t, s) => t.GetValue() + (s.GetValue() * this.Stamps.Price.Value))
                .ToProperty(this, vm => vm.Subtotal, out _subtotal);
            this.WhenAny(
                vm => vm.TaxableAmt,
                vm => vm.DiscountPercentage,
                (a, d) => a.GetValue() * (d.GetValue()/100))
                .ToProperty(this, vm => vm.Discount, out _discount);
            this.WhenAny(
                vm => vm.TaxableAmt,
                vm => vm.Discount,
                (a, d) => decimal.Round((a.GetValue() - d.GetValue()) * salesTaxRate, 2, MidpointRounding.AwayFromZero))
                .ToProperty(this, vm => vm.SalesTax, out _salesTax);
            this.WhenAny(
                vm => vm.Subtotal,
                vm => vm.SalesTax,
                vm => vm.Discount,
                (a, b, d) => (a.GetValue() - d.GetValue()) + b.GetValue())
                .ToProperty(this, vm => vm.Total, out _total);
            this.WhenAnyValue(vm => vm.Total)
                .Subscribe(_ => { if (this.Payments.Count == 1) this.Payments[0].Amount = this.Total; });
            this.WhenAny(
                vm => vm.Total,
                vm => vm.TotalPayments,
                (t, tp) => {
                    var total = t.GetValue();
                    var totalPmt = tp.GetValue();
                    return total > 0 && total == totalPmt;
                })
                .ToProperty(this, vm => vm.InvoiceInBalance, out _invoiceInBalance);

            this.SaveTransaction = ReactiveCommand.Create(
                this.WhenAny(
                    vm => vm.InvoiceNumber,
                    vm => vm.Total,
                    vm => vm.TotalPayments,
                    (i, t, tp) => {
                        var invoiceNo = i.GetValue();
                        var total = t.GetValue();
                        var totalpayments = tp.GetValue();
                        return !string.IsNullOrEmpty(invoiceNo) && total > 0 && total == totalpayments;
                    }));
            this.SaveTransaction.Subscribe(_ => {
                var svc = tgsdesktop.infrastructure.IocContainer.Resolve<tgsdesktop.infrastructure.ISalesInvoiceService>();
                var item = new models.AddSalesInvoiceModel {
                    EffectiveDate = this.InvoiceDate,
                    InvoiceNumber = this.InvoiceNumber,
                    SalesTax = this.SalesTax,
                    TxnMemo = "Invoice " + this.InvoiceNumber
                };
                if (this.TaxableAmt > 0)
                    item.Items.Add(new models.AddSalesInvoiceModel.Item{
                        Description = "Taxable Sales",
                        ItemId = 101,
                        IsTaxable = true,
                        Price = this.TaxableAmt,
                        Discount = this.DiscountPercentage == 0 ? 0 : this.TaxableAmt * (this.DiscountPercentage/100),
                        Quantity = 1
                    });
                if (this.StampsQty > 0)
                    item.Items.Add(new models.AddSalesInvoiceModel.Item {
                        Description = "Stamps",
                        ItemId = this.Stamps.Id,
                        IsTaxable = false,
                        Price = this.Stamps.Price.Value,
                        Quantity = this.StampsQty
                    });
                foreach (var p in this.Payments)
                    item.Payments.Add(new models.AddSalesInvoiceModel.Payment {
                        Amount = p.Amount.Value,
                        CheckNumber = p.CheckNumber,
                        Method = (models.transaction.PaymentMethod)p.PaymentMethod.Key
                    });
                svc.AddSalesInvoice(item);

                this.InvoiceNumber = DateTime.Now.ToString("Hmmssff");
                this.TaxableAmt = 0;
                this.DiscountPercentage = 0;
                this.StampsQty = 0;
                this.Payments.Clear();
                this.Payments.Add(new transaction.PaymentViewModel());
            });

        }

        public ReactiveCommand<object> SaveTransaction { get; private set; }

        models.Product Stamps { get; set; }

        DateTime _invoiceDate;
        public DateTime InvoiceDate {
            get { return _invoiceDate; }
            set { this.RaiseAndSetIfChanged(ref _invoiceDate, value); }
        }

        string _invoiceNumber;
        public string InvoiceNumber {
            get { return _invoiceNumber; }
            set { this.RaiseAndSetIfChanged(ref _invoiceNumber, value); }
        }

        decimal _taxableAmt;
        public decimal TaxableAmt {
            get { return _taxableAmt; }
            set { this.RaiseAndSetIfChanged(ref _taxableAmt, value); }
        }

        int _stampsQty;
        public int StampsQty {
            get { return _stampsQty; }
            set { this.RaiseAndSetIfChanged(ref _stampsQty, value); }
        }

        decimal _discountPercentage;
        public decimal DiscountPercentage {
            get { return _discountPercentage; }
            set { this.RaiseAndSetIfChanged(ref _discountPercentage, value); }
        }

        public ReactiveList<transaction.PaymentViewModel> Payments { get; private set; }

        readonly ObservableAsPropertyHelper<decimal> _discount;
        public decimal Discount { get { return _discount.Value; } }

        readonly ObservableAsPropertyHelper<decimal> _subtotal;
        public decimal Subtotal { get { return _subtotal.Value; } }

        readonly ObservableAsPropertyHelper<decimal> _salesTax;
        public decimal SalesTax { get { return _salesTax.Value; } }

        readonly ObservableAsPropertyHelper<decimal> _total;
        public decimal Total { get { return _total.Value; } }

        readonly ObservableAsPropertyHelper<decimal> _totalPayments;
        public decimal TotalPayments { get { return _totalPayments.Value; } }

        readonly ObservableAsPropertyHelper<bool> _invoiceInBalance;
        public bool InvoiceInBalance { get { return _invoiceInBalance.Value; } }
    }
}
