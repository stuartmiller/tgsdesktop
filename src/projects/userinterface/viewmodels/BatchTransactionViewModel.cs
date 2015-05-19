using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace tgsdesktop.viewmodels {
    public class BatchTransactionViewModel : ViewModelBase {

        public BatchTransactionViewModel(IScreen screen) : base(screen) {

            this.BatchTransactions = new ReactiveList<models.AccountReceivableTransaction>();

            this.SubmitBatchCommand = ReactiveCommand.Create();
            this.CancelBatchCommand = ReactiveCommand.Create();
            this.CancelBatchCommand.Subscribe(_ => {
                this.BatchTransactions.Clear();
            });

        }

        bool _hasTransactions;
        public bool BatchHasTransactions {
            get { return _hasTransactions; }
            set { this.RaiseAndSetIfChanged(ref _hasTransactions, value); }
        }

        DateTime _batchTransDate;
        public DateTime BatchTransactionDate {
            get { return _batchTransDate; }
            set { this.RaiseAndSetIfChanged(ref _batchTransDate, value); }
        }

        int _selectedAccountId;
        public int BatchSelectedAccountId {
            get { return _selectedAccountId; }
            set { this.RaiseAndSetIfChanged(ref _selectedAccountId, value); }
        }

        int _transactionCount;
        public int BatchTransactionCount {
            get { return _transactionCount; }
            set { this.RaiseAndSetIfChanged(ref _transactionCount, value); }
        }

        decimal _batchTotal;
        public decimal BatchTotal {
            get { return _batchTotal; }
            set { this.RaiseAndSetIfChanged(ref _batchTotal, value); }
        }

        decimal _totalNonTaxableAmt;
        public decimal TotalNonTaxableAmt {
            get { return _totalNonTaxableAmt; }
            set { this.RaiseAndSetIfChanged(ref _totalNonTaxableAmt, value); }
        }

        bool _lockAccount;
        public bool LockAccount {
            get { return _lockAccount; }
            set { this.RaiseAndSetIfChanged(ref _lockAccount, value); }
        }

        bool _lockMemo;
        public bool LockMemo {
            get { return _lockMemo; }
            set { this.RaiseAndSetIfChanged(ref _lockMemo, value); }
        }

        public ReactiveList<models.AccountReceivableTransaction> BatchTransactions { get; private set; }

        public ReactiveCommand<object> SubmitBatchCommand { get; set; }
        public ReactiveCommand<object> CancelBatchCommand { get; set; }

    }
}
