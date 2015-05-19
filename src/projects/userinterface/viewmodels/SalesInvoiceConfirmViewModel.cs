using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Reactive.Linq;

namespace tgsdesktop.viewmodels {

    public interface ISalesInvoiceConfirmViewModel : IRoutableViewModel {
        ReactiveCommand<object> Save { get; }
        ReactiveCommand<object> Cancel { get; }
    }

    public class SalesInvoiceConfirmViewModel : ViewModelBase, ISalesInvoiceConfirmViewModel {

        public SalesInvoiceConfirmViewModel(IScreen screen, models.AddSalesInvoiceModel invoice)
            : base(screen) {

            this.AddSalesInvoiceModel = invoice;
            //this.CurrentPayment = 
            this.Payments = new ReactiveList<transaction.PaymentViewModel>();
            this.Payments.Add(new transaction.PaymentViewModel());
            this.PaymentMethods = new ReactiveList<KeyValuePair<int, string>> {
                models.transaction.PaymentMethod.Undefined.GetPaymentMethodKeyValuePair(),
                models.transaction.PaymentMethod.AmEx.GetPaymentMethodKeyValuePair(),
                models.transaction.PaymentMethod.Visa.GetPaymentMethodKeyValuePair(),
                models.transaction.PaymentMethod.Check.GetPaymentMethodKeyValuePair(),
                models.transaction.PaymentMethod.Cash.GetPaymentMethodKeyValuePair(),
                models.transaction.PaymentMethod.MasterCard.GetPaymentMethodKeyValuePair(),
                models.transaction.PaymentMethod.Discover.GetPaymentMethodKeyValuePair()
            };
            if (invoice.Person != null && (invoice.Person.IsCamper || invoice.Person.IsStaff)){
                this.CurrentPayment.PaymentMethod = models.transaction.PaymentMethod.Account.GetPaymentMethodKeyValuePair();
                this.PaymentMethods.Insert(1, this.CurrentPayment.PaymentMethod);
            }
            this.CurrentPayment.Amount = invoice.Items.Sum(x => x.Price) - invoice.Items.Sum(x => x.Discount) + invoice.SalesTax;

            this.WhenAnyValue(vm => vm.CurrentPayment)
                .Select(x => x.PaymentMethod.Key == (int)models.transaction.PaymentMethod.Check ? true : false)
                .ToProperty(this, x => x.CheckNumberEnabled, out _checkNumberEnabled);

            this.SaveButtonVisibility = Visibility.Visible;
            this.PrintButtonVisibility = Visibility.Collapsed;

            this.Save = ReactiveCommand.Create();
            this.Save.Subscribe(_ => {

                this.SaveTransaction();

                PrintButtonVisibility = Visibility.Visible;
                SaveButtonVisibility = Visibility.Collapsed;
                //this.HostScreen.Router.NavigateAndReset.Execute(new PosRegisterViewModel(HostScreen));
            });

            this.Cancel = ReactiveCommand.Create();
            this.Cancel.Subscribe(_ => this.HostScreen.Router.NavigateBack.Execute(null));
            this.NewInvoice = ReactiveCommand.Create();
            this.NewInvoice.Subscribe(_ => this.HostScreen.Router.NavigateAndReset.Execute(new PosRegisterViewModel(HostScreen)));
            this.Print = ReactiveCommand.Create();
            this.Print.Subscribe(_ => {

                
                reporting.SalesInvoice salesInvoice = new reporting.SalesInvoice();
                salesInvoice.ReportParameters[0].Value = this.SalesInvoice.Id;
                var processor = new Telerik.Reporting.Processing.ReportProcessor();
                var reportSource = new Telerik.Reporting.InstanceReportSource();
                reportSource.ReportDocument = salesInvoice;
                processor.PrintReport(reportSource, new System.Drawing.Printing.PrinterSettings());
            });

        }

        public models.SalesInvoice SalesInvoice { get; private set; }
        public models.AddSalesInvoiceModel AddSalesInvoiceModel { get; private set; }

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
            var posService = infrastructure.IocContainer.Resolve<infrastructure.IPointOfSaleService>();
            var retVal = posService.AddSalesInvoice(this.AddSalesInvoiceModel);
        }

        readonly ObservableAsPropertyHelper<bool> _checkNumberEnabled;
        public bool CheckNumberEnabled {
            get { return _checkNumberEnabled.Value; }
        }
    }
}
