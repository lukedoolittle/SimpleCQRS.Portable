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

            var results = TypePublish(
                message, 
                messageType, 
                limitSubscriptionCount);

            if (messageType.IsConstructedGenericType)
            {
                var openGenericMessageType = messageType.GetGenericTypeDefinition();
                var genericResults = TypePublish(
                    message,
                    openGenericMessageType,
                    limitSubscriptionCount);

                return Task.WhenAll(results, genericResults);
            }

            return Task.WhenAll(results);
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
            Type messageType, 
            Type eventHandlerType)
        {
            if (!(messageType.HasBase(typeof(Command)) || 
                messageType.HasBase(typeof(Event))))
            {
                throw new TypeMismatchException();
            }

            var implementedInterfaces =
                eventHandlerType
                    .GetTypeInfo()
                    .ImplementedInterfaces.Select(i => i.GetGenericTypeDefinition());

            if (!(implementedInterfaces.Contains(typeof(IEventHandler<>)) || 
                implementedInterfaces.Contains(typeof(ICommandHandler<>))))
            {
                throw new TypeMismatchException();
            }

            if (eventHandlerType.GetTypeInfo().IsGenericType)
            {
                var subscription = new MessageSubscription(
                    this,
                    message => Reflection.CreateEventHandlerAndHandleEvent(
                        eventHandlerType,
                        message,
                        _factory),
                    messageType);

                if (_subscriptions.ContainsKey(messageType))
                    _subscriptions[messageType].Add(subscription);
                else
                    _subscriptions.Add(
                        messageType,
                        new List<ISubscription> {subscription});

                return subscription;
            }
            else
            {
                var handler = _factory.CreateHandler(eventHandlerType);
                return Subscribe(handler, messageType);
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

        public ISubscription Subscribe(
            object handler,
            Type eventType,
            Type eventTypeGenericParameters = null)
        {
            string subscriptionMethod = "Subscribe";
            string handlerMethod = "Handle";

            var type = eventType;
            if (eventTypeGenericParameters != null)
            {
                type = eventType.MakeGenericType(eventTypeGenericParameters);
            }

            return (ISubscription)new MethodInvoker(this, subscriptionMethod)
                .AddGenericParameter(type)
                .AddMethodParameter(new Action<object>(o =>
                    new MethodInvoker(handler, handlerMethod)
                        .AddMethodParameter(o)
                        .Execute()))
                .Execute();
        }

        public virtual void UnSubscribe(ISubscription subscription)
        {
            Type messageType = subscription.ActionType;
            if (_subscriptions.ContainsKey(messageType))
                _subscriptions[messageType].Remove(subscription);
        }
    }
}
