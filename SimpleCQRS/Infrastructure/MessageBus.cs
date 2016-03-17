using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SimpleCQRS.Domain;
using SimpleCQRS.Exceptions;
using SimpleCQRS.Framework;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Infrastructure
{
    public class MessageBus : IMessageBus
    {
        private readonly IDictionary<Type, IList<ISubscription>> _subscriptions =
            new Dictionary<Type, IList<ISubscription>>();

        private readonly IHandlerFactory _factory;

        public MessageBus(IHandlerFactory factory)
        {
            if (factory == null)
            {
                throw new NullReferenceException(nameof(factory));
            }

            _factory = factory;
        }

        public virtual Task Send<TCommand>(TCommand command)
            where TCommand : Command
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var messageType = command.GetType();
            var results = TypePublish(command, messageType);

            var openGenericMessageType = command.GetType().GetGenericTypeDefinition();
            var genericResults = TypePublish(command, openGenericMessageType);

            return Task.WhenAll(results, genericResults);
        }

        public virtual Task Publish<TEvent>(TEvent @event)
            where TEvent : Event
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            var messageType = @event.GetType();
            var results = TypePublish(@event, messageType);
           
            var openGenericMessageType = @event.GetType().GetGenericTypeDefinition();
            var genericResults = TypePublish(@event, openGenericMessageType);

            return Task.WhenAll(results, genericResults);
        }

        private Task TypePublish<TMessage>(
            TMessage message, 
            Type subscriptionType, 
            bool limitSubscriptionCount = false)
            where TMessage : IMessage
        {
            if (_subscriptions.ContainsKey(subscriptionType))
            {
                var subscriptionList = new List<ISubscription>(
                    _subscriptions[subscriptionType]);
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

        public virtual ISubscription OpenSubscribe(
            Type openGenericMessageType, 
            Type openGenericEventHandlerType)
        {
            if (!(openGenericMessageType.HasBase(typeof(Command)) || 
                openGenericMessageType.HasBase(typeof(Event))))
            {
                throw new TypeMismatchException();
            }

            var implementedInterfaces =
                openGenericEventHandlerType
                    .GetTypeInfo()
                    .ImplementedInterfaces.Select(i => i.GetGenericTypeDefinition());

            if (!(implementedInterfaces.Contains(typeof(IEventHandler<>)) || 
                implementedInterfaces.Contains(typeof(ICommandHandler<>))))
            {
                throw new TypeMismatchException();
            }

            var subscription = new MessageSubscription(
                this,
                message => Reflection.CreateEventHandlerAndHandleEvent(
                    openGenericEventHandlerType, 
                    message,
                    _factory),
                openGenericMessageType);

            if (_subscriptions.ContainsKey(openGenericMessageType))
                _subscriptions[openGenericMessageType].Add(subscription);
            else
                _subscriptions.Add(
                    openGenericMessageType,
                    new List<ISubscription> { subscription });

            return subscription;
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
