using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.infrastructure {
    public interface    IGeneralJournalService {

        IList<models.transaction.AccountType> GetAccountTypes();

        IList<models.transaction.Account> GetAccounts(bool includeArchived = false);

        models.transaction.Transaction2 AddTransaction(models.transaction.Transaction2 transaction, IEnumerable<tgsdesktop.models.transaction.Payment> payments);
    }
}
