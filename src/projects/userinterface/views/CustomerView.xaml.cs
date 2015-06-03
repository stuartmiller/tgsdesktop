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
    /// Interaction logic for CustomerView.xaml
    /// </summary>
    public partial class CustomerView : UserControl, IViewFor<viewmodels.ICustomerViewModel> {
        public CustomerView() {
            InitializeComponent();
            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);
        }

        public viewmodels.ICustomerViewModel ViewModel {
            get { return (viewmodels.ICustomerViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel {
            get { return this.ViewModel; }
            set { this.ViewModel = (viewmodels.ICustomerViewModel)value; }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(viewmodels.ICustomerViewModel), typeof(CustomerView), new PropertyMetadata(null));
    }
}
