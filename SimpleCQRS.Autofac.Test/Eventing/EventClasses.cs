using System;
using SimpleCQRS.Domain;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Autofac.Test.Eventing
{
    public class NonGenericEventMock : Event { }

    public class EventHandlerMock1 : IEventHandler<NonGenericEventMock>
    {
        public static Action<object> Action { get; set; }

        public void Handle(NonGenericEventMock @event)
        {
            Action(@event);
        }
    }

    public class EventHandlerMock<TGeneric> : IEventHandler<EventMock<TGeneric>>
        where TGeneric : GenericBase
    {
        public static Action<object> Action { get; set; }

        public void Handle(EventMock<TGeneric> @event)
        {
            Action(@event);
        }
    }

    public class EventMock<TGeneric> : Event where TGeneric : GenericBase{ }

    public class GenericDerived1 : GenericBase{ }

    public class GenericDerived2 : GenericBase{ }

    public class GenericBase{ }
}
