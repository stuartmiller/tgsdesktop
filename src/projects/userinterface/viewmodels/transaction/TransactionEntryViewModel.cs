using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.viewmodels.transaction {

    public interface ITransactionEntryViewModel : IRoutableViewModel {
        ReactiveCommand<object> AddPayment { get; }
        ReactiveCommand<object> AddJournalEntry { get; }
        ReactiveCommand<object> AddTransaction { get; }
    }

    public class TransactionEntryViewModel : ViewModelBase, ITransactionEntryViewModel {


        public TransactionEntryViewModel(IScreen screen) : base(screen) {

            this.PaymentMethods = new KeyValuePair<int, string>().GetPaymentMethods();
            this.Transactions = new ReactiveList<TransactionViewModel>();
            // get the financial accounts and put them into a list
            var transService = infrastructure.IocContainer.Resolve<infrastructure.IGeneralJournalService>();
            this.Accounts = new ReactiveList<AccountViewModel>(transService.GetAccounts().Select(x => new AccountViewModel {
                Account = x,
                Id = x.Id,
                Name = x.Name,
            }));

            this.CurrentPayment = new PaymentViewModel();
            this.Payments = new ReactiveList<PaymentViewModel>();
            this.AddPayment = ReactiveCommand.Create(
                this.CurrentPayment.WhenAny(
                vm => vm.PaymentMethod,
                vm => vm.Amount,
                vm => vm.CheckNumber,
                (pm, a, cn) => {
                    return pm.GetValue().Key != 0
                        && a.GetValue().HasValue && a.Value.Value > 0
                        && ((pm.GetValue().Key) != 2 || !string.IsNullOrEmpty(cn.GetValue()));
                }));
            this.AddPayment.Subscribe(x => {
                var memo = this.CurrentPayment.PaymentMethod.Value.ToString();
                if (!string.IsNullOrEmpty(this.CurrentPayment.CheckNumber))
                    memo += " #" + this.CurrentPayment.CheckNumber;
                this.Memo += string.IsNullOrEmpty(this.Memo) ? ' ' + memo : memo;
                var p = new PaymentViewModel {
                    Amount = this.CurrentPayment.Amount,
                    CheckNumber = this.CurrentPayment.CheckNumber,
                    PaymentMethod = this.CurrentPayment.PaymentMethod
                };
                p.DeletePayment.Subscribe(_ => this.Payments.Remove(p));
                this.Payments.Add(p);
                this.CurrentPayment.Amount = null;
                this.CurrentPayment.CheckNumber = null;
                this.CurrentPayment.PaymentMethod = models.transaction.PaymentMethod.Undefined.GetPaymentMethodKeyValuePair();
            });


            this.CurrentPayment.WhenAnyValue(vm => vm.CheckNumber)
                .Subscribe(cn => {
                    if (!string.IsNullOrEmpty(cn))
                        this.CurrentPayment.PaymentMethod = 
                            this.PaymentMethods.Single(x => x.Key == (int)models.transaction.PaymentMethod.Check);
                });
            


            // get the customers, and put them in a list
            var accountService = infrastructure.IocContainer.Resolve<infrastructure.IAccountReceivableService>();
            this.Customers = new ReactiveList<CustomerViewModel>(accountService.GetPeople(
                models.PersonType.Camper
                | models.PersonType.Staff
                | models.PersonType.Other).Select(x => new CustomerViewModel(x as models.Person)));
            // create the jounalentry list and set the first two journal entries. These cannot be deleted, because any journal
            // transaction requires at least two journal entries
            this.JournalEntries = new ReactiveList<GeneralJournalViewModel>();
            this.JournalEntries.ChangeTrackingEnabled = true;
            this.CurrentJournalEntry = new GeneralJournalViewModel();

            // create the commands
            this.AddJournalEntry = ReactiveCommand.Create(this.WhenAny(
                vm => vm.CurrentJournalEntry.SelectedAccount,
                vm => vm.CurrentJournalEntry.SeasonId,
                vm => vm.CurrentJournalEntry.Amount,
                vm => vm.CurrentPayment.PaymentMethod,
                (act, sid, amt, pm) => {
                    return amt.Value.HasValue
                        && amt.Value.Value != 0
                        && pm.GetValue().Key == 0
                        && act.Value != null
                        && sid.Value.HasValue;
                }));
            this.AddJournalEntry.Subscribe(_ => {
                var je = this.CurrentJournalEntry;
                je.DeleteJournalEntry.Subscribe(x => this.JournalEntries.Remove(je));
                this.JournalEntries.Add(je);
                var amt = this.JournalEntries.Where(x => x.IsCredit == false).Sum(x => x.Amount)
                    - this.JournalEntries.Where(x => x.IsCredit == true).Sum(x => x.Amount);
                if (amt > 0)
                    this.CurrentJournalEntry = new GeneralJournalViewModel { SeasonId = je.SeasonId, Amount = amt, IsCredit = true };
                else if (amt< 0)
                    this.CurrentJournalEntry = new GeneralJournalViewModel { SeasonId = je.SeasonId, Amount = System.Math.Abs(amt.Value), IsCredit = false };
                else
                    this.CurrentJournalEntry = new GeneralJournalViewModel { SeasonId = je.SeasonId };
            });
            
            this.EffectiveDate = DateTime.Today;

            var journalInBalance = this.WhenAnyObservable(vm => vm.JournalEntries.CountChanged)
                .Select(_ => {
                    return this.JournalEntries.Count > 0
                        && this.JournalEntries.Sum(je => je.DebitAmount) == this.JournalEntries.Sum(je => je.CreditAmount);
                });
            var journalOutOfBalance = this.WhenAnyObservable(
                vm => vm.JournalEntries.CountChanged)
                .Select(_ => {
                    return this.JournalEntries.Count == 0
                        || this.JournalEntries.Sum(je => je.DebitAmount) != this.JournalEntries.Sum(je => je.CreditAmount);
                });
            this._journalBalanced = journalInBalance.ToProperty(this, vm => vm.JournalBalanced);
            this._journalOutOfBalance = journalOutOfBalance.ToProperty(this, vm => vm.JournalOutOfBalance);

            // only enabled if the debits are > 0 && debits == credits
            this.AddTransaction = ReactiveCommand.Create(this.WhenAnyObservable(
                vm => vm.JournalEntries.CountChanged)
                .Select(_ => {
                    var debitAmt = this.JournalEntries.Sum(je => je.DebitAmount);
                    var creditAmt = this.JournalEntries.Sum(je => je.CreditAmount);
                    return debitAmt > 0 && creditAmt == debitAmt;
                })
            );
            this.AddTransaction.Subscribe(_ => {
                System.Diagnostics.Debug.WriteLine("TransactionAdded");
                var transaction = new models.transaction.Transaction {
                    EffectiveDate = this.EffectiveDate.Date,
                    InvoiceNumber = this.InvoiceNumber,
                    Memo = this.Memo
                };

                List<tgsdesktop.models.transaction.Payment> payments = new List<models.transaction.Payment>();
                payments.AddRange(this.Payments.Select(p => new models.transaction.Payment {
                    Method = (models.transaction.PaymentMethod)p.PaymentMethod.Key,
                    Amount = p.Amount.Value,
                    CheckNumber = p.CheckNumber
                }));
                transaction.JournalEntries.AddRange(this.JournalEntries.Select(je => new models.transaction.JournalEntry {
                    AccountId = je.SelectedAccount.Id.Value,
                    Amount = je.Amount.Value,
                    CustomerId = je.SelectedCustomer == null ? null : (int?)je.SelectedCustomer.PersonModel.Id,
                    IsCredit = je.IsCredit,
                    Memo = je.Memo,
                    SeasonId = je.SeasonId.Value
                }));
                var txn = transService.AddTransaction(transaction, payments);
                this.Transactions.Add(new TransactionViewModel(txn));
                this.InvoiceNumber = null;
                this.Memo = null;
                this.JournalEntries.Clear();
                this.Payments.Clear();
            });
        }

        public override string UrlPathSegment { get { return "generaljournalentry"; } }

        public ReactiveCommand<object> AddJournalEntry { get; private set; }
        public ReactiveCommand<object> AddTransaction { get; private set; }
        public ReactiveCommand<object> AddPayment { get; private set; }

        public ReactiveList<GeneralJournalViewModel> JournalEntries{ get; private set; }
        public ReactiveList<AccountViewModel> Accounts { get; private set; }
        public ReactiveList<CustomerViewModel> Customers { get; private set; }
        public List<KeyValuePair<int, string>> PaymentMethods { get; private set; }
        public ReactiveList<PaymentViewModel> Payments { get; private set; }
        public ReactiveList<TransactionViewModel> Transactions { get; private set; }


        DateTime _effectiveDate;
        public DateTime EffectiveDate {
            get { return _effectiveDate; }
            set { this.RaiseAndSetIfChanged(ref _effectiveDate, value); }
        }

        string _invoiceNumber;
        public string InvoiceNumber {
            get { return _invoiceNumber; }
            set { this.RaiseAndSetIfChanged(ref _invoiceNumber, value); }
        }

        string _memo;
        public string Memo {
            get { return _memo; }
            set { this.RaiseAndSetIfChanged(ref _memo, value); }
        }

        bool _journalEntriesBalance;
        public bool JournalEntriesBalance {
            get { return _journalEntriesBalance; }
            set { this.RaiseAndSetIfChanged(ref _journalEntriesBalance, value); }
        }

        GeneralJournalViewModel _currentJournalEntry;
        public GeneralJournalViewModel CurrentJournalEntry {
            get { return _currentJournalEntry; }
            set { this.RaiseAndSetIfChanged(ref _currentJournalEntry, value); }
        }
        PaymentViewModel _currentPayment;
        public PaymentViewModel CurrentPayment {
            get { return _currentPayment;}
            set { this.RaiseAndSetIfChanged(ref _currentPayment, value);}
        }

        readonly ObservableAsPropertyHelper<bool> _journalBalanced;
        public bool JournalBalanced {
            get { return _journalBalanced.Value; }
        }

        readonly ObservableAsPropertyHelper<bool> _journalOutOfBalance;
        public bool JournalOutOfBalance {
            get { return _journalOutOfBalance.Value; }
        }

    }
}
