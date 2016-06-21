using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Features.ResolveAnything;
using BatmansBelt;
using BatmansBelt.Extensions;
using Microsoft.Practices.ServiceLocation;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Autofac
{
    public class CQRSAutofacBootstrapper : AutofacBootstrapperBase
    {
        public CQRSAutofacBootstrapper(params Assembly[] assemblies) : 
            base(assemblies)
        { }

        protected override IServiceLocator BuildContainer(
            ContainerBuilder builder, 
            IEnumerable<Assembly> assemblies)
        {
            builder.RegisterAssemblyGenericInterfaceImplementors(
                assemblies,
                typeof(IEventHandler<>));

            builder.RegisterAssemblyGenericInterfaceImplementors(
                assemblies,
                typeof(ICommandHandler<>));

            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());

            return base.BuildContainer(builder, assemblies);
        }
    }
}
