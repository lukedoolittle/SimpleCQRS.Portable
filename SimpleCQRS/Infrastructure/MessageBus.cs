using System;
using System.Collections.Generic;
using System.Linq;
using SimpleCQRS.Domain;
using SimpleCQRS.Exceptions;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Infrastructure
{
    public class MessageBus : IMessageBus
    {
        private readonly IDictionary<Type, IList<ISubscription>> _subscriptions =
            new Dictionary<Type, IList<ISubscription>>();

        public virtual void Send<TCommand>(TCommand command)
            where TCommand : Command
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            Type messageType = typeof(TCommand);
            if (_subscriptions.ContainsKey(messageType))
            {
                var subscriptionList = new List<ISubscription>(
                    _subscriptions[messageType].Cast<ISubscription>());
                if (subscriptionList.Count != 1)
                {
                    throw new MultipleCommandHandlersException();
                }
                subscriptionList[0].Action(command);
            }
        }

        public virtual void Publish<TEvent>(TEvent @event)
            where TEvent : Event
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            Type messageType = @event.GetType();
            if (_subscriptions.ContainsKey(messageType))
            {
                var subscriptionList = new List<ISubscription>(
                    _subscriptions[messageType]);
                foreach (var subscription in subscriptionList)
                    subscription.Action(@event);
            }
        }

        public virtual ISubscription Subscribe<TMessage>(Action<TMessage> action)
            where TMessage : IMessage
        {
            Type messageType = typeof(TMessage);
            var subscription = new MessageSubscription(
                this, 
                a=>action((TMessage)a),
                typeof(TMessage));

            if (_subscriptions.ContainsKey(messageType))
                _subscriptions[messageType].Add(subscription);
            else
                _subscriptions.Add(
                    messageType, 
                    new List<ISubscription> { subscription });

            return subscription;
        }

        public virtual void UnSubscribe(ISubscription subscription)
        {
            Type messageType = subscription.ActionType;
            if (_subscriptions.ContainsKey(messageType))
                _subscriptions[messageType].Remove(subscription);
        }
    }
}
