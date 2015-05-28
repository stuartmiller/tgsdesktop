using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace tgsdesktop.views {
    /// <summary>
    /// Interaction logic for TransactionsView.xaml
    /// </summary>
    public partial class SalesTransactionsView : UserControl, IViewFor<viewmodels.ISalesTransactionsViewModel> {
        public SalesTransactionsView() {
            InitializeComponent();
            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);
        }

        public viewmodels.ISalesTransactionsViewModel ViewModel {
            get { return (viewmodels.ISalesTransactionsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel {
            get { return this.ViewModel; }
            set { this.ViewModel = (viewmodels.ISalesTransactionsViewModel)value; }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(viewmodels.ISalesTransactionsViewModel), typeof(SalesTransactionsView), new PropertyMetadata(null));
    }
}
