using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.infrastructure {
    public interface    IGeneralJournalService {

        IList<models.transaction.AccountType> GetAccountTypes();

        IList<models.transaction.Account> GetAccounts(bool includeArchived = false);

        models.transaction.Transaction AddTransaction(models.transaction.AddTransactionRequest transaction, IEnumerable<tgsdesktop.models.transaction.Payment> payments);

        void ReverseTransaction(int id);
    }
}
