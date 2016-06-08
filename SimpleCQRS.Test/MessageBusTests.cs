
using SimpleCQRS.Domain;
using SimpleCQRS.Infrastructure;
using Xunit;

namespace SimpleCQRS.Test
{
    public class MessageBusTests
    {
        [Fact]
        public async void PassSubscriptionWithNonGenericParameter()
        {
            var messageBus = new MessageBus();

            var expected = new Event();
            Event actual = null;

            messageBus.Subscribe<Event>((e) => actual = e);

            await messageBus.Publish(expected);

            Assert.Equal(expected, actual);
        }
    }
}
