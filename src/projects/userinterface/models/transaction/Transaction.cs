using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.models.transaction {
    public class Transaction2 {
        public Transaction2() {
            this.JournalEntries = new List<JournalEntry2>();
        }

        public int Id { get; set; }
        public DateTime PostDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string InvoiceNumber { get; set; }
        public string Memo { get; set; }
        public List<JournalEntry2> JournalEntries { get; private set; }
        public DateTime Modified { get; set; }
        public bool IsReversed { get; set; }
        public byte[] Version { get; set; }
    }

    public class Transaction {
        public Transaction() {
            this.JournalEntries = new List<JournalEntry>();
        }

        public int Id { get; set; }
        public DateTime PostDate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public int? InvoiceId { get; set; }
        public List<Payment> Payments { get; set; }
        public string Memo { get; set; }
        public List<JournalEntry> JournalEntries { get; private set; }
        public DateTime Modified { get; set; }
        public bool IsReversed { get; set; }
        public byte[] Version { get; set; }
    }
}
