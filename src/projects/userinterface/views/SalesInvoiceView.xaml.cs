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
using System.Windows.Shapes;

namespace tgsdesktop.views {
    /// <summary>
    /// Interaction logic for POSRegister.xaml
    /// </summary>
    public partial class SalesInvoiceView : UserControl, IViewFor<viewmodels.ISalesInvoiceViewModel> {
        public SalesInvoiceView() {
            InitializeComponent();
            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);

            this.WhenAnyValue(x => x.ViewModel.CurrentCartItem.IsComplete)
                .Subscribe(x => btnAddItem.IsDefault = x);
            this.Loaded += SalesInvoiceView_Loaded;

        }

        void SalesInvoiceView_Loaded(object sender, RoutedEventArgs e) {
            txtInvoiceNumber.Focus();
        }

        public viewmodels.ISalesInvoiceViewModel ViewModel {
            get { return (viewmodels.ISalesInvoiceViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel {
            get { return this.ViewModel; }
            set { this.ViewModel = (viewmodels.ISalesInvoiceViewModel)value; }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(viewmodels.ISalesInvoiceViewModel), typeof(SalesInvoiceView), new PropertyMetadata(null));

        private void acProducts_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        }

        private void txtCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (txtCustomers.SelectedItem != null)
                txtProducts.Focus();
        }

        private void txtProducts_SelectionChanged(object sender, SelectionChangedEventArgs e) {

            if (this.ViewModel.SelectedProduct == null) {
                if (!this.txtProducts.IsFocused)
                    this.txtProducts.Focus();
            } else {
                if (this.ViewModel.SelectedProduct.Price.HasValue)
                    this.txtQuantity.Focus();
                else
                    this.txtPrice.Focus();
            }
        }

    }
}
