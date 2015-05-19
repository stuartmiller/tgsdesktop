
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Telerik.Reporting;
using Telerik.Reporting.Drawing;

namespace tgsdesktop.views.reporting.reports {
    /// <summary>
    /// Summary description for ShippingLabel.
    /// </summary>
    public partial class OrderShippment : Telerik.Reporting.Report {
        public OrderShippment() {
            //
            // Required for telerik Reporting designer support
            //
            InitializeComponent();
        }


        #region Class Method - SetSubReport
        /// <summary>
        /// A method which sets the subreport specified to the datatable provided
        /// </summary>
        /// <param name="u">The current Utility class</param>
        /// <param name="rpt">the source report to search within</param>
        /// <param name="subReportName">the name of the subreport object to find</param>
        /// <param name="dataTable">the datatable to bind to your report</param>
        /// <param name="hideIfEmpty">True to hide the subreport if the table is empty, false otherwise</param>
        /// <returns>The report object of the subreport found</returns>
        private static Telerik.Reporting.Report SetSubReport(
            //Telerik.Reporting.Report rpt,
            //String subReportName,
            //DataTable dataTable,
            //Boolean hideIfEmpty
            ) {
            Telerik.Reporting.Report actualSubReport = null;

            //try {
            //    if ((String.IsNullOrWhiteSpace(subReportName) == false) && (rpt != null) && (dataTable != null)) {
            //        Telerik.Reporting.SubReport subReport = rpt.Items.Find(subReportName, true)[0] as Telerik.Reporting.SubReport;
            //        if (subReport != null) {
            //            Telerik.Reporting.InstanceReportSource subReportSource = subReport.ReportSource as Telerik.Reporting.InstanceReportSource;
            //            if (subReportSource != null) {
            //                actualSubReport = subReportSource.ReportDocument as Telerik.Reporting.Report;
            //                if (actualSubReport != null) {
            //                    if ((hideIfEmpty == true) &&
            //                        ((dataTable == null) |
            //                         ((dataTable != null) &&
            //                          (dataTable.Rows.Count < 1)))) {
            //                        subReport.Visible = false;
            //                    } else {
            //                        Telerik.Reporting.ObjectDataSource odsSalary = actualSubReport.DataSource as Telerik.Reporting.ObjectDataSource;
            //                        if (odsSalary != null) {
            //                            odsSalary.DataSource = dataTable;
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //} catch (Exception) {
            //    throw;
            //}
            return actualSubReport;
        }
        #endregion


    }
}