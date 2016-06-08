using System;
using Microsoft.Practices.ServiceLocation;

namespace SimpleCQRS.Autofac.Test
{
    public class MessageBusFixture : IDisposable
    {
        public IServiceLocator Resolver { get; set; }

        public MessageBusFixture()
        {
            Resolver = new CQRSAutofacBootstrapper().Run();
        }

        public void Dispose()
        {
        }
    }
}
