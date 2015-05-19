using Autofac;

namespace tgsdesktop.infrastructure.dependencymanagement {
    public interface IDependencyRegistrar {

        void Register(ContainerBuilder builder, ITypeFinder typeFinder);

        int Order { get; }
    }
}
