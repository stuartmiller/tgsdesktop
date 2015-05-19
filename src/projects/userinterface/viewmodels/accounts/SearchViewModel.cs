//using ReactiveUI;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace tgsdesktop.viewmodels.account {

//    public interface IAccountSearchViewModel : IRoutableViewModel {
//        ReactiveList<viewmodels.account.AccountDetailViewModel> AccountList { get; }
//        AccountDetailViewModel SelectedItem { get; set; }
//    }

//    public abstract class AccountSearchViewModel : ViewModelBase, IAccountSearchViewModel {

//        public AccountSearchViewModel(IScreen screen) : base(screen) {

//            this.ApplyFilter(models.PersonType.Camper | models.PersonType.Staff);

//            this.ClearSelectedItem = ReactiveCommand.Create();
//            this.ClearSelectedItem.Subscribe(_ => {
//                this.SelectedItem = null;
//            });
//        }

//        public void ApplyFilter(models.PersonType? filter) {
//            //var accountService = infrastructure.IocContainer.Resolve<infrastructure.IAccountReceivableService>();
//            //this.AccountList = new ReactiveList<AccountDetailViewModel>(accountService.GetReceivableAccounts(filter).Select(x => {
//            //    AccountDetailViewModel retVal = null;
//            //    if (x is models.Camper)
//            //        retVal = new CamperDetailViewModel((models.Camper)x);
//            //    else if (x is models.Parent)
//            //        retVal = new ParentDetailsViewModel((models.Parent)x);
//            //    else if (x is models.Staff)
//            //        retVal = new StaffDetailsViewModel((models.Staff)x);
//            //    else if (x is models.Family)
//            //        retVal = new FamilyDetailsViewModel((models.Family)x);
//            //    return retVal;
//            //}));
//        }

//        public ReactiveList<viewmodels.account.AccountDetailViewModel> AccountList { get; private set; }

//        public ReactiveCommand<object> ClearSelectedItem { get; private set; }

//        tgsdesktop.viewmodels.transaction.CustomerViewModel _selectedItem;
//        public tgsdesktop.viewmodels.transaction.CustomerViewModel SelectedItem {
//            get { return _selectedItem; }
//            set { this.RaiseAndSetIfChanged(ref _selectedItem, value); }
//        }

//        System.Windows.Visibility _clearAccountSearchVisibility = System.Windows.Visibility.Visible;
//        public System.Windows.Visibility ClearAccountSearchVisibility {
//            get { return _clearAccountSearchVisibility; }
//            set { this.RaiseAndSetIfChanged(ref _clearAccountSearchVisibility, value); }
//        }

//        bool _isSearchBoxFocused;
//        public bool IsSearchBoxFocused {
//            get { return _isSearchBoxFocused; }
//            set { this.RaiseAndSetIfChanged(ref _isSearchBoxFocused, value); }
//        }
//    }
//}
