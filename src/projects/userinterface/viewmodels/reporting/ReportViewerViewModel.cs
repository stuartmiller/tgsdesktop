//using ReactiveUI;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using tgsdesktop.viewmodels.account;

//namespace tgsdesktop.viewmodels.reporting {

//    public interface IReportViewerViewModel : IRoutableViewModel, IAccountSearchViewModel {
//        Telerik.Reporting.Report Report { get; }
//        ReactiveCommand<object> RunReport { get; }
//    }
//    public class ReportViewerViewModel : AccountSearchViewModel, IReportViewerViewModel {

//        public ReportViewerViewModel(IScreen screen)
//            : base(screen) {

//            this.RunReport = ReactiveCommand.Create();
//            this.RunReport.Subscribe(_ => {
//                this.Report = new tgsdesktop.views.reporting.reports.OrderShippment();
//            });
//        }

//        public override string UrlPathSegment { get { return "reporting/reportview"; } }

//        public ReactiveCommand<object> RunReport { get; private set; }

//        Telerik.Reporting.Report _report;
//        public Telerik.Reporting.Report Report {
//            get { return _report; }
//            set { this.RaiseAndSetIfChanged(ref _report, value); }
//        }
//    }
//}
