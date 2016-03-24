using System;

namespace SimpleCQRS.Framework.Contracts
{
    public interface ISubscriptionManager
    {
        ISubscription Subscribe<TMessage>(Action<TMessage> action)
            where TMessage : IMessage;

        ISubscription Subscribe(
            object handler,
            Type eventType,
            Type eventTypeGenericParameters = null);

        ISubscription OpenSubscribe(
            Type messageType,
            Type eventHandlerType);

        void UnSubscribe(ISubscription subscription);
    }
}
