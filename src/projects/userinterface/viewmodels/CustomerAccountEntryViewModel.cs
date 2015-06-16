using ReactiveUI;
using System.Reactive.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.viewmodels {

    public interface ICustomerAccountEntryViewModel {

    }

    public class CustomerAccountEntryViewModel : ViewModelBase, ICustomerAccountEntryViewModel {
        public CustomerAccountEntryViewModel(IScreen screen)
            : base(screen) {

            this.Accounts = new List<AccountDetails>(new AccountDetails[] {
                new AccountDetails{Id = 106, Name="Table Girl Scholarship", AllowPrice=false, AllowQuantity = false, FixedPrice = 100m, IsTaxable=false},
                new AccountDetails{Id = 107, Name="Account Withdrawal", AllowPrice=true, AllowQuantity = false, IsTaxable=false},
                new AccountDetails{Id = 109, Name="Travel Expense", AllowPrice=true, AllowQuantity = false, IsTaxable=false},
                new AccountDetails{Id = 110, Name="Offering", AllowPrice=true, AllowQuantity = false, IsTaxable=false},
                new AccountDetails{Id = 111, Name="Prescriptions", AllowPrice=true, AllowQuantity = false, IsTaxable=false},
                new AccountDetails{Id = 112, Name="Great Day Donation", AllowPrice=true, AllowQuantity = false, IsTaxable=false},
                new AccountDetails{Id = 116, Name="Personal Expense Refund", AllowPrice=true, AllowQuantity = false, IsTaxable=false},
                new AccountDetails{Id = 106, Name="Theme Shirt", AllowPrice=true, AllowQuantity = true, IsTaxable=true, ItemId=125}
            });

            var accountItems = this.Accounts.Where(x => x.ItemId.HasValue);
            var items = tgsdesktop.infrastructure.IocContainer.Resolve<infrastructure.ISalesInvoiceService>().GetItems(accountItems.Select(x => x.ItemId.Value));
            foreach (var i in items) {
                var ai = accountItems.SingleOrDefault(x => x.ItemId == i.ItemId);
                if (ai != null)
                    ai.FixedPrice = i.Price;
            }

            this.Customers = new ReactiveList<transaction.CustomerViewModel>();
            var accountService = infrastructure.IocContainer.Resolve<infrastructure.IAccountReceivableService>();
            this.Customers.Clear();
            var customers = accountService.GetPeople(models.PersonType.Camper | models.PersonType.Staff);

            this.Customers.AddRange(customers.Select(x => new transaction.CustomerViewModel(x as models.Person)));
            this.Customers.Reset();

            this.WhenAnyValue(vm => vm.SelectedAccount)
                .Select(_ => this.SelectedAccount == null ? true : this.SelectedAccount.AllowPrice)
                .ToProperty(this, vm => vm.AmountVisible, out _amountVisible);
            this.WhenAnyValue(vm => vm.SelectedAccount)
                .Select(_ => this.SelectedAccount == null ? true : this.SelectedAccount.AllowQuantity)
                .ToProperty(this, vm => vm.QuantityVisible, out _quantityVisible);

            this.WhenAnyValue(vm => vm.SelectedAccount)
                .Subscribe(_ => {
                    if (this.SelectedAccount == null) {
                        this.Amount = null;
                        this.Quantity = null;
                    } else {
                        this.Amount = SelectedAccount.FixedPrice.HasValue ? (int?)SelectedAccount.FixedPrice.Value : null;
                        this.Quantity = 1;
                    }
                });

            this.AddTransaction = ReactiveCommand.Create(this.WhenAny(
                vm => vm.SelectedAccount,
                vm => vm.SelectedCustomer,
                vm => vm.Amount,
                vm => vm.Quantity,
                (a, b, c, d) => a.GetValue() != null && b.GetValue() != null && c.GetValue().HasValue && c.GetValue().Value > 0 && d.GetValue().HasValue && d.GetValue().Value > 0));
            this.AddTransaction.Subscribe(_ => {
                var svc = tgsdesktop.infrastructure.IocContainer.Resolve<infrastructure.IGeneralJournalService>();
                var atRequest = new models.transaction.AddTransactionRequest {
                    EffectiveDate = this.EffectiveDate,
                    Memo = this.Memo
                };
                atRequest.JournalEntries.AddRange(new models.transaction.AddJournalEntryRequest[] {
                        new models.transaction.AddJournalEntryRequest {
                            AccountId = this.SelectedAccount.Id,
                            Amount = this.Total,
                            CustomerId = this.SelectedCustomer.PersonModel.Id,
                            IsCredit = true
                        },
                        new models.transaction.AddJournalEntryRequest {
                            AccountId = 101,
                            Amount = this.Total,
                            CustomerId = this.SelectedCustomer.PersonModel.Id,
                            IsCredit = false
                        }
                });
            });
        }

        public ReactiveList<transaction.CustomerViewModel> Customers { get; private set; }
        transaction.CustomerViewModel _selectedCustomer;
        public transaction.CustomerViewModel SelectedCustomer { get { return _selectedCustomer; } set { this.RaiseAndSetIfChanged(ref _selectedCustomer, value); } }

        public List<AccountDetails> Accounts { get; private set; }
        AccountDetails _selectedAccount;
        public AccountDetails SelectedAccount { get { return _selectedAccount; } set { this.RaiseAndSetIfChanged(ref _selectedAccount, value); } }

        decimal? _amount;
        public decimal? Amount { get { return _amount; } set { this.RaiseAndSetIfChanged(ref _amount, value); } }
        int? _quantity;
        public int? Quantity { get { return _quantity; } set { this.RaiseAndSetIfChanged(ref _quantity, value); } }
        DateTime _effectiveDate = DateTime.Now.Date;
        public DateTime EffectiveDate { get { return _effectiveDate; } set { this.RaiseAndSetIfChanged(ref _effectiveDate, value); } }
        string _memo;
        public string Memo { get { return _memo; } set { this.RaiseAndSetIfChanged(ref _memo, value); } }

        public class AccountDetails {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsTaxable { get; set; }
            public bool AllowQuantity { get; set; }
            public bool AllowPrice { get; set; }
            public decimal? FixedPrice { get; set; }
            public int? ItemId { get; set; }
        }

        readonly ObservableAsPropertyHelper<bool> _amountVisible;
        public bool AmountVisible { get { return _amountVisible.Value; } }
        readonly ObservableAsPropertyHelper<bool> _quantityVisible;
        public bool QuantityVisible { get { return _quantityVisible.Value; } }
        readonly ObservableAsPropertyHelper<decimal> _total;
        public decimal Total { get { return _total.Value; } }

        public ReactiveCommand<object> AddTransaction { get; private set; }
    }
}
