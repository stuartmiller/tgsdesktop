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
    /// Interaction logic for SalesInvoice2View.xaml
    /// </summary>
    public partial class SalesInvoice2View : UserControl, IViewFor<viewmodels.ISalesInvoice2ViewModel> {
        public SalesInvoice2View() {
            InitializeComponent();
            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);
            this.WhenAnyObservable(x => x.ViewModel.SaveTransaction).Subscribe(_ => {
                this.txtInvoiceNumber.Focus();
                this.txtInvoiceNumber.SelectAll();
            });

            this.Loaded += SalesInvoice2View_Loaded;

        }

        void SalesInvoice2View_Loaded(object sender, RoutedEventArgs e) {
            this.txtInvoiceNumber.Focus();
        }
        public viewmodels.ISalesInvoice2ViewModel ViewModel {
            get { return (viewmodels.ISalesInvoice2ViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel {
            get { return this.ViewModel; }
            set { this.ViewModel = (viewmodels.ISalesInvoice2ViewModel)value; }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(viewmodels.ISalesInvoice2ViewModel), typeof(SalesInvoice2View), new PropertyMetadata(null));
    }
}
