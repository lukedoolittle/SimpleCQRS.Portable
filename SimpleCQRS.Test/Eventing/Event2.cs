using SimpleCQRS.Domain;
using SimpleCQRS.Test.Eventing.EventConstraintOne;

namespace SimpleCQRS.Test.Eventing
{
    public class Event2<TConstraint1> : Event
        where TConstraint1 : EventConstraintBase1
    {
    }
}
