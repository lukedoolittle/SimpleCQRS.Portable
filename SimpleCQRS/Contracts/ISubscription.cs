using System;

namespace SimpleCQRS.Framework.Contracts
{
    public interface ISubscription
    {
        Action<IMessage> Action { get; }
        ISubscriptionManager Manager { get; }
        Type ActionType { get; }
    }
}
