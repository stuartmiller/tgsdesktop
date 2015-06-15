using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.models {
    public class CustomerTransactionSummary {

        public int PersonId { get; set; }
        public int TransactionId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsCredit { get; set; }
        public string Memo { get; set; }
        public int? InvoiceId { get; set; }
        public int? ReturnId { get; set; }
    }

}
