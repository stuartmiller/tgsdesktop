using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.viewmodels.transaction {

    public class AccountViewModel : ReactiveObject {

        public AccountViewModel(models.transaction.Account account = null) {
            this.Account = account;
            if (account != null) {
                this.Id = account.Id;
                this.Number = account.Number;
                this.Name = account.Name;
                this.AccountType = (AccountType)account.TypeId;
                this.IsTaxable = account.IsTaxable;
                this.IsArchived = account.Archived.HasValue;

                if (account.ParentId.HasValue) {
                    var transService = infrastructure.IocContainer.Resolve<infrastructure.IGeneralJournalService>();
                    var parentAccount = transService.GetAccounts(true).SingleOrDefault(a => a.Id == account.ParentId.Value);
                    if (parentAccount != null)
                        this.Parent = new AccountViewModel(parentAccount);
                }
            }
        }

        public models.transaction.Account Account { get; set; }

        int? _id;
        public int? Id { get { return _id; } set { this.RaiseAndSetIfChanged(ref _id, value); } }
        string _number;
        public string Number { get { return _number; } set { this.RaiseAndSetIfChanged(ref _number, value); } }
        string _name;
        public string Name { get { return _name; } set { this.RaiseAndSetIfChanged(ref _name, value); } }
        AccountType? _accountType;
        public AccountType? AccountType { get { return _accountType; } set { this.RaiseAndSetIfChanged(ref _accountType, value); } }
        bool _isTaxable;
        public bool IsTaxable { get { return _isTaxable; } set { this.RaiseAndSetIfChanged(ref _isTaxable, value); } }
        bool _isArchived;
        public bool IsArchived { get { return _isArchived; } set { this.RaiseAndSetIfChanged(ref _isArchived, value); } }
        AccountViewModel _parent;
        public AccountViewModel Parent { get { return _parent; } set { this.RaiseAndSetIfChanged(ref _parent, value); } }

    }
}
