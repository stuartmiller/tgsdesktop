using ReactiveUI;
using System.Reactive.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.viewmodels {

    public interface ICustomerViewModel {

    }

    public class CustomerViewModel : ViewModelBase, ICustomerViewModel {

        public CustomerViewModel(IScreen screen)
            : base(screen) {

            this.Customers = new ReactiveList<transaction.CustomerViewModel>();

            var accountService = infrastructure.IocContainer.Resolve<infrastructure.IAccountReceivableService>();
            this.Customers.Clear();
            var customers = accountService.GetPeople(models.PersonType.Camper | models.PersonType.Staff);

            this.Customers.AddRange(customers.Select(x => new transaction.CustomerViewModel(x as models.Person)));
            this.Customers.Reset();

            Transactions = new ReactiveList<CustomerTransactionViewModel>();
            this.WhenAnyValue(vm => vm.SelectedCustomer.Changed)
                .Subscribe(_ => this.ShowTransactions());
            this.WhenAnyObservable(vm => vm.Transactions.ItemsAdded)
                .Select(_ => this.Transactions.Where(t => t.CreditAmount.HasValue).Sum(t => t.CreditAmount.Value) - this.Transactions.Where(t => t.DebitAmount.HasValue).Sum(t => t.DebitAmount.Value))
                .ToProperty(this, vm => vm.Balance, out _balance);

            this.ConfirmReverseTransaction = ReactiveCommand.Create();
            this.ConfirmReverseTransaction.Subscribe(_ => {
                if (this.ReverseSelectedTransaction && this.SelectedTransaction != null) {
                    tgsdesktop.infrastructure.IocContainer.Resolve<infrastructure.IGeneralJournalService>().ReverseTransaction(this.SelectedTransaction.TransactionId);
                    this.ReverseSelectedTransaction = false;
                    this.SelectedTransaction = null;
                    this.ShowTransactions();
                }
            });
            this.CancelReverseTransaction = ReactiveCommand.Create();
            this.CancelReverseTransaction.Subscribe(_ => {
                this.ReverseSelectedTransaction = false;
            });

            this.WhenAnyValue(vm => vm.ReverseSelectedTransaction)
                .ToProperty(this, vm => vm.ReverseTransactionVisible, out _reverseTransactionVisible, initialValue: false);
            this.WhenAnyValue(
                vm => vm.SelectedCustomer)
                .Select(x => x != null)
                .ToProperty(this, vm => vm.TransactionsVisible, out _transactionsVisible);
            this.WhenAnyValue(vm => vm.SelectedTransaction)
                .Select(t => t != null && t.IsInvoice)
                .ToProperty(this, vm => vm.InvoiceVisible, out _invoiceVisible);
        }

        void ShowTransactions() {

            this.Transactions.Clear();
            if (this.SelectedCustomer != null) {
                var accountService = infrastructure.IocContainer.Resolve<infrastructure.IAccountReceivableService>();
                var transactions = accountService.GetCustomerTransactions(new int[] { this.SelectedCustomer.PersonModel.Id });
                var invoiceModels = tgsdesktop.infrastructure.IocContainer.Resolve<infrastructure.ISalesInvoiceService>().GetSalesInvoices(transactions.Where(x => x.InvoiceId.HasValue).Select(x => x.InvoiceId.Value));
                foreach (var t in transactions) {
                    var tvm = new CustomerTransactionViewModel {
                        PersonId = t.PersonId,
                        TransactionId = t.TransactionId,
                        EffectiveDate = t.EffectiveDate,
                        CreditAmount = t.IsCredit ? (decimal?)t.Amount : null,
                        DebitAmount = t.IsCredit ? null : (decimal?)t.Amount,
                        Memo = t.Memo,
                        IsInvoice = t.InvoiceId.HasValue,
                        IsReturn = t.ReturnId.HasValue
                    };
                    if (t.InvoiceId.HasValue) {
                        var invoiceModel = invoiceModels.Single(x => x.Id == t.InvoiceId.Value);
                        tvm.Invoice = new CustomerTransactionViewModel.InvoiceViewModel {
                            Id = t.InvoiceId.Value,
                            InvoiceNumber = invoiceModel.InvoiceNumber,
                        };
                        tvm.Invoice = new CustomerTransactionViewModel.InvoiceViewModel {
                            Id = invoiceModel.Id,
                            DiscountPercentage = (int)invoiceModel.DiscountPercentage * 100,
                            InvoiceNumber = invoiceModel.InvoiceNumber
                        };
                        tvm.Invoice.Items.AddRange(invoiceModel.Items.Select(x => new CustomerTransactionViewModel.InvoiceViewModel.InvoiceItemViewModel {
                            Id = x.Id,
                            Description = x.Description,
                            Price = x.Price,
                            Quantity = x.Quantity
                        }));
                        if (invoiceModel.ReturnItems.Count > 0) {
                            foreach (var ri in invoiceModel.ReturnItems) {
                                var item = tvm.Invoice.Items.Single(x => x.Id == ri.ItemId);
                                item.QuantityReturned += ri.Quantity;
                            }
                        }
                    }
                    tvm.ReverseTransaction.Subscribe(__ => this.ReverseSelectedTransaction = true);
                    //tvm.Cancel.Subscribe(__ => this.TransactionToReverse = null);
                    tvm.ViewTransaction.Subscribe(__ => {
                        this.SelectedTransaction = tvm;
                    });
                    this.Transactions.Add(tvm);
                }
            }
        }

        public ReactiveList<transaction.CustomerViewModel> Customers { get; private set; }
        transaction.CustomerViewModel _selectedCustomer;
        public transaction.CustomerViewModel SelectedCustomer { get { return _selectedCustomer; } set { this.RaiseAndSetIfChanged(ref _selectedCustomer, value); } }
        CustomerTransactionViewModel _selectedTransaction;
        public CustomerTransactionViewModel SelectedTransaction { get { return _selectedTransaction; } set { this.RaiseAndSetIfChanged(ref _selectedTransaction, value); } }
        bool _reverseSelectedTransaction;
        public bool ReverseSelectedTransaction { get { return _reverseSelectedTransaction; } set { this.RaiseAndSetIfChanged(ref _reverseSelectedTransaction, value); } }

        public ReactiveList<CustomerTransactionViewModel> Transactions { get; private set; }

        readonly ObservableAsPropertyHelper<decimal> _balance;
        public decimal Balance { get{ return _balance.Value;} }

        readonly ObservableAsPropertyHelper<bool> _transactionsVisible;
        public bool TransactionsVisible { get { return _transactionsVisible.Value; } }
        readonly ObservableAsPropertyHelper<bool> _reverseTransactionVisible;
        public bool ReverseTransactionVisible { get { return _reverseTransactionVisible.Value; } }
        readonly ObservableAsPropertyHelper<bool> _invoiceVisible;
        public bool InvoiceVisible { get { return _invoiceVisible.Value; } }

        public ReactiveCommand<object> ConfirmReverseTransaction { get; private set; }
        public ReactiveCommand<object> CancelReverseTransaction { get; private set; }

        public class CustomerTransactionViewModel : ReactiveObject {

            public CustomerTransactionViewModel() {
                this.ReverseTransaction = ReactiveCommand.Create();
                this.Cancel = ReactiveCommand.Create();
                this.ViewTransaction = ReactiveCommand.Create();
            }

            public int PersonId { get; set; }
            public int TransactionId { get; set; }
            public DateTime EffectiveDate { get; set; }
            public decimal? CreditAmount { get; set; }
            public decimal? DebitAmount { get; set; }
            public string Memo { get; set; }
            public bool IsInvoice { get; set; }
            public bool IsReturn { get; set; }
            public InvoiceViewModel Invoice { get; set; }

            public ReactiveCommand<object> ReverseTransaction { get; private set; }
            public ReactiveCommand<object> ViewTransaction { get; private set; }
            public ReactiveCommand<object> Cancel { get; private set; }

            public class InvoiceViewModel : ReactiveObject {

                public InvoiceViewModel() {
                    this.Items = new ReactiveList<InvoiceItemViewModel>();
                    this.Items.ChangeTrackingEnabled = true;
                    this.PrintInvoice = ReactiveCommand.Create();
                    this.RefundItems = ReactiveCommand.Create(
                        this.WhenAnyObservable(vm => vm.Items.ItemChanged)
                        .Select(_ => this.Items.Any(i => i.QuantityToReturn.HasValue && i.QuantityToReturn.Value > 0))
                    );
                    this.RefundItems.Subscribe(_ => {
                        var items = this.Items.Where(x => x.QuantityToReturn.HasValue && x.QuantityToReturn.Value > 0)
                            .Select(x => new Tuple<int, int>(x.Id, x.QuantityToReturn.Value));;
                        tgsdesktop.infrastructure.IocContainer.Resolve<infrastructure.ISalesInvoiceService>().RefundInvoiceItems(this.Id, items); 
                    });
                }

                public int Id { get; set; }
                public string InvoiceNumber { get; set; }
                public decimal TaxableAmount { get; set; }
                public decimal NontaxableAmount { get; set; }
                public int DiscountPercentage { get; set; }
                public decimal SalesTax { get; set; }

                public ReactiveList<InvoiceItemViewModel> Items { get; private set; }

                public ReactiveCommand<object> PrintInvoice { get; private set; }
                public ReactiveCommand<object> RefundItems { get; private set; }

                public class InvoiceItemViewModel: ReactiveObject {

                    public InvoiceItemViewModel() {
                        this.WhenAnyValue(vm => vm.QuantityToReturn).Subscribe(_ => {
                            if (this.QuantityReturned + this.QuantityToReturn > this.Quantity)
                                this.QuantityToReturn = this.Quantity - this.QuantityReturned;
                        });
                    }

                    public int Id { get; set; }
                    public string Description { get; set; }
                    public decimal Price { get; set; }
                    public int Quantity { get; set; }
                    // this is the total quantity returned
                    public int QuantityReturned { get; set; }
                    // this is the quantity to return. This amount plus the QuantityReturned cannot exceed the Quantity
                    int? _quantityToReturn;
                    public int? QuantityToReturn { get { return _quantityToReturn; } set { this.RaiseAndSetIfChanged(ref _quantityToReturn, value); } }

                    public bool IsReturnEnabled { get { return this.Quantity > this.QuantityReturned; } }
                }

            }
        }
    }
}
