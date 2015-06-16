using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.services {
    public class TransactionService : ServiceBase, infrastructure.ITransactionService {

        public void AddReceivablePayment(IList<models.transaction.Payment2> payments, int seasonId, int personId, string memo, string invoiceNo, DateTime? effectiveDate) {
            this.Command.CommandText = "proc_addTransactionJournalEntries";
            this.Command.Parameters.AddWithValue("@invoiceNo", invoiceNo);
            this.AddAccountJournalEntryTableParam(new models.transaction.AddJournalEntryRequest[] {
                new models.transaction.AddJournalEntryRequest {
                    AccountId = 101,
                    Amount = payments.Sum(p => p.Amount),
                    IsCredit = true,
                    CustomerId = personId,
                    SeasonId = seasonId
                }
            });
            this.AddPaymentEntryTableParam(payments);
            this.Command.Parameters.AddWithValue("@effectiveDate", effectiveDate.HasValue ? effectiveDate.Value : DateTime.Today);
            this.Command.Parameters.AddWithValue("@txnMemo", this.GetPaymentMemo(payments));

        }

        string GetPaymentMemo(IList<models.transaction.Payment2> payments) {
            var sl = payments.Select(x => {
                switch (x.PaymentMethod.Item2) {
                    case models.transaction.PaymentMethod.Cash:
                        return "Cash";
                    case models.transaction.PaymentMethod.Check:
                        return "ck #" + x.CheckNumber;
                    default:
                        return x.PaymentMethod.ToString();
                };
            });
            return string.Join("/", sl);
        }

        void AddPaymentEntryTableParam(IList<models.transaction.Payment2> payments) {
            var sqlDataRecords = payments.Select(x => {
                var dataRecord = new SqlDataRecord(new SqlMetaData[] {
                    new SqlMetaData("typeId", SqlDbType.Int),
                    new SqlMetaData("amount", SqlDbType.Money),
                    new SqlMetaData("checkNo", SqlDbType.NVarChar, 50)
                });
                dataRecord.SetInt32(0, (int)x.PaymentMethod.Item2);
                dataRecord.SetDecimal(1, x.Amount);
                dataRecord.SetString(2, x.CheckNumber);
                return dataRecord;
            });

            var pmtParam = this.Command.Parameters.AddWithValue("@pmt", sqlDataRecords);
            pmtParam.SqlDbType = SqlDbType.Structured;
            pmtParam.TypeName = "udt_accountPaymentEntry";
            this.Command.Parameters.AddWithValue("@pmt", pmtParam);
        }

        void AddAccountJournalEntryTableParam(IList<models.transaction.AddJournalEntryRequest> journalEntries) {
            var sqlDataRecords = journalEntries.Select(x => {
                var dataRecord = new SqlDataRecord(new SqlMetaData[] {
                    new SqlMetaData("accountId", SqlDbType.Int),
                    new SqlMetaData("seasonId", SqlDbType.Int),
                    new SqlMetaData("amount", SqlDbType.Money),
                    new SqlMetaData("isCredit", SqlDbType.Bit),
                    new SqlMetaData("personId", SqlDbType.Int),
                    new SqlMetaData("memo", SqlDbType.NVarChar, 300)
                });
                dataRecord.SetInt32(0, x.AccountId);
                if (x.SeasonId.HasValue)
                    dataRecord.SetInt32(1, x.SeasonId.Value);
                dataRecord.SetDecimal(2, x.Amount);
                dataRecord.SetBoolean(3, x.IsCredit);
                if (x.CustomerId.HasValue)
                    dataRecord.SetInt32(4, x.CustomerId.Value);
                dataRecord.SetString(5, x.Memo);
                return dataRecord;
            });

            var jeParam = this.Command.Parameters.AddWithValue("@pmt", sqlDataRecords);
            jeParam.SqlDbType = SqlDbType.Structured;
            jeParam.TypeName = "udt_accountPaymentEntry";
            this.Command.Parameters.AddWithValue("@je", jeParam);
        }
    }
}
