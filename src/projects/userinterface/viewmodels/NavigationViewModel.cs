using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.viewmodels {

    public interface INavigationViewModel {
    }

    public class NavigationViewModel : ViewModelBase, INavigationViewModel {

        public NavigationViewModel(IScreen screen) : base(screen) {

        }
    }
}
