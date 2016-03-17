using System;
using SimpleCQRS.Domain;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Test.Mocks
{
    public class EventHandlerMock<TGeneric> : 
        IEventHandler<EventMock<TGeneric>>
        where TGeneric : GenericBase
    {
        private readonly Action<object> _action;
        public EventHandlerMock(Action<object> action)
        {
            _action = action;
        }
         
        public void Handle(EventMock<TGeneric> @event)
        {
            _action(@event);
        }
    }



    public class EventMock<TGeneric> : Event
        where TGeneric : GenericBase
    {}

    public class GenericDerived1 : GenericBase
    {}

    public class GenericDerived2 : GenericBase
    {}

    public class GenericBase
    {}
}
