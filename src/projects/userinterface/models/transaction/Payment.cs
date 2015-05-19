using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.models.transaction {
    public class Payment2 {

        public Payment2() {
            this.PersonAccounts = new List<ArPersonAccount>();
        }

        public DateTime EffectiveDate { get; set; }
        public Tuple<string, models.transaction.PaymentMethod> PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string CheckNumber { get; set; }
        public int DepositId { get; set; }
        public int DepositOrderIndex { get; set; }
        public List<ArPersonAccount> PersonAccounts { get; private set; }

        public class ArPersonAccount {
            public int ArPersonId { get; set; }
            public decimal Amount { get; set; }
            public int? SeasonId { get; set; }
        }
    }
    public class Payment {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public int TransactionId { get; set; }
        public int MethodId { get { return (int)this.Method; } }
        public models.transaction.PaymentMethod Method { get; set; }
        public decimal Amount { get; set; }
        public string CheckNumber { get; set; }
        public int? DepositId { get; set; }
        public int? DepositOrderIndex { get; set; }
    }
}
