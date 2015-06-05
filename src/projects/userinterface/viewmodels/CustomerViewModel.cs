﻿using ReactiveUI;
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
            this.WhenAnyValue(vm => vm.SelectedCustomer)
                .Subscribe(_ => {
                    this.Transactions.Clear();
                    if (this.SelectedCustomer != null) {
                        var transactions = accountService.GetCustomerTransactions(new int[] { this.SelectedCustomer.PersonModel.Id });
                        foreach (var t in transactions) {
                            var tvm = new CustomerTransactionViewModel {
                                PersonId = t.PersonId,
                                TransactionId = t.TransactionId,
                                EffectiveDate = t.EffectiveDate,
                                CreditAmount = t.IsCredit ? (decimal?)t.Amount : null,
                                DebitAmount = t.IsCredit ? null : (decimal?)t.Amount,
                                Memo = t.Memo
                            };
                            if (t.Invoice != null) {
                                tvm.Invoice = new CustomerTransactionViewModel.InvoiceViewModel {
                                    Id = t.Invoice.Id,
                                    InvoiceNumber = t.Invoice.InvoiceNumber,
                                    TaxableAmount = t.Invoice.TaxableAmount,
                                    NontaxableAmount = t.Invoice.NontaxableAmount,
                                    SalesTax = t.Invoice.SalesTax
                                };
                                //tvm.ReverseTransaction.CanExecute = tvm.Invoice.WhenAnyObservable(inv => inv.items);
                                tvm.ReverseTransaction.Subscribe(inv => {
                                });
                            }
                            this.Transactions.Add(tvm);
                        }

                    }
                });
            this.WhenAnyObservable(vm => vm.Transactions.ItemsAdded)
                .Select(_ => this.Transactions.Where(t => t.CreditAmount.HasValue).Sum(t => t.CreditAmount.Value) - this.Transactions.Where(t => t.DebitAmount.HasValue).Sum(t => t.DebitAmount.Value))
                .ToProperty(this, vm => vm.Balance, out _balance);

        }

        public ReactiveList<transaction.CustomerViewModel> Customers { get; private set; }
        transaction.CustomerViewModel _selectedCustomer;
        public transaction.CustomerViewModel SelectedCustomer { get { return _selectedCustomer; } set { this.RaiseAndSetIfChanged(ref _selectedCustomer, value); } }

        public ReactiveList<CustomerTransactionViewModel> Transactions { get; private set; }

        readonly ObservableAsPropertyHelper<decimal> _balance;
        public decimal Balance { get{ return _balance.Value;} }

        readonly ObservableAsPropertyHelper<bool> _isRefundInvoiceVisible;
        public bool IsRefundInvoiceVisible { get { return _isRefundInvoiceVisible.Value; } }
        readonly ObservableAsPropertyHelper<bool> _isReverseTransactionVisible;
        public bool IsReverseTransactionVisible { get { return _isReverseTransactionVisible.Value; } }


        public class CustomerTransactionViewModel : ReactiveObject {

            public CustomerTransactionViewModel() {
                this.ReverseTransaction = ReactiveCommand.Create();
                this.Cancel = ReactiveCommand.Create();
            }

            public int PersonId { get; set; }
            public int TransactionId { get; set; }
            public DateTime EffectiveDate { get; set; }
            public decimal? CreditAmount { get; set; }
            public decimal? DebitAmount { get; set; }
            public string Memo { get; set; }
            public bool IsInvoice { get; set; }
            public InvoiceViewModel Invoice { get; set; }

            public ReactiveCommand<object> ReverseTransaction { get; private set; }
            public ReactiveCommand<object> Cancel { get; private set; }

            public class InvoiceViewModel {

                public InvoiceViewModel() {
                    this.PrintInvoice = ReactiveCommand.Create();
                    this.RefundInvoice = ReactiveCommand.Create();
                }

                public int Id { get; set; }
                public string InvoiceNumber { get; set; }
                public decimal TaxableAmount { get; set; }
                public decimal NontaxableAmount { get; set; }
                public int DiscountPercentage { get; set; }
                public decimal SalesTax { get; set; }

                public ReactiveCommand<object> PrintInvoice { get; private set; }
                public ReactiveCommand<object> RefundInvoice { get; private set; }

                public class InvoiceItemViewModel {
                    public int Id { get; set; }
                    public string Description { get; set; }
                    public decimal Price { get; set; }
                    public int Quantity { get; set; }
                }

            }
        }
    }
}
