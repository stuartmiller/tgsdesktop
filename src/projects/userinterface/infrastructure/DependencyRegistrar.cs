using Autofac;
using Autofac.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tgsdesktop.infrastructure {

    public class DependencyRegistrar : tgsdesktop.infrastructure.dependencymanagement.IDependencyRegistrar {

        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder) {
            ////we cache presentation models between requests
            //builder.RegisterType<BlogController>()
            //    .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));
            //builder.RegisterType<CatalogController>()
            //    .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));
            //builder.RegisterType<CountryController>()
            //    .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));
            //builder.RegisterType<CommonController>()
            //    .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));
            //builder.RegisterType<NewsController>()
            //    .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));
            //builder.RegisterType<PollController>()
            //    .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));
            //builder.RegisterType<ShoppingCartController>()
            //    .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));
            //builder.RegisterType<TopicController>()
            //    .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));

            ////installation localization service
            //builder.RegisterType<InstallationLocalizationService>().As<IInstallationLocalizationService>().InstancePerHttpRequest();
        }

        public int Order {
            get { return 2; }
        }
    }
}
