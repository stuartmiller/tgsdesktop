using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Reactive.Linq;

namespace tgsdesktop.viewmodels {

    public interface ISalesInvoiceCheckoutViewModel : IRoutableViewModel {
        ReactiveCommand<object> Save { get; }
        ReactiveCommand<object> Cancel { get; }
    }

    public class SalesInviceCheckoutViewModel : ViewModelBase, ISalesInvoiceCheckoutViewModel {

        public SalesInviceCheckoutViewModel(IScreen screen, SalesInvoiceViewModel invoice)
            : base(screen) {

            this.InvoiceViewModel = invoice;

            //this.CurrentPayment = 
            this.Payments = new ReactiveList<transaction.PaymentViewModel>();
            this.Payments.ChangeTrackingEnabled = true;
            this.WhenAnyObservable(vm => vm.Payments.ItemChanged)
                .Select(_ => this.Payments.Where(x => x.Amount.HasValue).Sum(x => x.Amount.Value))
                .ToProperty(this, vm => vm.PaymentTotal, out _paymentTotal);
            this.PaymentMethods = new ReactiveList<KeyValuePair<int, string>> {
                models.transaction.PaymentMethod.Undefined.GetPaymentMethodKeyValuePair(),
                models.transaction.PaymentMethod.AmEx.GetPaymentMethodKeyValuePair(),
                models.transaction.PaymentMethod.Visa.GetPaymentMethodKeyValuePair(),
                models.transaction.PaymentMethod.Check.GetPaymentMethodKeyValuePair(),
                models.transaction.PaymentMethod.Cash.GetPaymentMethodKeyValuePair(),
                models.transaction.PaymentMethod.MasterCard.GetPaymentMethodKeyValuePair(),
                models.transaction.PaymentMethod.Discover.GetPaymentMethodKeyValuePair()
            };
            var pvm = new transaction.PaymentViewModel { Amount = invoice.Total, PaymentMethod = this.PaymentMethods[0] };
            pvm.PaymentMethods.AddRange(this.PaymentMethods);
            this.Payments.Add(pvm);
            //if (invoice.Person != null && (invoice.Person.IsCamper || invoice.Person.IsStaff)){
            //    this.CurrentPayment.PaymentMethod = models.transaction.PaymentMethod.Account.GetPaymentMethodKeyValuePair();
            //    this.PaymentMethods.Insert(1, this.CurrentPayment.PaymentMethod);
            //}
            //this.CurrentPayment.Amount = invoice.Items.Sum(x => x.Price) - invoice.Items.Sum(x => x.Discount) + invoice.SalesTax;

            this.WhenAnyValue(vm => vm.CurrentPayment)
                .Select(x => x != null && x.PaymentMethod.Key == (int)models.transaction.PaymentMethod.Check ? true : false)
                .ToProperty(this, x => x.CheckNumberEnabled, out _checkNumberEnabled);

            this.SaveButtonVisibility = Visibility.Visible;
            this.PrintButtonVisibility = Visibility.Collapsed;

            this.Save = ReactiveCommand.Create(this.WhenAnyValue(vm => vm.PaymentTotal)
                .Select(pt => pt == this.InvoiceViewModel.Total));
            this.Save.Subscribe(_ => {

                this.SaveTransaction();

                PrintButtonVisibility = Visibility.Visible;
                SaveButtonVisibility = Visibility.Collapsed;
                //this.HostScreen.Router.NavigateAndReset.Execute(new SalesInvoiceViewModel(HostScreen));
            });

            this.Cancel = ReactiveCommand.Create();
            this.Cancel.Subscribe(_ => this.HostScreen.Router.NavigateBack.Execute(null));
            this.NewInvoice = ReactiveCommand.Create();
            this.NewInvoice.Subscribe(_ => this.HostScreen.Router.NavigateAndReset.Execute(new SalesInvoiceViewModel(HostScreen)));
            this.Print = ReactiveCommand.Create();
            this.Print.Subscribe(_ => {

                
                reporting.SalesInvoice salesInvoice = new reporting.SalesInvoice();
                salesInvoice.ReportParameters[0].Value = this.InvoiceModel.Id;
                var processor = new Telerik.Reporting.Processing.ReportProcessor();
                var reportSource = new Telerik.Reporting.InstanceReportSource();
                reportSource.ReportDocument = salesInvoice;
                processor.PrintReport(reportSource, new System.Drawing.Printing.PrinterSettings());
            });

        }

        public models.SalesInvoice InvoiceModel;
        public SalesInvoiceViewModel InvoiceViewModel { get; private set; }

        public ReactiveList<KeyValuePair<int, string>> PaymentMethods { get; private set; }
        public ReactiveList<transaction.PaymentViewModel> Payments { get; private set; }

        transaction.PaymentViewModel _currentPayment;
        public transaction.PaymentViewModel CurrentPayment {
            get { return _currentPayment; }
            set { this.RaiseAndSetIfChanged(ref _currentPayment, value); }
        }

        public ReactiveCommand<object> Save { get; private set; }
        public ReactiveCommand<object> Cancel { get; private set; }
        public ReactiveCommand<object> NewInvoice { get; private set; }
        public ReactiveCommand<object> Print { get; private set; }

        Visibility _printButtonVisibility;
        public Visibility PrintButtonVisibility {
            get { return _printButtonVisibility; }
            set { this.RaiseAndSetIfChanged(ref _printButtonVisibility, value); }
        }

        Visibility _saveButtonVisibility;
        public Visibility SaveButtonVisibility {
            get { return _saveButtonVisibility; }
            set { this.RaiseAndSetIfChanged(ref _saveButtonVisibility, value); }
        }


        private void SaveTransaction() {


            var invoice = new models.AddSalesInvoiceModel {
                Person = this.InvoiceViewModel.SelectedCustomer == null ? null : this.InvoiceViewModel.SelectedCustomer.PersonModel,
                InvoiceNumber = this.InvoiceViewModel.InvoiceNumber,
                SalesTax = this.InvoiceViewModel.SalesTax,
                EffectiveDate = this.InvoiceViewModel.EffectiveDate,
                SeasonId = this.InvoiceViewModel.SeasonId
            };
            var items = this.InvoiceViewModel.Items.Select(i => new models.AddSalesInvoiceModel.Item {
                Cost = i.UnitCost,
                Price = i.UnitPrice.Value,
                Description = i.Description,
                Discount = i.Discount,
                IsTaxable = i.IsTaxable,
                ItemId = i.ItemId,
                ProductId = i.ProductId,
                Quantity = i.Quantity.Value
            });
            invoice.Items.AddRange(items);

            var posService = infrastructure.IocContainer.Resolve<infrastructure.IPointOfSaleService>();
            this.InvoiceModel = posService.AddSalesInvoice(invoice);
        }


        readonly ObservableAsPropertyHelper<decimal> _paymentTotal;
        public Decimal PaymentTotal {
            get { return _paymentTotal.Value; }
        }

        readonly ObservableAsPropertyHelper<bool> _checkNumberEnabled;
        public bool CheckNumberEnabled {
            get { return _checkNumberEnabled.Value; }
        }
    }
}
