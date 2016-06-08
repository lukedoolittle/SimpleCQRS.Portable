using System;

namespace SimpleCQRS.Framework.Contracts
{
    public interface ISubscriptionManager
    {
        ISubscription Subscribe<TMessage>(Action<TMessage> action)
            where TMessage : IMessage;

        void UnSubscribe(ISubscription subscription);
    }
}
