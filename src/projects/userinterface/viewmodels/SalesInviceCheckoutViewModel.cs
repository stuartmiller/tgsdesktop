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
            this.SaveButtonVisibility = Visibility.Visible;
            this.PrintButtonVisibility = Visibility.Collapsed;

            //var canExecute = this.WhenAny(vm =>
            //    vm.Payments.
            //    )

            this.Save = ReactiveCommand.Create();
            this.Save.Subscribe(_ => {

                this.SaveTransaction();
                PrintButtonVisibility = Visibility.Visible;
                SaveButtonVisibility = Visibility.Collapsed;
            });

            this.Cancel = ReactiveCommand.Create();
            this.Cancel.Subscribe(_ => this.HostScreen.Router.NavigateBack.Execute(null));
            this.NewInvoice = ReactiveCommand.Create();
            this.NewInvoice.Subscribe(_ => this.HostScreen.Router.NavigateAndReset.Execute(new SalesInvoiceViewModel(HostScreen)));
            this.Print = ReactiveCommand.Create();
            this.Print.Subscribe(_ => {

                try {
                    reporting.SalesInvoice salesInvoice = new reporting.SalesInvoice();
                    var standardPrintController = new System.Drawing.Printing.StandardPrintController();
                    salesInvoice.ReportParameters[0].Value = this.InvoiceModel.Id;
                    var processor = new Telerik.Reporting.Processing.ReportProcessor();
                    processor.PrintController = standardPrintController;
                    var reportSource = new Telerik.Reporting.InstanceReportSource();
                    reportSource.ReportDocument = salesInvoice;
                    processor.PrintReport(reportSource, new System.Drawing.Printing.PrinterSettings());
                } catch (Exception ex) {
                    this.DebugOutput += (Environment.NewLine + ex.Message);
                }

            });

            this.Payments = new ReactiveList<transaction.PaymentViewModel>();
            this.Payments.ChangeTrackingEnabled = true;
            this.WhenAnyObservable(vm => vm.Payments.ItemChanged)
                .Select(_ =>  this.Payments.Where(x => x.Amount.HasValue).Sum(x => x.Amount.Value))
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
            var pvm = new transaction.PaymentViewModel();
            this.Payments.Add(pvm);
            pvm.Amount = invoice.Total;
            if (invoice.SelectedCustomer != null && (invoice.SelectedCustomer.PersonModel.IsCamper || invoice.SelectedCustomer.PersonModel.IsStaff)) {
                pvm.PaymentMethod = models.transaction.PaymentMethod.Account.GetPaymentMethodKeyValuePair();
                this.PaymentMethods.Insert(1, pvm.PaymentMethod);
            }
            pvm.PaymentMethods.AddRange(this.PaymentMethods);
        }

        private string GetDefaultPrinter() {
            System.Drawing.Printing.PrinterSettings printerSettings = new System.Drawing.Printing.PrinterSettings();
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters) {
                printerSettings.PrinterName = printer;
                if (printerSettings.IsDefaultPrinter) {
                    return printerSettings.PrinterName;
                    //return printer;
                }
            }
            return string.Empty;
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
                PersonId = this.InvoiceViewModel.SelectedCustomer == null ? null : (int?)this.InvoiceViewModel.SelectedCustomer.PersonModel.Id,
                InvoiceNumber = this.InvoiceViewModel.InvoiceNumber,
                DiscountPercentage = this.InvoiceViewModel.InvoiceDiscount/100,
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
            var payments = this.Payments.Where(p => p.Amount.HasValue).Select(p => new models.AddSalesInvoiceModel.Payment {
                Amount = p.Amount.Value,
                CheckNumber = p.CheckNumber,
                Method = (models.transaction.PaymentMethod)p.PaymentMethod.Key
            });
            invoice.Payments.AddRange(payments);
            invoice.Items.AddRange(items);

            var posService = infrastructure.IocContainer.Resolve<infrastructure.ISalesInvoiceService>();
            this.InvoiceModel = posService.AddSalesInvoice(invoice);
            this.InvoiceNumber = invoice.InvoiceNumber;
        }


        readonly ObservableAsPropertyHelper<decimal> _paymentTotal;
        public Decimal PaymentTotal {
            get { return _paymentTotal.Value; }
        }

        string _invoiceNumber;
        public string InvoiceNumber {
            get { return _invoiceNumber; }
            set { this.RaiseAndSetIfChanged(ref _invoiceNumber, value); }
        }


        string _debugOutput;
        public string DebugOutput {
            get { return _debugOutput; }
            set { this.RaiseAndSetIfChanged(ref _debugOutput, value); }
        }

    }
}
