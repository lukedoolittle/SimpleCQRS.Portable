using System;

namespace SimpleCQRS.Framework.Contracts
{
    public interface ISubscriptionManager
    {
        ISubscription Subscribe<TMessage>(Action<TMessage> action)
            where TMessage : IMessage;

        ISubscription OpenSubscribe(
            Type openGenericMessageType,
            Type openGenericEventHandlerType);

        void UnSubscribe(ISubscription subscription);
    }
}
