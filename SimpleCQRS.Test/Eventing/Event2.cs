using SimpleCQRS.Domain;

namespace SimpleCQRS.Test.Eventing
{
    public class Event2<TConstraint1> : Event
        where TConstraint1 : EventConstraintBase1
    {
    }
}
