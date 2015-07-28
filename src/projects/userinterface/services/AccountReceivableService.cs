using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace tgsdesktop.services {
    public class AccountReceivableService : ServiceBase, infrastructure.IAccountReceivableService {

        static readonly object _refreshLock = new object();
        static DateTime _lastRefresh = DateTime.UtcNow.AddYears(-100);
        static readonly Dictionary<int, decimal> _accountBalances = new Dictionary<int,decimal>();
        static System.Timers.Timer _timer;

        public AccountReceivableService() {
            lock (_refreshLock)	{
                if (_accountBalances == null){
                }
            }
        }

        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
            try {
                this.RefreshBalances();
            } catch { }
        }

        static readonly List<models.Person> _people = new List<models.Person>();

        public IList<models.Person> GetPeople(models.PersonType? typeFilter = null, int? seasonId = null) {

            if (!seasonId.HasValue)
                seasonId = new SettingsService().GetSettings().CurrentSeasonId;

            if (_people.Count == 0) {

                var households = this.GetHouseholds(seasonId.Value);

                this.Command.Parameters.Clear();
                this.Command.CommandText = @"SELECT p.id, p.lastName, p.firstName, p.nickName, p.householdId, p.dob, p.genderId,
    CAST(CASE WHEN cs.personId IS NOT NULL THEN 1 ELSE 0 END AS bit) AS isCamper,
    CAST(CASE WHEN ps.personId IS NOT NULL THEN 1 ELSE 0 END AS bit) AS isParent,
    CAST(CASE WHEN ss.personId IS NOT NULL THEN 1 ELSE 0 END AS bit) AS isStaff,
    p.cmFamilyRole,
    cs.cmSessionId, sess.name, cs.cmCabinId, c.name
FROM tbl_person p
    LEFT OUTER JOIN tbl_cmCamperSeason cs ON p.id=cs.personId AND cs.seasonId=@seasonId
    LEFT OUTER JOIN tbl_cmStaffSeason ss ON p.id=ss.personId AND ss.seasonId=@seasonId
    LEFT OUTER JOIN tbl_cmParentSeason ps ON p.id=ps.personId AND ps.seasonId=@seasonId
    LEFT OUTER JOIN tbl_session sess ON cs.cmSessionId=sess.cmId
    LEFT OUTER JOIN tbl_cabin c ON cs.cmCabinId=c.cmId
    LEFT OUTER JOIN view_householdBalances ab ON p.id=ab.personId
ORDER BY p.lastName, p.firstName";
                this.Command.Parameters.AddWithValue("@seasonId", seasonId);

                using (var dr = this.ExecuteReader()) {
                    while (dr.Read()) {
                        var i = 0;
                        var p = new models.Person {
                            Id = dr.GetInt32(i++),
                            LastName = dr.GetString(i),
                            FirstName = dr.IsDBNull(++i) ? null : dr.GetString(i),
                            NickName = dr.IsDBNull(++i) ? null : dr.GetString(i),
                            Household = dr.IsDBNull(++i) ? null : households.Single(x => x.Id == dr.GetInt32(i)),
                            Dob = dr.IsDBNull(++i) ? null : (DateTime?)dr.GetDateTime(i).Date,
                            GenderId = dr.IsDBNull(++i) ? null : (int?)dr.GetInt32(i),
                            IsCamper = dr.GetBoolean(++i),
                            IsParent = dr.GetBoolean(++i),
                            IsStaff = dr.GetBoolean(++i),
                            HouseholdRoleId = dr.IsDBNull(++i) ? null : (int?)dr.GetInt32(i)
                        };
                        int? sessionId = dr.IsDBNull(++i) ? null : (int?)dr.GetInt32(i);
                        var sessionName = dr.IsDBNull(++i) ? null : dr.GetString(i);
                        var cabinId = dr.IsDBNull(++i) ? null : (int?)dr.GetInt32(i);
                        var cabinName = dr.IsDBNull(++i) ? null : dr.GetString(i);

                        if (p.IsCamper) {
                            var camper = new models.Camper {
                                Session = sessionId.HasValue ? new models.KeyNamePair<int, string> { Key = sessionId.Value, Name = sessionName } : null,
                                Cabin = cabinId.HasValue ? new models.KeyNamePair<int, string> { Key = cabinId.Value, Name = cabinName } : null
                            };
                            p.ToPerson(camper);
                            p = camper;
                        } else if (p.IsParent) {
                            var parent = new models.Parent();
                            p.ToPerson(parent);
                            p = parent;
                        }
                        _people.Add(p);
                    }
                }
                foreach (var h in households) {
                    h.People.AddRange(_people.Where(p => p.Household == h));
                }
                var parents = _people.Where(p => p.IsParent);
                foreach(var p in parents) {
                    var parent = p as models.Parent;
                    parent.Campers.AddRange(_people.Where(c => c.IsCamper && c.Household == p.Household).Select(c => c as models.Camper));
                }
                this.RefreshBalances();
                _timer = new System.Timers.Timer(30000);
                _timer.Elapsed += _timer_Elapsed;
                _timer.Enabled = true;
                _timer.Start();
            }

            if (typeFilter.HasValue)
                return _people.Where(x =>
                    (((typeFilter & models.PersonType.Camper) == models.PersonType.Camper))
                    ||
                    (((typeFilter & models.PersonType.Staff) == models.PersonType.Staff))
                    ||
                    (((typeFilter & models.PersonType.Parent) == models.PersonType.Parent))
                    ||
                    (((typeFilter & models.PersonType.Other) == models.PersonType.Other))
                ).ToList();
            return _people;
        }

        IList<models.Household> GetHouseholds(int seasonId) {
            this.Command.CommandText = @"SELECT hhld.id, hhld.name, hhld.phone, hhld.email,
    hhld.address1, hhld.address2, hhld.city, hhld.stateProvince, hhld.postalCode, hhld.country, hhld.cmFamilyId
FROM tbl_household hhld
WHERE EXISTS(SELECT * FROM tbl_person WHERE householdId=hhld.id)";

            var retVal = new List<models.Household>();
            using (var dr = this.ExecuteReader(System.Data.CommandBehavior.Default)) {
                while (dr.Read()) {
                    var i = 0;
                    var arAct = new models.Household {
                        Id = dr.GetInt32(i++),
                        Name = dr.GetString(i),
                        Phone = dr.IsDBNull(++i) ? null : dr.GetString(i),
                        Email = dr.IsDBNull(++i) ? null : dr.GetString(i),
                        Address1 = dr.IsDBNull(++i) ? null : dr.GetString(i),
                        Address2 = dr.IsDBNull(++i) ? null : dr.GetString(i),
                        City = dr.IsDBNull(++i) ? null : dr.GetString(i),
                        StateProvince = dr.IsDBNull(++i) ? null : dr.GetString(i),
                        PostalCode = dr.IsDBNull(++i) ? null : dr.GetString(i),
                        Country = dr.IsDBNull(++i) ? null : dr.GetString(i)
                    };
                    retVal.Add(arAct);
                }
            };

            return retVal;
        }

        public void AddPayment(models.transaction.Payment2 payment) {
            this.Reset();
            this.Command.CommandText = "proc_addTransactionJournalEntries";
            this.Command.CommandType = CommandType.StoredProcedure;

            var pmtDt = SqlUdtTypes.GetPaymentEntryTable();
            var gjeDt = SqlUdtTypes.GetAccountJournalEntryTable();

            var effectiveDateParam = this.Command.Parameters.Add("@effectiveDate", SqlDbType.Date);
            var memoParam = this.Command.Parameters.Add("@txnMemo", SqlDbType.NVarChar, 300);

            var gjeParam = this.Command.Parameters.Add("@accounts", SqlDbType.Structured);
            var pmtParam = this.Command.Parameters.Add("@payments", SqlDbType.Structured);

            effectiveDateParam.Value = payment.EffectiveDate.Date;
            memoParam.Value = "check #" + payment.CheckNumber;

            pmtDt.Rows.Add((int)payment.PaymentMethod.Item2, payment.Amount, payment.CheckNumber, payment.DepositOrderIndex);

            foreach (var act in payment.PersonAccounts) {
                gjeDt.Rows.Add(101, act.SeasonId, act.Amount, true, act.ArPersonId, null);
                gjeDt.Rows.Add(105, act.SeasonId, act.Amount, false, act.ArPersonId, null);
            }

            pmtParam.Value = pmtDt;
            gjeParam.Value = gjeDt;

            this.Execute();
        }

        internal void RefreshBalances() {
            var db = new Db();
            db.Command.CommandText = "SELECT GETUTCDATE()";
            var lastRefhresh = DateTime.SpecifyKind(db.ExecuteScaler<DateTime>(), DateTimeKind.Utc);
            db.Reset();
            db.Command.CommandText = @"SELECT p.id AS personId,
ISNULL((SELECT SUM(signedAmt) FROM dbo.view_generalJournal WHERE personId=p.id AND accountId=101),0) AS balance
FROM tbl_person p
WHERE EXISTS(SELECT * FROM dbo.tbl_generalJournal WHERE personId=p.id)
AND (SELECT MAX(CASE WHEN t.reversedUtc IS NULL THEN t.postDateUtc ELSE t.reversedUtc END) FROM dbo.tbl_transaction t
	INNER JOIN dbo.tbl_generalJournal gj ON t.id=gj.txnId WHERE gj.personId=p.id)>=@date";
            db.Command.Parameters.AddWithValue("@date", _lastRefresh);
 
            var accountBalances = new Dictionary<int, decimal>();

            using (var dr = db.ExecuteReader()) {
                while (dr.Read()) {
                    int id = dr.GetInt32(0);
                    var balance = dr.GetDecimal(1);
                    accountBalances.Add(id, balance);
                    var person = _people.SingleOrDefault(x => x.Id == id);
                    if (person != null)
                        person.Balance = balance;
                }
            }
            var parents = _people.Where(p => p.IsParent).Select(p => p as models.Parent);
            foreach (var p in parents) {
                p.Balance = p.Campers.Sum(x => x.Balance);
            }
            lock (_refreshLock) {
                foreach (var key in accountBalances.Keys)
                    _accountBalances[key] = accountBalances[key];
                _lastRefresh = lastRefhresh;
            }
        }

        public List<tgsdesktop.models.CustomerTransactionSummary> GetCustomerTransactions(IEnumerable<int> customerIds) {
            this.Db.Reset();
            this.Db.Command.CommandText = @"SELECT gj.personId, txn.id AS txnId, txn.effectiveDate, gj.signedAmt,
	CASE
		WHEN ISNULL(gj.memo,'')<>'' THEN gj.memo
		WHEN ISNULL(txn.memo,'')<>'' THEN txn.memo
		WHEN si.id IS NOT NULL THEN 'Invoice #' + si.invoiceNo
		WHEN sir.id IS NOT NULL THEN 'Return'
		WHEN pmt.id IS NOT NULL THEN 'Payment ' +
			CASE WHEN pmt.methodId=2 THEN 'Check #' + pmt.checkNo
				WHEN pmt.methodId=1 THEN 'Cash'
				WHEN pmt.methodId=3 THEN 'Amex'
				WHEN pmt.methodId=4 THEN 'Visa'
				WHEN pmt.methodId=5 THEN 'MasterCard'
				WHEN pmt.methodId=6 THEN 'Discover'
			END
		ELSE (SELECT TOP 1 xa.name FROM tbl_account xa INNER JOIN tbl_generalJournal xgj ON gj.txnId=xgj.txnId AND xgj.accountId<>101 WHERE xgj.accountId=xa.id)
	END, si.id, sir.id
FROM tbl_generalJournal gj
	INNER JOIN @keys k ON gj.personId=k.id
	INNER JOIN tbl_transaction txn ON gj.txnId=txn.id
	LEFT OUTER JOIN tbl_payment pmt ON txn.id=pmt.txnId
	LEFT OUTER JOIN tbl_salesInvoice si ON txn.id=si.txnId
	LEFT OUTER JOIN tbl_salesInvoiceReturn sir ON txn.id=sir.txnId
WHERE gj.accountId=101
	AND txn.reversedUtc IS NULL
ORDER BY gj.personId, txn.effectiveDate, txn.id";

            var sqlDataRecords = customerIds.Distinct().Select(x => {
                var dataRecord = new SqlDataRecord(new SqlMetaData[] {
                    new SqlMetaData("id", SqlDbType.Int)
                });
                dataRecord.SetInt32(0, x);
                return dataRecord;
            });

            var pkParam = this.Db.Command.Parameters.AddWithValue("@keys", sqlDataRecords);
            pkParam.SqlDbType = SqlDbType.Structured;
            pkParam.TypeName = "udt_intIdArray";

            var retVal = new List<models.CustomerTransactionSummary>();
            using (var dr = this.Db.ExecuteReader()) {
                while (dr.Read()) {
                    int i = 0;
                    retVal.Add(new models.CustomerTransactionSummary {
                        PersonId = dr.GetInt32(i++),
                        TransactionId = dr.GetInt32(i++),
                        EffectiveDate = dr.GetDateTime(i++),
                        Amount = System.Math.Abs(dr.GetDecimal(i)),
                        IsCredit = dr.GetDecimal(i) > 0 ? false : true,
                        Memo =dr.IsDBNull(++i) ? null : dr.GetString(i),
                        InvoiceId = dr.IsDBNull(++i) ? null : (int?)dr.GetInt32(i),
                        ReturnId = dr.IsDBNull(++i) ? null : (int?)dr.GetInt32(i),
                    });
                }
            }
            return retVal;
        }

    }
}
