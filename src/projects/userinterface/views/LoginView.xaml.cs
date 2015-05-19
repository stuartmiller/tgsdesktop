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
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl, IViewFor<viewmodels.ILoginViewModel> {
        public LoginView() {
            InitializeComponent();
            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);
            //this.ViewModel = new viewmodels.LoginViewModel((() => this.txtPassword.Clear()));
            //this.ViewModel = new viewmodels.LoginViewModel();
            //this.DataContext = this.ViewModel;
            // this subscribes to the LoginSuccessful command in the ViewModel, and will then navigate to the home view if the
            // login attempt was successful
            //this.WhenAnyObservable(x => x.ViewModel.LoginSuccessful).Subscribe(x => this.NavigationService.Navigate(new views.HomeView()));
        }

        // These two methods are required because of the unique nature of the PasswordBox.
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e) {
            if (this.DataContext != null)
                ((dynamic)this.DataContext).SecurePassword = ((PasswordBox)sender).SecurePassword;
        }
        private void TextBox_KeyUp(object sender, KeyEventArgs e) {
            ((dynamic)this.DataContext).FormComplete = !string.IsNullOrEmpty(this.txtUsername.Text) && this.txtPassword.SecurePassword.Length > 0;
        }

        public viewmodels.ILoginViewModel ViewModel {
            get { return (viewmodels.ILoginViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel {
            get { return this.ViewModel; }
            set { this.ViewModel = (viewmodels.ILoginViewModel)value; }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(viewmodels.ILoginViewModel), typeof(LoginView), new PropertyMetadata(null));
    }
}
