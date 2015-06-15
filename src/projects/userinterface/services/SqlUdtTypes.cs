using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.services {
    internal static class SqlUdtTypes {

        internal static DataTable GetPaymentEntryTable() {
            var dt = new DataTable("udt_payment");
            dt.Columns.Add("methodId", typeof(int));
            dt.Columns.Add("amount", typeof(decimal));
            dt.Columns.Add("checkNo", typeof(string));
            return dt;
        }
        internal static DataTable GetAccountPaymentEntryTable() {
            var dt = new DataTable("udt_payment");
            dt.Columns.Add("personId", typeof(int));
            dt.Columns.Add("amount", typeof(decimal));
            return dt;
        }

        internal static DataTable GetAccountJournalEntryTable() {
            var dt = new DataTable("udt_accountJournalEntry");
            dt.Columns.Add("accountId", typeof(int));
            dt.Columns.Add("seasonId", typeof(int));
            dt.Columns.Add("amount", typeof(decimal));
            dt.Columns.Add("isCredit", typeof(bool));
            dt.Columns.Add("personId", typeof(int));
            dt.Columns.Add("memo", typeof(string));
            return dt;
        }

        internal static DataTable GetSalesInvoiceItemTable() {
            var dt = new DataTable("udt_salesInvoiceItem");
            dt.Columns.Add("description", typeof(string));
            dt.Columns.Add("productId", typeof(int));
            dt.Columns.Add("itemId", typeof(int));
            dt.Columns.Add("unitPrice", typeof(decimal));
            dt.Columns.Add("unitCost", typeof(decimal));
            dt.Columns.Add("quantity", typeof(int));    
            dt.Columns.Add("isTaxable", typeof(bool));
            dt.Columns.Add("discount", typeof(decimal));
            return dt;
        }

        internal static DataTable GetIdArrayTable(IEnumerable<int> ids) {
            var dt = new DataTable("udt_intIdArray");
            dt.Columns.Add("id", typeof(int));
            foreach (var id in ids)
                dt.Rows.Add(id);
            return dt;
        }

        internal static DataTable GetDualKeyTable(IEnumerable<Tuple<int, int>> keys) {
            var dt = new DataTable("udt_int-intArray");
            dt.Columns.Add("key1", typeof(int));
            dt.Columns.Add("key2", typeof(int));
            foreach (var key in keys)
                dt.Rows.Add(key.Item1, key.Item2);
            return dt;
        }

    }
}
