using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using System.Security;

namespace tgsdesktop.viewmodels {

    public interface ILoginViewModel : IRoutableViewModel {
        ReactiveCommand<object> LoginSuccessful { get; }
        bool FormComplete { get; set; }
    }

    public class LoginViewModel : ViewModelBase, ILoginViewModel {


        public LoginViewModel(IScreen screen)
            : base(screen) {

            var loginService = infrastructure.IocContainer.Resolve<infrastructure.IUserService>();

            this.LoginCommand = ReactiveCommand.Create(this.WhenAny(x => x.FormComplete, (a) => a.Value == true));
            this.LoginCommand.Subscribe(_ => {
                try {
                    loginService.Login(this.UserName, this.SecurePassword);
                    this.UserName = null;
                    this.SecurePassword = null;
                    this.LoginFailedVisibility = System.Windows.Visibility.Hidden;

                    this.GoHome.Execute(null);
                } catch (models.InvalidLoginException ex) {
                    this.LoginFailedVisibility = System.Windows.Visibility.Visible;
                    this.LoginFailedText = ex.Message == string.Empty ? "Login Failed" : ex.Message;
                } catch (Exception) {
                    throw;
                }
            });
        }

        public override string UrlPathSegment {
            get { return "login"; }
        }

        string _userName;
        public string UserName {
            get { return _userName; }
            set { this.RaiseAndSetIfChanged(ref _userName, value); }
        }

        bool _formComplete;
        public bool FormComplete {
            get { return _formComplete; }
            set { this.RaiseAndSetIfChanged(ref _formComplete, value); }
        }

        string _loginFailedText;
        public string LoginFailedText {
            get { return _loginFailedText; }
            set { this.RaiseAndSetIfChanged(ref _loginFailedText, value); }
        }

        System.Windows.Visibility _loginFailedVisibility = System.Windows.Visibility.Hidden;
        public System.Windows.Visibility LoginFailedVisibility {
            get { return this._loginFailedVisibility; }
            private set { this.RaiseAndSetIfChanged(ref _loginFailedVisibility, value); }
        }


        public SecureString SecurePassword { private get; set; }

        public ReactiveCommand<object> CancelCommand { get; set; }
        public ReactiveCommand<object> LoginCommand { get; set; }
        public ReactiveCommand<object> LoginSuccessful { get; private set; }

    }
}
