using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.services {
    public class SalesInvoiceService : ServiceBase, infrastructure.ISalesInvoiceService {

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
                        Id = dr.GetInt32(i++),
                        IsWebProduct = true,
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
                        Id = dr.GetInt32(i++),
                        IsWebProduct = false,
                        Name = dr.GetString(i),
                        Cost = dr.IsDBNull(++i) ? null : (decimal?)dr.GetDecimal(i),
                        Price = dr.IsDBNull(++i) ? null : (decimal?)dr.GetDecimal(i),
                        IsTaxable = dr.GetBoolean(++i)
                    });
                }
            }
            return retVal;
        }


        public List<models.SalesInvoice> GetSalesInvoices(IList<int> ids) {

            this.Reset();
            this.Command.CommandText = @"SELECT si.id, si.txnId, si.invoiceNo, si.personId 
FROM tbl_salesInvoice si INNER JOIN @keys k ON si.id=k.id";
            var idTable = SqlUdtTypes.GetIdArrayTable(ids.Distinct());
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
                        PersonId = dr.IsDBNull(++i) ? null : (int?)dr.GetInt32(i)
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

            return dict.Values.ToList();
        }

        public models.SalesInvoice AddSalesInvoice(models.AddSalesInvoiceModel invoice) {

            this.Reset();
            this.Command.CommandText = "proc_addSalesInvoice";
            var idParam = this.Command.Parameters.Add("@id", SqlDbType.Int);
            idParam.Direction = System.Data.ParameterDirection.Output;
            this.Command.Parameters.AddWithValue("@invoiceNo", invoice.InvoiceNumber);
            this.Command.Parameters.AddWithValue("seasonId", invoice.SeasonId);
            this.Command.Parameters.AddWithValue("@effectiveDate", invoice.EffectiveDate.HasValue ?
                invoice.EffectiveDate.Value : (object)DBNull.Value);
            var txnMemoParam = this.Command.Parameters.Add("@txnMemo", SqlDbType.NVarChar, 300);
            txnMemoParam.Value = string.IsNullOrEmpty(invoice.TxnMemo) ? (object)DBNull.Value : invoice.TxnMemo;

            var pmtDt = SqlUdtTypes.GetAccountPaymentEntryTable();
            var itmDt = SqlUdtTypes.GetSalesInvoiceItemTable();
            var itemsParam = this.Command.Parameters.Add("@items", SqlDbType.Structured);
            var pmtParam = this.Command.Parameters.Add("@payments", SqlDbType.Structured);

            foreach (var pmt in invoice.Payments)
                pmtDt.Rows.Add((int)pmt.Method, pmt.Amount, pmt.CheckNumber);
            foreach (var item in invoice.Items)
                itmDt.Rows.Add(item.Description, item.ProductId, item.ItemId, item.Price, item.Cost,
                    item.Quantity, item.IsTaxable, item.Discount);

            pmtParam.Value = pmtDt;
            itemsParam.Value = itmDt;

            this.Command.Parameters.AddWithValue("@personId", invoice.Person == null ? (object)DBNull.Value : invoice.Person.Id);
            this.Command.Parameters.AddWithValue("@salesTax", invoice.SalesTax);
            this.Command.Parameters.AddWithValue("@userId", User.Person.Id);

            this.Execute();

            var id = Convert.ToInt32(idParam.Value);

            return this.GetSalesInvoices(new int[] { id}).FirstOrDefault();
        }
    }
}
