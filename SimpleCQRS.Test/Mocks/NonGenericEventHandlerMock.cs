using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleCQRS.Domain;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Test.Mocks
{
    public class NonGenericEventHandlerMock : IEventHandler<NonGenericEvent>
    {
        private readonly Action<object> _action;
        public NonGenericEventHandlerMock(Action<object> action)
        {
            _action = action;
        }

        public void Handle(NonGenericEvent @event)
        {
            _action(@event);
        }
    }

    public class NonGenericEvent : Event
    {
        
    }
}
