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
    /// Interaction logic for SalesInvoiceConfirm.xaml
    /// </summary>
    public partial class SalesInvoiceCheckoutView : UserControl, IViewFor<viewmodels.ISalesInvoiceCheckoutViewModel> {
        public SalesInvoiceCheckoutView() {
            InitializeComponent();
            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);
        }

        public viewmodels.ISalesInvoiceCheckoutViewModel ViewModel {
            get { return (viewmodels.ISalesInvoiceCheckoutViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel {
            get { return this.ViewModel; }
            set { this.ViewModel = (viewmodels.ISalesInvoiceCheckoutViewModel)value; }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel",
            typeof(viewmodels.ISalesInvoiceCheckoutViewModel),
            typeof(SalesInvoiceCheckoutView),
            new PropertyMetadata(null));

    }
}
