using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.infrastructure {
    public interface IAccountReceivableService {
        IList<models.Person> GetPeople(models.PersonType? typeFilter = null, int? seasonId = null);
        void AddPayment(models.transaction.Payment2 payment);
        List<tgsdesktop.models.CustomerTransactionSummary> GetCustomerTransactions(IEnumerable<int> customerIds);
    }
}
