using System;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Infrastructure
{
    public class MessageSubscription : ISubscription
    {
        public Action<IMessage> Action { get; private set; }
        public ISubscriptionManager Manager { get; private set; }
        public Type ActionType { get; private set; }

        public MessageSubscription(
            ISubscriptionManager manager,
            Action<IMessage> action,
            Type actionType)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (action == null) throw new ArgumentNullException(nameof(action));

            Manager = manager;
            Action = action;
            ActionType = actionType;
        }
    }
}
