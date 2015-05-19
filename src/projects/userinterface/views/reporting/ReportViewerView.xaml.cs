//using ReactiveUI;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;
//using System.Windows.Threading;

//namespace tgsdesktop.views.reporting {
//    /// <summary>
//    /// Interaction logic for ReportViewerView.xaml
//    /// </summary>
//    public partial class ReportViewerView : UserControl, IViewFor<viewmodels.reporting.IReportViewerViewModel> {
//        public ReportViewerView() {
//            InitializeComponent();
//            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);

//        }

//        public viewmodels.reporting.IReportViewerViewModel ViewModel {
//            get { return (viewmodels.reporting.IReportViewerViewModel)GetValue(ViewModelProperty); }
//            set { SetValue(ViewModelProperty, value); }
//        }

//        object IViewFor.ViewModel {
//            get { return this.ViewModel; }
//            set { this.ViewModel = (viewmodels.reporting.IReportViewerViewModel)value; }
//        }
//        public static readonly DependencyProperty ViewModelProperty =
//            DependencyProperty.Register("ViewModel", typeof(viewmodels.reporting.IReportViewerViewModel), typeof(ReportViewerView), new PropertyMetadata(null));

//        private void ShippingLabel_Click(object sender, RoutedEventArgs args) {

//            // Create our context, and install it:
//            SynchronizationContext.SetSynchronizationContext(
//                new DispatcherSynchronizationContext(
//                    Dispatcher.CurrentDispatcher));

//            Thread newWindowThread = new Thread(new ThreadStart(() => {
//                // Create and show the Window
//                ReportViewerWindow tempWindow = new ReportViewerWindow();
//                // When the window closes, shut down the dispatcher
//                tempWindow.Closed += (s, e) => Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
//                tempWindow.Show();
//                // Start the Dispatcher Processing
//                System.Windows.Threading.Dispatcher.Run();
//            }));
//            // Set the apartment state
//            newWindowThread.SetApartmentState(ApartmentState.STA);
//            // Make the thread a background thread
//            newWindowThread.IsBackground = true;
//            // Start the thread
//            newWindowThread.Start();
//        }
//    }
//}
