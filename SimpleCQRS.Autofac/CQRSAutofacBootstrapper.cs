using BatmansBelt;
using BatmansBelt.Extensions;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Autofac
{
    public class CQRSAutofacBootstrapper : AutofacBootstrapperBase
    {
        protected override void UseDefaults()
        {
            var assemblies = CurrentAppDomain.GetAssemblies();

            _builder.RegisterAssemblyGenericInterfaceImplementors(
                assemblies,
                typeof(IEventHandler<>));

            _builder.RegisterAssemblyGenericInterfaceImplementors(
                assemblies,
                typeof(ICommandHandler<>));

            base.UseDefaults();
        }
    }
}
