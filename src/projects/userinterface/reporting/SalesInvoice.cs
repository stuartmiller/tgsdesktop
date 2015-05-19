namespace tgsdesktop.reporting {
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using Telerik.Reporting;
    using Telerik.Reporting.Drawing;

    /// <summary>
    /// Summary description for SalesInvoice.
    /// </summary>
    public partial class SalesInvoice : Telerik.Reporting.Report {
        public SalesInvoice() {
            //
            // Required for telerik Reporting designer support
            //
            InitializeComponent();

            //this.InvoiceDataSource.ConnectionString = 
        }
    }
}