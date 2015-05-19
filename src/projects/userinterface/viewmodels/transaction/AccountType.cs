using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.viewmodels.transaction {
    public enum AccountType {

        CurrentAsset = 1,
        FixedAsset = 2,
        OtherAsset = 3,
        AccountsPayable = 4,
        CurrentLiability = 5,
        Equity = 6,
        Income = 7,
        CostOfGoodsSold = 8,
        Expense = 9
    }
}
