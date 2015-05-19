using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace tgsdesktop.views.account {
    /// <summary>
    /// Interaction logic for ArAccountLocator.xaml
    /// </summary>
    public partial class AccountSearchView : UserControl {
        public AccountSearchView() {
            InitializeComponent();
            //this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);

            // this will set the focus to the searchbox once the control is loaded.
            //this.Loaded += new RoutedEventHandler(Control_Loaded);

            //this.WhenAny(x => x.ViewModel.SelectedItem, x => x.Value == null).Subscribe(_ => {
            //    if (_ == true)
            //        this.SearchBox.SelectedItem = null;
            //});

            //this.GotFocus += AccountSearchView_GotFocus;
            this.SearchBox.GotFocus += SearchBox_GotFocus;
        }

        void SearchBox_GotFocus(object sender, System.Windows.RoutedEventArgs e) {
        }

        void AccountSearchView_GotFocus(object sender, System.Windows.RoutedEventArgs e) {

            this.SearchBox.Focus();
        }

        //void Control_Loaded(object sender, RoutedEventArgs e) {
        //    this.SearchBox.Focus();
        //}
        //public viewmodels.account.IAccountSearchViewModel ViewModel {
        //    get { return (viewmodels.account.IAccountSearchViewModel)GetValue(ViewModelProperty); }
        //    set { SetValue(ViewModelProperty, value); }
        //}

        //object IViewFor.ViewModel {
        //    get { return this.ViewModel; }
        //    set { this.ViewModel = (viewmodels.account.IAccountSearchViewModel)value; }
        //}
        //public static readonly DependencyProperty ViewModelProperty =
        //    DependencyProperty.Register("ViewModel", typeof(viewmodels.account.IAccountSearchViewModel), typeof(AccountSearchView), new PropertyMetadata(null));

    }
}
