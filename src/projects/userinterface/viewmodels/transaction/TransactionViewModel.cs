using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.viewmodels.transaction {

    public class TransactionViewModel : ReactiveObject {

        public TransactionViewModel(models.transaction.Transaction transaction) {
            this.Reverse = ReactiveCommand.Create();

            this.Payments = new ReactiveList<PaymentViewModel>();
            this.JouranlEntries = new ReactiveList<GeneralJournalViewModel>();

            if (transaction != null) {
                this.Id = transaction.Id;
                this.EffectiveDate = transaction.EffectiveDate;
                this.Amount = transaction.JournalEntries == null || transaction.JournalEntries.Count == 0 ?
                    null : (decimal?)transaction.JournalEntries.Sum(x => x.Amount);
                this.InvoiceNumber = transaction.InvoiceNumber;
                this.Memo = transaction.Memo;
                //if (transaction.Payments.Count > 0)
                //    this.Payments.AddRange(transaction.Payments.Select(p => new PaymentViewModel(p)));
                if (transaction.JournalEntries.Count > 0)
                    this.JouranlEntries.AddRange(transaction.JournalEntries.Select(je => new GeneralJournalViewModel(je)));
            }
        }

        int? _id;
        public int? Id { get { return _id; } set { this.RaiseAndSetIfChanged(ref _id, value); } }
        DateTime? _effectiveDate;
        public DateTime? EffectiveDate { get { return _effectiveDate; } set { this.RaiseAndSetIfChanged(ref _effectiveDate, value); } }
        decimal? _amount;
        public decimal? Amount { get { return _amount; } set { this.RaiseAndSetIfChanged(ref _amount, value); } }
        string _invoiceNumber;
        public string InvoiceNumber { get { return _invoiceNumber; } set { this.RaiseAndSetIfChanged(ref _invoiceNumber, value); } }
        string _memo;
        public string Memo { get { return _memo; } set { this.RaiseAndSetIfChanged(ref _memo, value); } }

        public ReactiveList<PaymentViewModel> Payments { get; private set; }
        public ReactiveList<GeneralJournalViewModel> JouranlEntries { get; private set; }

        public ReactiveCommand<object> Reverse { get; private set; }

    }
}
