using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive;

namespace tgsdesktop.viewmodels {

    public interface ISalesInvoice2ViewModel {
        ReactiveCommand<object> SaveTransaction { get; }
    }

    public class SalesInvoice2ViewModel : ViewModelBase, ISalesInvoice2ViewModel {


        public SalesInvoice2ViewModel(IScreen screen)
            : base(screen) {

            this.InvoiceDate = DateTime.Now;
            this.InvoiceNumber = DateTime.Now.ToString("Hmmssff");

            this.Customers = new ReactiveList<transaction.CustomerViewModel>();
            this.RefreshCustomers();

            var salesTaxRate = tgsdesktop.infrastructure.IocContainer.Resolve<infrastructure.IGlobalSettingsAccessor>().SalesTaxRate;

            this.Payments = new ReactiveList<transaction.PaymentViewModel>();
            this.Payments.ChangeTrackingEnabled = true;
            this.Payments.Add(new transaction.PaymentViewModel());
            this.AccountPayments = new ReactiveList<AccountPaymentViewModel>();
            this.AccountPayments.ChangeTrackingEnabled = true;

            this.WhenAnyObservable(a => a.Payments.ItemChanged)
                .Select(_ => Unit.Default)
                .Merge(this.WhenAnyObservable(a => a.AccountPayments.ItemChanged)
                    .Select(_ => Unit.Default))
                .Select(vm => this.Payments.Where(x => x.Amount.HasValue).Sum(x => x.Amount.Value)
                    + this.AccountPayments.Where(x => x.Amount.HasValue).Sum(x => x.Amount.Value))
                .ToProperty(this, vm => vm.TotalPayments, out _totalPayments);


            this.WhenAnyObservable(vm => vm.Payments.ItemChanged)
                .Subscribe(p => {
                    if (p.PropertyName == "Amount") {
                        if (p.Sender.Amount.HasValue && this.TotalPayments != this.Total && !this.Payments.Any(x => !x.Amount.HasValue || x.Amount.Value == 0))
                            this.Payments.Add(new transaction.PaymentViewModel());
                    }
                });

            this.WhenAnyValue(vm => vm.SelectedCustomer)
                .Subscribe(c => {
                    if (c == null)
                        this.AccountPayments.Clear();
                    else {
                        this.AccountPayments.Add(new AccountPaymentViewModel {
                            AccountName = "On Account",
                            PersonId = c.PersonModel.Id
                        });
                        var parent = c.PersonModel as models.Parent;
                        if (parent != null) {
                            foreach (var camper in parent.Campers) {
                                this.AccountPayments.Add(new AccountPaymentViewModel {
                                    AccountName = camper.FirstName,
                                    PersonId = camper.Id
                                });
                            }
                        }
                    }
                });

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

            this.WhenAnyObservable(vm => vm.AccountPayments.ItemChanged)
                .Subscribe(ap => {
                    if (ap.Sender.Amount.HasValue && ap.Sender.Amount.Value == this.Total) {
                        this.Payments.Clear();
                        foreach (var x in this.AccountPayments)
                            if (x != ap.Sender)
                                x.Amount = null;
                    }

                });

            this.WhenAnyValue(vm => vm.TotalPayments)
                .Subscribe(_ => {
                    if (this.TotalPayments > 0 && this.TotalPayments == this.Total) {
                        var zeroAmtPmts = this.Payments.Where(x => !x.Amount.HasValue || x.Amount.Value == 0).ToArray();
                        this.Payments.RemoveAll(zeroAmtPmts);
                    } else if (this.TotalPayments > 0 && this.TotalPayments < this.Total && !this.Payments.Any(x => !x.Amount.HasValue || x.Amount.Value == 0))
                        this.Payments.Add(new transaction.PaymentViewModel());
                });

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
                    TxnMemo = "Invoice " + this.InvoiceNumber,
                    DiscountPercentage = this.DiscountPercentage > 0 ? (this.DiscountPercentage / 100) : 0
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
                        ProductId = this.Stamps.ProductId,
                        ItemId = this.Stamps.ItemId,
                        IsTaxable = false,
                        Price = this.Stamps.Price.Value,
                        Quantity = this.StampsQty
                    });
                foreach (var p in this.Payments.Where(x => x.Amount.HasValue && x.Amount.Value > 0))
                    item.Payments.Add(new models.AddSalesInvoiceModel.Payment {
                        Amount = p.Amount.Value,
                        CheckNumber = p.CheckNumber,
                        Method = (models.transaction.PaymentMethod)p.PaymentMethod.Key
                    });
                foreach (var p in this.AccountPayments.Where(x => x.Amount.HasValue && x.Amount.Value > 0))
                    item.AccountPayments.Add(new models.AddSalesInvoiceModel.AccountPayment {
                        PersonId = p.PersonId,
                        Amount = p.Amount.Value
                    });
                if (SelectedCustomer != null) {
                    item.PersonId = this.SelectedCustomer.PersonModel.Id;
                }
                svc.AddSalesInvoice(item);

                this.InvoiceNumber = DateTime.Now.ToString("Hmmssff");
                this.SelectedCustomer = null;
                this.TaxableAmt = 0;
                this.DiscountPercentage = 0;
                this.StampsQty = 0;
                this.Payments.Clear();
                this.Payments.Add(new transaction.PaymentViewModel());
            });

        }

        public ReactiveCommand<object> SaveTransaction { get; private set; }
        public ReactiveList<tgsdesktop.viewmodels.transaction.CustomerViewModel> Customers { get; private set; }

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

        tgsdesktop.viewmodels.transaction.CustomerViewModel _selectedCustomer;
        public tgsdesktop.viewmodels.transaction.CustomerViewModel SelectedCustomer {
            get { return _selectedCustomer; }
            set { this.RaiseAndSetIfChanged(ref _selectedCustomer, value); }
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
        public ReactiveList<AccountPaymentViewModel> AccountPayments { get; private set; }

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


        void RefreshCustomers() {
            var accountService = infrastructure.IocContainer.Resolve<infrastructure.IAccountReceivableService>();
            this.Customers.Clear();
            var customers = accountService.GetPeople();

            this.Customers.AddRange(customers.Select(x => new transaction.CustomerViewModel(x as models.Person)));
            this.Customers.Reset();
        }

        public class AccountPaymentViewModel : ReactiveObject {

            public int PersonId { get; set; }
            public string AccountName { get; set; }

            decimal? _amount;
            public decimal? Amount { get { return _amount; } set { this.RaiseAndSetIfChanged(ref _amount, value); } }
        }
    }
}
