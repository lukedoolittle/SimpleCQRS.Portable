using System;
using SimpleCQRS.Framework;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Test.Mocks
{
    public class HandlerFactoryMock : 
        MockBase<IHandlerFactory>, 
        IHandlerFactory
    {
        private readonly Action<object> _action;

        public HandlerFactoryMock(Action<object> action)
        {
            _action = action;
        }

        public object CreateHandler(Type handlerType)
        {
            return new InstanceCreator(handlerType)
                .AddConstructorParameter(_action)
                .Create<object>();
        }
    }
}
