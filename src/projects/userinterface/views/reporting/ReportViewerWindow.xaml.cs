namespace tgsdesktop.views.reporting {
    using System;
    using System.Windows;
    using Telerik.Windows.Controls;

    public partial class ReportViewerWindow : Window {
        public ReportViewerWindow() {
            InitializeComponent();

            var objectDataSource1 = new Telerik.Reporting.ObjectDataSource();
            //objectDataSource1.DataMember = "GetOrderShipments";
            //objectDataSource1.DataSource = typeof(models.OrderShipments);

            var report = new reporting.reports.OrderShippment();
            report.DataSource = new models.OrderShipments().GetOrderShipments();
            //report.DataSource = objectDataSource1;

            var reportSource = new Telerik.Reporting.InstanceReportSource();
            reportSource.ReportDocument = report;
            this.ReportViewer1.ReportSource = reportSource;
        }
    }
}