using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.viewmodels {
    /// <summary>
    /// This is where the application begins. It is the ViewModel for the main window container and handles the navigation state.
    /// </summary>
    public class AppBootStrapper : ReactiveObject, IScreen {

        public RoutingState Router { get; private set; }

        public AppBootStrapper(IMutableDependencyResolver dependencyResolver = null, RoutingState testRouter = null) {

            this.Router = testRouter ?? new RoutingState();
            dependencyResolver = dependencyResolver ?? Locator.CurrentMutable;

            // bind
            this.RegisterParts(dependencyResolver);

            LogHost.Default.Level = LogLevel.Debug;

            // Navigate to the opening page of the application
            Router.Navigate.Execute(new LoginViewModel(this));
        }

        private void RegisterParts(IMutableDependencyResolver dependencyResolver) {
            dependencyResolver.RegisterConstant(this, typeof(IScreen));

            dependencyResolver.Register(() => new views.LoginView(), typeof(IViewFor<LoginViewModel>));
            dependencyResolver.Register(() => new views.HomeView(), typeof(IViewFor<HomeViewModel>));
            dependencyResolver.Register(() => new views.StoreSalesView(), typeof(IViewFor<StoreSalesViewModel>));
            dependencyResolver.Register(() => new views.SalesInvoiceView(), typeof(IViewFor<SalesInvoiceViewModel>));
            //dependencyResolver.Register(() => new views.account.HomeView(), typeof(IViewFor<viewmodels.account.HomeViewModel>));
            //dependencyResolver.Register(() => new views.account.PaymentEntryView(), typeof(IViewFor<viewmodels.account.PaymentEntryViewModel>));
            //dependencyResolver.Register(() => new views.account.AccountSearchView(), typeof(IViewFor<viewmodels.account.AccountSearchViewModel>));
            //dependencyResolver.Register(() => new views.reporting.ReportViewerView(), typeof(IViewFor<viewmodels.reporting.ReportViewerViewModel>));
            dependencyResolver.Register(() => new views.transaction.TransactionEntryView(), typeof(IViewFor<viewmodels.transaction.TransactionEntryViewModel>));
            dependencyResolver.Register(() =>
                new views.SalesInvoiceCheckoutView(),
                typeof(IViewFor<viewmodels.SalesInviceCheckoutViewModel>));
        }

    }
}
