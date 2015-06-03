using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.viewmodels.transaction {
    public class GeneralJournalViewModel : ReactiveObject {

        public GeneralJournalViewModel(models.transaction.JournalEntry2 journalEntry = null) {

            var debitAmount = this.WhenAny(
                vm => vm.IsCredit,
                vm => vm.Amount,
                (c, a) => { return c.Value ? 0m : System.Math.Abs(a.Value.Value); });
            this._debitAmt = debitAmount.ToProperty(this, vm => vm.DebitAmount);
            var creditAmount = this.WhenAny(
                vm => vm.IsCredit,
                vm => vm.Amount,
                (c, a) => { return c.Value ? System.Math.Abs(a.Value.Value) : 0m; });
            this._creditAmt = creditAmount.ToProperty(this, vm => vm.CreditAmount);

            this.DeleteJournalEntry = ReactiveCommand.Create();

            if (journalEntry != null) {
                this.Id = journalEntry.Id;
                this.SeasonId = journalEntry.SeasonId.Value;
                this.Amount = journalEntry.Amount;
                this.IsCredit = journalEntry.IsCredit;
                if (journalEntry.CustomerId.HasValue) {
                    var accountService = infrastructure.IocContainer.Resolve<infrastructure.IAccountReceivableService>();
                    this.SelectedCustomer = new CustomerViewModel(
                        accountService.GetPeople(models.PersonType.Camper
                            | models.PersonType.Staff
                            | models.PersonType.Other).Single(c => c.Id == journalEntry.CustomerId) as models.Person);
                }
                this.Memo = journalEntry.Memo;
                
                var transService = infrastructure.IocContainer.Resolve<infrastructure.IGeneralJournalService>();
                SelectedAccount = new AccountViewModel(transService.GetAccounts(true).Single(a => a.Id == journalEntry.AccountId));
            }
        }

        int? _id;
        public int? Id { get { return _id; } set { this.RaiseAndSetIfChanged(ref _id, value); } }
        int? _seasonId;
        public int? SeasonId { get { return _seasonId; } set { this.RaiseAndSetIfChanged(ref _seasonId, value); } }
        decimal? _amount;
        public decimal? Amount { get { return _amount; } set { this.RaiseAndSetIfChanged(ref _amount, value); } }
        bool _isCredit;
        public bool IsCredit { get { return _isCredit; } set { this.RaiseAndSetIfChanged(ref _isCredit, value); } }
        readonly ObservableAsPropertyHelper<decimal> _debitAmt;
        public decimal DebitAmount { get { return _debitAmt.Value; } }
        readonly ObservableAsPropertyHelper<decimal> _creditAmt;
        public decimal CreditAmount { get { return _creditAmt.Value; } }
        CustomerViewModel _selectedCustomer;
        public CustomerViewModel SelectedCustomer { get { return _selectedCustomer; } set { this.RaiseAndSetIfChanged(ref _selectedCustomer, value); } }
        string _memo;
        public string Memo { get { return _memo; } set { this.RaiseAndSetIfChanged(ref _memo, value); } }
        AccountViewModel _selectedAccount;
        public AccountViewModel SelectedAccount { get { return _selectedAccount; } set { this.RaiseAndSetIfChanged(ref _selectedAccount, value); } }

        public ReactiveCommand<object> DeleteJournalEntry { get; private set; }
    
    }
}
