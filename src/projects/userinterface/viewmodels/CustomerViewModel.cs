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
            this.WhenAnyValue(vm => vm.SelectedCustomer)
                .Subscribe(_ => {
                    this.Transactions.Clear();
                    if (this.SelectedCustomer != null) {
                        var transactions = accountService.GetCustomerTransactions(new int[] { this.SelectedCustomer.PersonModel.Id });
                        foreach(var t in transactions)
                            this.Transactions.Add(new CustomerTransactionViewModel {
                            PersonId = t.PersonId,
                            TransactionId = t.TransactionId,
                            EffectiveDate = t.EffectiveDate,
                            CreditAmount = t.IsCredit ? (decimal?)t.Amount : null,
                            DebitAmount = t.IsCredit ? null : (decimal?)t.Amount,
                            Memo = t.Memo
                        });
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


        public class CustomerTransactionViewModel : ReactiveObject {

            public int PersonId { get; set; }
            public int TransactionId { get; set; }
            public DateTime EffectiveDate { get; set; }
            public decimal? CreditAmount { get; set; }
            public decimal? DebitAmount { get; set; }
            public string Memo { get; set; }
        }
    }
}
