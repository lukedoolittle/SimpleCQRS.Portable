using System;

namespace SimpleCQRS.Framework.Contracts
{
    public interface IHandlerFactory
    {
        object CreateHandler(Type handlerType);
    }
}
