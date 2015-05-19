﻿using Autofac;
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
                builder.RegisterType<services.LoginService>().As<ILoginService>();
                builder.RegisterType<services.AccountReceivableService>().As<IAccountReceivableService>();
                builder.RegisterType<services.TransactionService>().As<ITransactionService>();
                builder.RegisterType<services.transaction.GeneralJournalService>().As<IGeneralJournalService>();
                builder.RegisterType<services.PointOfSaleService>().As<IPointOfSaleService>();
                builder.RegisterType<services.SettingsService>().As<IGlobalSettingsAccessor>();
                BaseContainer = builder.Build();
            }
        }

        public static TService Resolve<TService>() {
            return BaseContainer.Resolve<TService>();
        }
    }
}
