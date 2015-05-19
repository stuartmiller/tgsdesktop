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
using System.Windows.Threading;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace tgsdesktop.views.transaction {
    /// <summary>
    /// Interaction logic for TransactionEntryView.xaml
    /// </summary>
    public partial class TransactionEntryView : UserControl, IViewFor<viewmodels.transaction.ITransactionEntryViewModel> {
        public TransactionEntryView() {
            InitializeComponent();
            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);

            this.WhenAnyObservable(v => v.ViewModel.AddPayment).Subscribe(_ => this.txtFinancialAccounts.Focus());
            this.WhenAnyObservable(v => v.ViewModel.AddJournalEntry).Subscribe(x => this.txtFinancialAccounts.Focus());
            this.WhenAnyObservable(v => v.ViewModel.AddTransaction).Subscribe(_ => this.txtCheckNo.Focus());
        }

        public viewmodels.transaction.ITransactionEntryViewModel ViewModel {
            get { return (viewmodels.transaction.ITransactionEntryViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel {
            get { return this.ViewModel; }
            set { this.ViewModel = (viewmodels.transaction.ITransactionEntryViewModel)value; }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(viewmodels.transaction.ITransactionEntryViewModel),
            typeof(TransactionEntryView), new PropertyMetadata(null));

        private void txtFinancialAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (txtFinancialAccounts.SelectedItem != null)
                txtCustomers.Focus();
        }

        private void txtCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (txtCustomers.SelectedItem != null)
                txtAmount.Focus();
        }

    }
}
