using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.models.transaction {
    public class JournalEntry2 {
        public int Id { get; set; }
        public int TxnId { get; set; }
        public int? SeasonId { get; set; }
        public decimal Amount { get; set; }
        public bool IsCredit { get; set; }
        public int AccountId { get; set; }
        public int? CustomerId { get; set; }
        public string Memo { get; set; }
    }

    public class JournalEntry {
        public int Id { get; set; }
        public int TxnId { get; set; }
        public int SeasonId { get; set; }
        public decimal SignedAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal DebitAmount { get; set; }
        public int AccountId { get; set; }
        public int? PersonId { get; set; }
        public string Memo { get; set; }
    }
}
