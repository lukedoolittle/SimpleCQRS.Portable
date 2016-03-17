using System;
using System.Threading.Tasks;
using SimpleCQRS.Exceptions;
using SimpleCQRS.Infrastructure;
using SimpleCQRS.Test.Mocks;
using Xunit;

namespace SimpleCQRS.Test
{
    public class MessageBusTests
    {
        [Fact]
        public void PassOpenSubscriptionInvalidMessageTypeExpectException()
        {
            var handlerFactoryMock = new HandlerFactoryMock(null);
            var messageBus = new MessageBus(handlerFactoryMock);

            var messageType = this.GetType();
            var handlerType = typeof(EventHandlerMock<>);

            Assert.Throws<TypeMismatchException>(() => 
                messageBus.OpenSubscribe(messageType, handlerType));
        }

        [Fact]
        public void PassOpenSubscriptionInvalidEventHandlerTypeExpectException()
        {
            var handlerFactoryMock = new HandlerFactoryMock(null);
            var messageBus = new MessageBus(handlerFactoryMock);

            var messageType = typeof(EventMock<>);
            var handlerType = this.GetType();

            Assert.Throws<TypeMismatchException>(() =>
                messageBus.OpenSubscribe(messageType, handlerType));
        }

        [Fact]
        public async Task PassOpenSubscriptionValidEventHandlerAndMessage()
        {
            object actual = null;
            var action = new Action<object>(o => actual = o);
            var handlerFactoryMock = new HandlerFactoryMock(action);
            var messageBus = new MessageBus(handlerFactoryMock);

            var messageType = typeof(EventMock<>);
            var handlerType = typeof(EventHandlerMock<>);
            var expected = new EventMock<GenericDerived1>();

            messageBus.OpenSubscribe(messageType, handlerType);

            await messageBus.Publish(expected);

            var actualActual = (EventMock<GenericDerived1>)actual;

            Assert.Equal(expected, actualActual);
        }
    }
}
