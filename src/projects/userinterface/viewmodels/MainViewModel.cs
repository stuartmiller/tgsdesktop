using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace tgsdesktop.viewmodels {
    public class MainViewModel : infrastructure.IMainViewModel, IScreen {

        public MainViewModel()
            : base() {

            this.LogoutCommand = ReactiveCommand.Create();
            this.Router = new RoutingState();
        }

        public ReactiveCommand<object> LogoutCommand { get; private set; }

        public RoutingState Router { get; set; }
    }
}
