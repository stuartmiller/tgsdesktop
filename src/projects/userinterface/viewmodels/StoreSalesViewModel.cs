using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.viewmodels {

    public interface IStoreSalesViewModel : IRoutableViewModel {
    }

    public class StoreSalesViewModel : ViewModelBase, IStoreSalesViewModel {

        public StoreSalesViewModel(IScreen screen)
            : base(screen) {

        }

        public override string UrlPathSegment { get { return "storesales"; } }
    }
}
