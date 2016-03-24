using System;
using System.Threading.Tasks;
using SimpleCQRS.Domain;
using SimpleCQRS.Exceptions;
using SimpleCQRS.Framework.Contracts;
using SimpleCQRS.Infrastructure;
using SimpleCQRS.Test.Mocks;
using Xunit;

namespace SimpleCQRS.Test
{
    public class MessageBusTests
    {
        [Fact]
        public async void SubscribeToGenericEventAllowsEventToBeHandled()
        {
            object actual = null;
            var subscriptionManager = new MessageBus(new HandlerFactoryMock((o) => { }));
            var handler = new EventHandlerMock<GenericDerived1>((o) => { actual = o; });
            var openGenericType = typeof(EventMock<>);
            var genericTypeParameters =  typeof(GenericDerived1);
            var @event = new EventMock<GenericDerived1>();

            subscriptionManager.Subscribe(handler, openGenericType, genericTypeParameters);

            await subscriptionManager.Publish(@event);

            Assert.Equal(@event, actual);
        }

        [Fact]
        public async void PassSubscriptionWithNonGenericParameter()
        {
            var handlerFactoryMock = new HandlerFactoryMock((o) => { });
            var messageBus = new MessageBus(handlerFactoryMock);

            var expected = new Event();
            Event actual = null;

            messageBus.Subscribe<Event>((e) => actual = e);

            await messageBus.Publish(expected);

            Assert.Equal(expected, actual);
        }

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

        [Fact]
        public async Task PassOpenSubscriptionValidNonGenericEventHandlerAndMessage()
        {
            object actual = null;
            var action = new Action<object>(o => actual = o);
            var handlerFactoryMock = new HandlerFactoryMock(action);
            var messageBus = new MessageBus(handlerFactoryMock);

            var messageType = typeof(NonGenericEvent);
            var handlerType = typeof(NonGenericEventHandlerMock);
            var expected = new NonGenericEvent();

            messageBus.OpenSubscribe(messageType, handlerType);

            await messageBus.Publish(expected);

            Assert.Equal(expected, actual);
        }
    }
}
