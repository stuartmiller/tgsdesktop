using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.infrastructure {
    public interface ISalesInvoiceService {

        IList<models.Product> GetProducts();
        models.Product GetItem(int itemId);
        List<models.SalesInvoice> GetSalesInvoices(IEnumerable<int> ids);
        models.SalesInvoice AddSalesInvoice(models.AddSalesInvoiceModel invoice);
        Task<List<models.SalesInvoiceSummary>> GetTransactionSummariesAsync();
        models.SalesInvoice RefundInvoiceItems(int invoiceId, IEnumerable<Tuple<int, int>> itemIdQty);
    }
}
