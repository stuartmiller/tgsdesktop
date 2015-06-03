using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.infrastructure {
    public interface ISalesInvoiceService {

        IList<models.Product> GetProducts();
        models.Product GetItem(int itemId);
        models.SalesInvoice AddSalesInvoice(models.AddSalesInvoiceModel invoice);
        Task<List<models.SalesInvoiceSummary>> GetTransactionSummariesAsync();
    }
}
