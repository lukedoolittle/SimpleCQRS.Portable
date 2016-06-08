using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleCQRS.Domain;
using SimpleCQRS.Exceptions;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Infrastructure
{
    public class MessageBus : IMessageBus
    {
        private readonly IDictionary<Type, IList<ISubscription>> _subscriptions =
            new Dictionary<Type, IList<ISubscription>>();

        public virtual Task Send<TCommand>(TCommand command)
            where TCommand : Command
        {
            return Message(command, true);
        }

        public virtual Task Publish<TEvent>(TEvent @event)
            where TEvent : Event
        {
            return Message(@event, false);
        }

        private Task Message<TMessage>(
            TMessage message,
            bool limitSubscriptionCount)
            where TMessage : IMessage
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var messageType = message.GetType();

            if (_subscriptions.ContainsKey(messageType))
            {
                var subscriptionList = new List<ISubscription>(
                    _subscriptions[messageType]);

                if (limitSubscriptionCount && subscriptionList.Count != 1)
                {
                    throw new MultipleCommandHandlersException();
                }

                var tasks = subscriptionList
                    .Select(subscription => Task.Run(() => subscription.Action(message)))
                    .ToList();
                return Task.WhenAll(tasks);
            }

            return Task.FromResult(0);
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
