using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.services.transaction {
    public class GeneralJournalService : ServiceBase, infrastructure.IGeneralJournalService {

        static List<models.transaction.AccountType> _accountTypeList;
        static List<models.transaction.Account> _accountList;

        public IList<models.transaction.AccountType> GetAccountTypes() {

            if (_accountTypeList == null) {
                this.Command.CommandText = @"SELECT id, name FROM tbl_accountType";

                _accountTypeList = new List<models.transaction.AccountType>();
                using (var dr = this.ExecuteReader(System.Data.CommandBehavior.Default)) {
                    var count = 0;
                    while (dr.Read()) {
                        _accountList.Add(new models.transaction.Account {
                            Id = dr.GetInt32(count++),
                            Name = dr.GetString(count++),
                        });
                    }
                }
            }
            return _accountTypeList;
        }

        public IList<models.transaction.Account> GetAccounts(bool includeArchived = false) {

            if (_accountList == null) {
                this.Command.CommandText = @"SELECT act.id, act.name, act.typeId, act.parentId, act.isTaxable, act.archivedUtc FROM tbl_account act";

                _accountList = new List<models.transaction.Account>();
                using (var dr = this.ExecuteReader(System.Data.CommandBehavior.Default)) {
                    while (dr.Read()) {
                        var count = 0;
                        _accountList.Add(new models.transaction.Account {
                            Id = dr.GetInt32(count++),
                            Name = dr.GetString(count++),
                            TypeId = dr.GetInt32(count),
                            ParentId = dr.IsDBNull(++count) ? null : (int?)dr.GetInt32(count),
                            IsTaxable = dr.GetBoolean(++count),
                            Archived = dr.IsDBNull(++count)? null : (DateTime?)dr.GetDateTime(count)
                        });
                    }
                }
            }
            return _accountList.Where(x => includeArchived ? true : !x.Archived.HasValue).ToList();
        }

        public models.transaction.Transaction2 AddTransaction(models.transaction.Transaction2 transaction,
            IEnumerable<tgsdesktop.models.transaction.Payment> payments) {

            this.Reset();
            this.Command.CommandText = "proc_addTransactionJournalEntries";
            this.Command.CommandType = CommandType.StoredProcedure;

            var pmtDt = SqlUdtTypes.GetPaymentEntryTable();
            var gjeDt = SqlUdtTypes.GetAccountJournalEntryTable();

            //this.Command.Parameters.AddWithValue("@invoiceNo", transaction.InvoiceNumber);
            var postDateParam = this.Command.Parameters.Add("@postDate", SqlDbType.DateTime);
            postDateParam.Direction = ParameterDirection.Output;
            var effectiveDateParam = this.Command.Parameters.AddWithValue("@effectiveDate",
                transaction.EffectiveDate.HasValue ? transaction.EffectiveDate.Value : (object)DBNull.Value);
            effectiveDateParam.Direction = ParameterDirection.InputOutput;
            this.Command.Parameters.AddWithValue("@txnMemo", transaction.Memo);
            var txnIdParam = this.Command.Parameters.Add("@txnId", SqlDbType.Int);
            txnIdParam.Direction = ParameterDirection.Output;
            var versionParam = this.Command.Parameters.Add("@version", SqlDbType.Binary, 8);
            versionParam.Direction = ParameterDirection.Output;

            var gjeParam = this.Command.Parameters.Add("@accounts", SqlDbType.Structured);
            var pmtParam = this.Command.Parameters.Add("@payments", SqlDbType.Structured);

            this.Command.Parameters.AddWithValue("@userId", User.Person.Id);

            foreach (var pmt in payments)
                pmtDt.Rows.Add(pmt.MethodId, pmt.Amount, pmt.CheckNumber);
            foreach (var je in transaction.JournalEntries)
                gjeDt.Rows.Add(je.AccountId, je.SeasonId, je.Amount, je.IsCredit, je.CustomerId, je.Memo);

            pmtParam.Value = pmtDt;
            gjeParam.Value = gjeDt;

            this.Execute();

            var id = Convert.ToInt32(txnIdParam.Value);

            return this.GetTransactions(new int[] { id }).First();
        }

        public IList<models.transaction.Transaction2> GetTransactions(IEnumerable<int> ids) {
            this.Reset();
            this.Command.CommandText = @"SELECT t.id, t.postDateUtc, t.effectiveDate, t.invoiceNo, t.memo, t.modifiedUtc, t.reversedUtc, t.version
FROM tbl_transaction t
    INNER JOIN @keys k ON t.id=k.id";

            var idTable = SqlUdtTypes.GetIdArrayTable(ids.Distinct());
            var keysParam = this.Command.Parameters.AddWithValue("@keys", SqlDbType.Structured);
            keysParam.TypeName = "udt_intIdArray";
            keysParam.Value = idTable;

            var retVal = new Dictionary<int, models.transaction.Transaction2>();
            using (var dr = this.ExecuteReader()) {
                while (dr.Read()) {
                    var i = 0;
                    var id = dr.GetInt32(i++);
                    var t = new models.transaction.Transaction2 {
                        Id = id,
                        PostDate = DateTime.SpecifyKind(dr.GetDateTime(i++), DateTimeKind.Utc),
                        EffectiveDate = dr.GetDateTime(i).Date,
                        InvoiceNumber = dr.IsDBNull(++i) ? null : dr.GetString(i),
                        Memo = dr.IsDBNull(++i) ? null : dr.GetString(i),
                        Modified = DateTime.SpecifyKind(dr.GetDateTime(++i), DateTimeKind.Utc),
                        IsReversed = !dr.IsDBNull(++i)
                    };
                    t.Version = new byte[8];
                    dr.GetBytes(++i, 0, t.Version, 0, 8);
                    retVal.Add(id, t);
                }
            }

            this.Command.CommandText = @"SELECT gj.id, gj.txnId, gj.seasonId, gj.signedAmt, gj.accountId, gj.personId, gj.memo
FROM tbl_generalJournal gj
    INNER JOIN @keys k ON gj.txnId=k.id";

            using (var dr = this.ExecuteReader()) {
                while (dr.Read()) {
                    var i = 0;
                    var id = dr.GetInt32(i++);
                    var txnId = dr.GetInt32(i++);
                    retVal[txnId].JournalEntries.Add(new models.transaction.JournalEntry2 {
                        Id = id,
                        TxnId = txnId,
                        SeasonId = dr.GetInt32(i++),
                        Amount = System.Math.Abs(dr.GetDecimal(i)),
                        IsCredit = dr.GetDecimal(i++) < 0,
                        AccountId = dr.GetInt32(i),
                        CustomerId = dr.IsDBNull(++i) ? null : (int?)dr.GetInt32(i),
                        Memo = dr.IsDBNull(++i) ? null : dr.GetString(i)
                    });
                }
            }


//            this.Command.CommandText = @"SELECT p.id, p.txnId, p.methodId, p.amount, p.checkNo, p.depositId, p.createdUtc, p.orderIndex
//FROM tbl_payment p
//    INNER JOIN @keys k ON p.txnId=k.id";

//            using (var dr = this.ExecuteReader()) {
//                while (dr.Read()) {
//                    var i = 0;
//                    var id = dr.GetInt32(i++);
//                    var txnId = dr.GetInt32(i++);
//                    retVal[txnId].Payments.Add(new models.transaction.Payment {
//                        Id = id,
//                        TransactionId = txnId,
//                        Method = (models.transaction.PaymentMethod)dr.GetInt32(i++),
//                        Amount = dr.GetDecimal(i),
//                        CheckNumber = dr.IsDBNull(++i) ? null : dr.GetString(i),
//                        DepositId = dr.IsDBNull(++i) ? null : (int?)dr.GetInt32(i),
//                        Created = DateTime.SpecifyKind(dr.GetDateTime(++i), DateTimeKind.Utc),
//                        DepositOrderIndex = dr.GetInt32(++i)
//                    });
//                }
//            }

            return retVal.Values.ToList();

        }
    }
}
