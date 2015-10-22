using SimpleCQRS.Domain;

namespace SimpleCQRS.Test.Eventing
{
    public class Event3<TConstraint1, TConstraint2> : Event
        where TConstraint1 : EventConstraintBase1
        where TConstraint2 : EventConstraintBase2
    {
    }
}
