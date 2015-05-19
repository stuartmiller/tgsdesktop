using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.viewmodels {

    public interface IViewModelBase {
        ReactiveCommand<object> GoHome { get; }
        ReactiveCommand<object> Logout { get; }
        ReactiveCommand<object> GoBack { get; }
        ReactiveCommand<object> GoForward { get; }
    }

    public class ViewModelBase : ReactiveObject, IRoutableViewModel, IViewModelBase {

        public ViewModelBase(IScreen screen) {
            this.HostScreen = screen;

            this.GoHome = this.RegisterNavigationCommand(() => new HomeViewModel(HostScreen));
            this.GoBack = ReactiveCommand.Create();//this.WhenAny(x => x.HostScreen.Router.n, (a) => a. == true));
            this.GoBack.Subscribe(_ => {
                this.HostScreen.Router.NavigateBack.Execute(null);
            });
            this.Logout = ReactiveCommand.Create();
            this.Logout.Subscribe(_ => {
                this.HostScreen.Router.NavigateAndReset.Execute(new LoginViewModel(HostScreen));
            });
        }

        public ReactiveCommand<object> GoHome { get; private set; }
        public ReactiveCommand<object> GoBack { get; private set; }
        public ReactiveCommand<object> Logout { get; private set; }
        public ReactiveCommand<object> GoForward { get; private set; }

        public IScreen HostScreen { get; protected set; }

        public virtual string UrlPathSegment {
            get { throw new NotImplementedException(); }
        }

        protected ReactiveCommand<object> RegisterNavigationCommand(Func<ViewModelBase> fact) {
            var cmd = ReactiveCommand.Create();

            cmd.Subscribe(_ => {
                this.HostScreen.Router.Navigate.Execute(fact());
            });
            return cmd;
        }
    }
}
