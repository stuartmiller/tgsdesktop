using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.infrastructure {
    public interface ITransactionService {

        //models.CamperTransaction AddCamperAccountSale(models.CamperTransaction transaction);
        //models.CamperTransaction AddCamperAccountTransaction(models.CamperTransaction transaction);

        //models.AccountReceivableTransaction AddAccountReceivableSale(models.AccountReceivableTransaction transaction);
        //models.AccountReceivableTransaction AddAccountReceivableTransaction(models.AccountReceivableTransaction transaction);

        //models.Transaction AddCashSale(models.Transaction transaction);

        void AddReceivablePayment(IList<models.transaction.Payment2> payments, int seasonId, int personId, string memo, string invoiceNo, DateTime? effectiveDate);

    }
}
