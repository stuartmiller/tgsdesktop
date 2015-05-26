﻿using System;
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

            this.GoToSalesInvoice = this.RegisterNavigationCommand(() => new SalesInvoiceViewModel(HostScreen));
            this.GoToSalesInvoice2 = this.RegisterNavigationCommand(() => new SalesInvoice2ViewModel(HostScreen));
            //this.GoToAccountDetails = this.RegisterNavigationCommand(() => new account.HomeViewModel(HostScreen));
            //this.GoToPaymentEntry = this.RegisterNavigationCommand(() => new account.PaymentEntryViewModel(HostScreen));
            //this.PrintLabels = this.RegisterNavigationCommand(() => new reporting.ReportViewerViewModel(HostScreen));
            this.MakeGeneralJournalEntries = this.RegisterNavigationCommand(() => new transaction.TransactionEntryViewModel(HostScreen));
            this.GoToSettings = this.RegisterNavigationCommand(() => new SettingsViewModel(HostScreen));


        }

        public override string UrlPathSegment { get { return "home"; } }

        public ReactiveCommand<object> GoToStoreSales { get; private set; }
        public ReactiveCommand<object> GoToAccountDetails { get; private set; }
        public ReactiveCommand<object> GoToPaymentEntry { get; private set; }
        public ReactiveCommand<object> PrintLabels { get; private set; }
        public ReactiveCommand<object> MakeGeneralJournalEntries { get; private set; }
        public ReactiveCommand<object> GoToSalesInvoice { get; private set; }
        public ReactiveCommand<object> GoToSalesInvoice2 { get; private set; }
        public ReactiveCommand<object> GoToSettings { get; private set; }

    }
}
