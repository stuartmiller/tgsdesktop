using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace tgsdesktop.viewmodels {

    public interface IHomeViewModel : IRoutableViewModel {
        ReactiveCommand<object> GoToStoreSales { get; }
        ReactiveCommand<object> GoToAccountDetails { get; }
    }

    public class HomeViewModel : ViewModelBase, IHomeViewModel {
        public HomeViewModel(IScreen screen)
            : base(screen) {

            this.GoToSalesInvoice = this.RegisterNavigationCommand(() => new SalesInvoiceViewModel(this.HostScreen));
            this.GoToSalesInvoice2 = this.RegisterNavigationCommand(() => new SalesInvoice2ViewModel(this.HostScreen));
            this.GoToSalesTransactions = this.RegisterNavigationCommand(() => new SalesTransactionsViewModel(this.HostScreen));
            this.MakeGeneralJournalEntries = this.RegisterNavigationCommand(() => new transaction.TransactionEntryViewModel(this.HostScreen));
            this.GoToSettings = this.RegisterNavigationCommand(() => new SettingsViewModel(this.HostScreen));
            this.GoToCustomer = this.RegisterNavigationCommand(() => new CustomerViewModel(this.HostScreen));
            this.GoToCustomerAccountEntry = this.RegisterNavigationCommand(() => new CustomerAccountEntryViewModel(this.HostScreen));


        }

        public override string UrlPathSegment { get { return "home"; } }

        public ReactiveCommand<object> GoToStoreSales { get; private set; }
        public ReactiveCommand<object> GoToAccountDetails { get; private set; }
        public ReactiveCommand<object> GoToSalesTransactions { get; private set; }
        public ReactiveCommand<object> GoToPaymentEntry { get; private set; }
        public ReactiveCommand<object> PrintLabels { get; private set; }
        public ReactiveCommand<object> MakeGeneralJournalEntries { get; private set; }
        public ReactiveCommand<object> GoToSalesInvoice { get; private set; }
        public ReactiveCommand<object> GoToSalesInvoice2 { get; private set; }
        public ReactiveCommand<object> GoToSettings { get; private set; }
        public ReactiveCommand<object> GoToCustomer { get; private set; }
        public ReactiveCommand<object> GoToCustomerAccountEntry { get; private set; }

    }
}
