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
using ReactiveUI;

namespace tgsdesktop.views {
    /// <summary>
    /// Interaction logic for StoreSalesView.xaml
    /// </summary>
    public partial class StoreSalesView : UserControl, IViewFor<viewmodels.IStoreSalesViewModel> {
        public StoreSalesView() {
            InitializeComponent();
            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);
        }

        public viewmodels.IStoreSalesViewModel ViewModel {
            get { return (viewmodels.IStoreSalesViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel {
            get { return this.ViewModel; }
            set { this.ViewModel = (viewmodels.IStoreSalesViewModel)value; }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(viewmodels.IStoreSalesViewModel), typeof(StoreSalesView), new PropertyMetadata(null));

    }
}
