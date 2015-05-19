using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.infrastructure {
    public interface IPointOfSaleService {

        IList<models.Product> GetProducts();
        models.SalesInvoice AddSalesInvoice(models.AddSalesInvoiceModel invoice);
    }
}
