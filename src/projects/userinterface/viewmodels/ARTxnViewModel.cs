using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace tgsdesktop.viewmodels {
    public class ARTxnViewModel : ViewModelBase {

        public ARTxnViewModel(IScreen screen) : base(screen) {
            //this.TransactionModel = txn == null ? new models.AccountReceivableTransaction() : txn;
        }

        public models.AccountReceivableTransaction TransactionModel { get; private set; }


        int? _id;
        public int? Id {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }

        int _lineItemId;
        public int LineItemId {
            get { return _lineItemId; }
            set { this.RaiseAndSetIfChanged(ref _lineItemId, value); }
        }

        decimal _taxableAmt;
        public decimal TaxableAmount {
            get { return _taxableAmt; }
            set { this.RaiseAndSetIfChanged(ref _taxableAmt, value); }
        }

        decimal _nonTaxableAmt;
        public decimal NonTaxableAmount {
            get { return _nonTaxableAmt; }
            set { this.RaiseAndSetIfChanged(ref _nonTaxableAmt, value); }
        }

        string _memo;
        public string Memo {
            get { return _memo; }
            set { this.RaiseAndSetIfChanged(ref _memo, value); }
        }

        DateTime _effectiveDate;
        public DateTime EffectiveDate {
            get { return _effectiveDate; }
            set { this.RaiseAndSetIfChanged(ref _effectiveDate, value); }
        }

        int _householdId;
        public int HouseholdId {
            get { return _householdId; }
            set { this.RaiseAndSetIfChanged(ref _householdId, value); }
        }
    }
}
