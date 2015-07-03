using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.services {
    public class SalesInvoiceService : ServiceBase, infrastructure.ISalesInvoiceService {

        public models.Product GetItem(int itemId) {
            return this.GetItems(new int[] {itemId}).FirstOrDefault();
        }
        public List<models.Product> GetItems(IEnumerable<int> itemIds) {

            this.Reset();
            this.Command.CommandText = @"SELECT i.id, i.description, i.cost, i.price, i.isTaxable
FROM tbl_salesItem i
    INNER JOIN @t t ON i.id=t.id";
            var dt = SqlUdtTypes.GetIdArrayTable(itemIds);
            var tblParam = this.Command.Parameters.Add("@t", SqlDbType.Structured);
            tblParam.TypeName = dt.TableName;
            tblParam.Value = dt;

            var retVal = new List<models.Product>();
            using (var dr = this.ExecuteReader(CommandBehavior.SingleRow)) {
                while (dr.Read()) {
                    var i = 0;
                    retVal.Add(new models.Product {
                        ItemId = dr.GetInt32(i++),
                        Name = dr.GetString(i),
                        Cost = dr.IsDBNull(++i) ? null : (decimal?)dr.GetDecimal(i),
                        Price = dr.IsDBNull(++i) ? null : (decimal?)dr.GetDecimal(i),
                        IsTaxable = dr.GetBoolean(++i)
                    });
                }
            }
            return retVal;
        }

        public IList<models.Product> GetProducts() {

            this.Reset();
            this.Command.CommandText = @"SELECT p.id, p.Name, p.ProductCost, p.Price, p.OldPrice, p.specialPrice, p.IsTaxExempt,
	(SELECT TOP 1 PictureId FROM [Product_Picture_Mapping] WHERE productId=p.id ORDER BY displayOrder) AS pictureId
FROM [Product] p
WHERE Published=1 AND Deleted=0";
            var retVal = new List<models.Product>();
            using (var dr = this.ExecuteReader()) {
                while (dr.Read()) {
                    var i = 0;
                    retVal.Add(new models.Product {
                        ProductId = dr.GetInt32(i++),
                        Name = dr.GetString(i++),
                        Cost = dr.GetDecimal(i++),
                        Price = dr.GetDecimal(i++),
                        OldPrice = dr.GetDecimal(i),
                        SpecialPrice = dr.IsDBNull(++i) ? null : (decimal?)dr.GetDecimal(i),
                        IsTaxable = !dr.GetBoolean(++i)
                    });
                }
            }
            this.Reset();
            this.Command.CommandText = @"SELECT i.id, i.description, i.cost, i.price, i.isTaxable
FROM tbl_salesItem i";
            using (var dr = this.ExecuteReader()) {
                while (dr.Read()) {
                    var i = 0;
                    retVal.Add(new models.Product {
                        ItemId = dr.GetInt32(i++),
                        Name = dr.GetString(i),
                        Cost = dr.IsDBNull(++i) ? null : (decimal?)dr.GetDecimal(i),
                        Price = dr.IsDBNull(++i) ? null : (decimal?)dr.GetDecimal(i),
                        IsTaxable = dr.GetBoolean(++i)
                    });
                }
            }
            return retVal;
        }

        public List<models.SalesInvoice> GetSalesInvoices(IEnumerable<int> ids) {

            ids = ids.Distinct();
            var cache = infrastructure.IocContainer.Resolve<infrastructure.ICacheProvider>();
            var retVal = cache.GetItems<models.SalesInvoice>(ids.Select(x => "model.salesinvoice:pk:" + x.ToString()));

            var missingIds = ids.Except(retVal.Select(x => x.Id)).ToList();

            if (missingIds.Count > 0) {

                this.Reset();
                this.Command.CommandText = @"SELECT si.id, si.txnId, si.invoiceNo, si.personId, seasonId, si.salesTaxRate, si.discountPercentage
FROM tbl_salesInvoice si INNER JOIN @keys k ON si.id=k.id";
                var idTable = SqlUdtTypes.GetIdArrayTable(missingIds);
                var keysParam = this.Command.Parameters.AddWithValue("@keys", SqlDbType.Structured);
                keysParam.TypeName = "udt_intIdArray";
                keysParam.Value = idTable;

                var dict = new Dictionary<int, models.SalesInvoice>();
                using (var dr = this.ExecuteReader()) {
                    while (dr.Read()) {
                        int i = 0;
                        var id = dr.GetInt32(i++);
                        dict.Add(id, new models.SalesInvoice {
                            Id = id,
                            TransactionId = dr.GetInt32(i++),
                            InvoiceNumber = dr.GetString(i),
                            PersonId = dr.IsDBNull(++i) ? null : (int?)dr.GetInt32(i),
                            SeasonId = dr.GetInt32(++i),
                            SalesTaxRate = dr.GetDecimal(++i),
                            DiscountPercentage = dr.GetDecimal(++i)
                        });
                    }
                }

                this.Command.CommandText = @"SELECT i.id, i.description, i.invoiceId, i.productId, i.itemId, i.quantity, i.unitCost,
    i.unitPrice, i.isTaxable, i.discount
FROM tbl_salesInvoiceItem i
    INNER JOIN @keys k ON i.invoiceId=k.id";

                using (var dr = this.ExecuteReader()) {
                    while (dr.Read()) {
                        int i = 0;
                        var item = new models.SalesInvoice.SalesInvoiceItem {
                            Id = dr.GetInt32(i++),
                            Description = dr.GetString(i++),
                            InvoiceId = dr.GetInt32(i),
                            ProductId = dr.IsDBNull(++i) ? null : (int?)dr.GetInt32(i),
                            ItemId = dr.IsDBNull(++i) ? null : (int?)dr.GetInt32(i),
                            Quantity = dr.GetInt32(++i),
                            Cost = dr.IsDBNull(++i) ? null : (decimal?)dr.GetDecimal(i),
                            Price = dr.GetDecimal(++i),
                            IsTaxable = dr.GetBoolean(++i),
                            Discount = dr.GetDecimal(++i)
                        };
                        dict[item.InvoiceId].Items.Add(item);
                    }
                }


                this.Command.CommandText = @"SELECT ri.id, r.invoiceId, r.txnId, ri.itemId, ri.quantity
FROM tbl_salesInvoiceReturn r
    INNER JOIN tbl_salesInvoiceReturnItem ri ON r.id=ri.returnId
    INNER JOIN @keys k ON r.invoiceId=k.id";
                
                using (var dr = this.ExecuteReader()) {
                    while (dr.Read()) {
                        int i = 0;
                        var item = new models.SalesInvoice.SalesInvoiceItemReturn {
                            Id= dr.GetInt32(i++),
                            InvoiceId = dr.GetInt32(i++),
                            TxnId = dr.GetInt32(i++),
                            ItemId = dr.GetInt32(i++),
                            Quantity = dr.GetInt32(i++)
                        };
                        dict[item.InvoiceId].ReturnItems.Add(item);
                    }
                }

                var items = dict.Values.ToList();
                cache.AddItems(items.Select(x => new infrastructure.model.CacheItem { Key = "model.salesinvoice:pk:" + x.Id.ToString(), Value = x }));
                retVal.AddRange(items);
            }
            return retVal;
        }

        public models.SalesInvoice AddSalesInvoice(models.AddSalesInvoiceModel invoice) {

            this.Reset();
            this.Command.CommandText = "proc_addSalesInvoice";
            var idParam = this.Command.Parameters.Add("@id", SqlDbType.Int);
            idParam.Direction = System.Data.ParameterDirection.Output;
            this.Command.Parameters.AddWithValue("@invoiceNo", invoice.InvoiceNumber);
            this.Command.Parameters.AddWithValue("@discountPercentage", invoice.DiscountPercentage);
            this.Command.Parameters.AddWithValue("seasonId", invoice.SeasonId);
            this.Command.Parameters.AddWithValue("@effectiveDate", invoice.EffectiveDate.HasValue ?
                invoice.EffectiveDate.Value : (object)DBNull.Value);
            var txnMemoParam = this.Command.Parameters.Add("@txnMemo", SqlDbType.NVarChar, 300);
            txnMemoParam.Value = string.IsNullOrEmpty(invoice.TxnMemo) ? (object)DBNull.Value : invoice.TxnMemo;

            var pmtDt = SqlUdtTypes.GetPaymentEntryTable();
            var aPmtDt = SqlUdtTypes.GetAccountPaymentEntryTable();
            var itmDt = SqlUdtTypes.GetSalesInvoiceItemTable();
            var itemsParam = this.Command.Parameters.Add("@items", SqlDbType.Structured);
            var pmtParam = this.Command.Parameters.Add("@payments", SqlDbType.Structured);
            var actPmtParam = this.Command.Parameters.Add("@accountPayments", SqlDbType.Structured);

            foreach (var pmt in invoice.Payments)
                pmtDt.Rows.Add((int)pmt.Method, pmt.Amount, pmt.CheckNumber);
            foreach (var pmt in invoice.AccountPayments)
                aPmtDt.Rows.Add(pmt.PersonId, pmt.Amount);
            foreach (var item in invoice.Items)
                itmDt.Rows.Add(item.Description, item.ProductId, item.ItemId, item.Price, item.Cost,
                    item.Quantity, item.IsTaxable, item.Discount);

            if (pmtDt.Rows.Count > 0)
                pmtParam.Value = pmtDt;
            if (aPmtDt.Rows.Count > 0)
                actPmtParam.Value = aPmtDt;
            itemsParam.Value = itmDt;

            this.Command.Parameters.AddWithValue("@personId", invoice.PersonId.HasValue ? invoice.PersonId.Value : (object)DBNull.Value);
            this.Command.Parameters.AddWithValue("@salesTax", invoice.SalesTax);
            this.Command.Parameters.AddWithValue("@userId", User.Person.Id);

            this.Execute();

            var id = Convert.ToInt32(idParam.Value);

            new AccountReceivableService().RefreshBalances();

            return this.GetSalesInvoices(new int[] { id}).FirstOrDefault();

        }

        public models.SalesInvoice RefundInvoiceItems(int invoiceId, IEnumerable<Tuple<int, int>> itemIdQty) {
            this.Db.Reset();
            this.Db.Command.CommandText = "proc_addSalesInvoiceReturn";
            this.Db.Command.CommandType = CommandType.StoredProcedure;
            this.Db.Command.Parameters.AddWithValue("@invoiceId", invoiceId);

            var itemsTbl = SqlUdtTypes.GetDualKeyTable(itemIdQty);
            var itemsParam = this.Db.Command.Parameters.Add("@items", SqlDbType.Structured);
            itemsParam.Value = itemsTbl;

            this.Db.Command.Parameters.AddWithValue("@userId", User.Person.Id);

            this.Db.Execute();

            var cache = infrastructure.IocContainer.Resolve<infrastructure.ICacheProvider>();
            cache.RemoveItem("model.salesinvoice:pk:" + invoiceId);

            return this.GetSalesInvoices(new int[] {invoiceId}).First();
        }

        static List<models.SalesInvoiceSummary> _salesInvoiceSummaryList;
        static object _salesInvoiceSummaryListLock = new object();
        static DateTime _lastFetch = DateTime.Now.AddYears(-100);
        static System.Timers.Timer _timer;

        public async Task<List<models.SalesInvoiceSummary>> GetTransactionSummariesAsync() {
            if (_salesInvoiceSummaryList == null) {
                await RefreshTransactionSummariesAsync();
                _timer = new System.Timers.Timer();
                _timer.Interval = 60000;
                _timer.Elapsed += _timer_Elapsed;
                _timer.Enabled = true;
                _timer.Start();
            }
            return _salesInvoiceSummaryList;
        }

        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
            _timer.Stop();
            RefreshTransactionSummariesAsync();
            _timer.Start();
        }

        static async Task RefreshTransactionSummariesAsync() {
            var db = new Db();
            db.Reset();
            db.Command.CommandText = "SELECT GETUTCDATE()";
            var dt = DateTime.SpecifyKind(db.ExecuteScaler<DateTime>(), DateTimeKind.Utc);

            db.Command.CommandText = @"SELECT id, effectiveDate, invoiceNo, personId, name, taxableSales,
    nontaxableSales, discounts, salesTax, total, refunded
FROM view_salesInvoiceSummary
WHERE postDateUtc>=@lastFetch
ORDER BY effectiveDate DESC, id DESC";
            db.Command.Parameters.AddWithValue("@lastFetch", _lastFetch);

            var retVal = new List<models.SalesInvoiceSummary>();
            using (var dr = await db.ExecuteReaderAsync()) {
                while (dr.Read()) {
                    int i = 0;
                    retVal.Add(new models.SalesInvoiceSummary {
                        Id = dr.GetInt32(i++),
                        EffectiveDate = dr.GetDateTime(i++),
                        InvoiceNumber = dr.GetString(i),
                        PersonId = dr.IsDBNull(++i) ? null : (int?)dr.GetInt32(i),
                        PersonName = dr.IsDBNull(++i) ? null : dr.GetString(i),
                        TaxableSales = dr.GetDecimal(++i),
                        NonTaxableSales = dr.GetDecimal(++i),
                        Discounts = dr.GetDecimal(++i),
                        SalesTax = dr.GetDecimal(++i),
                        Total = dr.GetDecimal(++i),
                        Refunded = dr.GetDecimal(++i),
                    });
                }
            }
            lock (_salesInvoiceSummaryListLock) {
                if (_salesInvoiceSummaryList == null)
                    _salesInvoiceSummaryList = retVal;
                else {
                    _salesInvoiceSummaryList.AddRange(retVal);
                }
                _lastFetch = dt;
            }
        }
    }
}
