using Autofac;
using System.Reflection;
using formneo.core.Repositories;
using formneo.core.Services;
using formneo.core.UnitOfWorks;
using formneo.repository.Repositories;
using formneo.repository.UnitOfWorks;
using formneo.repository;
using formneo.service.Mapping;
using formneo.service.Services;
using Module = Autofac.Module;
using NLayer.Service.Services;
using NLayer.Core.Services;

namespace formneo.api.Modules
{
    public class RepoServiceModule : Module

    {

        protected override void Load(ContainerBuilder builder)
        {

            builder.RegisterGeneric(typeof(GenericRepository<>)).As(typeof(IGenericRepository<>)).InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(Service<>)).As(typeof(IService<>)).InstancePerLifetimeScope();

            builder.RegisterGeneric(typeof(ServiceWithDto<,>)).As(typeof(IServiceWithDto<,>)).InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(GlobalServiceWithDto<,>)).As(typeof(IGlobalServiceWithDto<,>)).InstancePerLifetimeScope();



            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();



            var apiAssembly = Assembly.GetExecutingAssembly();
            var repoAssembly = Assembly.GetAssembly(typeof(AppDbContext));
            var serviceAssembly = Assembly.GetAssembly(typeof(MapProfile));

            builder.RegisterAssemblyTypes(apiAssembly, repoAssembly, serviceAssembly).Where(x => x.Name.EndsWith("Repository")).AsImplementedInterfaces().InstancePerLifetimeScope();


            builder.RegisterAssemblyTypes(apiAssembly, repoAssembly, serviceAssembly).Where(x => x.Name.EndsWith("Service")).AsImplementedInterfaces().InstancePerLifetimeScope();


            // builder.RegisterType<ProductServiceWithCaching>().As<IProductService>();

        }
    }
}
