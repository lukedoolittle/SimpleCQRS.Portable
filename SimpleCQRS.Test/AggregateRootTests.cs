using SimpleCQRS.Exceptions;
using SimpleCQRS.Test.Eventing;
using SimpleCQRS.Test.Eventing.EventConstraintBaseTwo;
using SimpleCQRS.Test.Eventing.EventConstraintOne;
using SimpleCQRS.Test.Mocks;
using Xunit;

namespace SimpleCQRS.Test
{
    public class AggregateRootTests
    {
        [Fact]
        public void RegisterOpenGenericEvents()
        {
            var mockRoot = new AggregateRootRegistrationMock();

            mockRoot.RegisterOpenGenericEvents(typeof(Event2<>));
            var expected = new Event2<EventConstraint1>();

            mockRoot.HandleEvent(expected);

            var actual = mockRoot.GetEvent<Event2<EventConstraint1>>();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RegisterGenericEventsWithAggregateRoot()
        {
            var mockRoot = new AggregateRootRegistrationMock();

            mockRoot.CallRegisterEvents();
        }
    }
}
