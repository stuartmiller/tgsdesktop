using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Core;

namespace tgsdesktop.infrastructure {
    public class IocContainer {

        public static IContainer BaseContainer { get; private set; }

        public static void Build() {
            if (BaseContainer == null) {
                var builder = new ContainerBuilder();
                builder.RegisterType<infrastructure.Cache>().As<infrastructure.ICacheProvider>().SingleInstance();
                builder.RegisterType<services.UserService>().As<IUserService>();
                builder.RegisterType<services.AccountReceivableService>().As<IAccountReceivableService>();
                builder.RegisterType<services.TransactionService>().As<ITransactionService>();
                builder.RegisterType<services.transaction.GeneralJournalService>().As<IGeneralJournalService>();
                builder.RegisterType<services.SalesInvoiceService>().As<ISalesInvoiceService>();
                builder.RegisterType<services.SettingsService>().As<IGlobalSettingsAccessor>();
                BaseContainer = builder.Build();
            }
        }

        public static TService Resolve<TService>() {
            return BaseContainer.Resolve<TService>();
        }
    }
}
