using System;
using System.Collections.Generic;
using System.Linq;
using Foundations.Serialization;
using Newtonsoft.Json;
using SimpleCQRS.Domain;
using SimpleCQRS.Infrastructure;
using SimpleCQRS.Test.Eventing;
using SimpleCQRS.Test.Eventing.EventConstraintBaseTwo;
using SimpleCQRS.Test.Eventing.EventConstraintOne;
using Xunit;

namespace SimpleCQRS.Test
{
    public class SerializationTests
    {
        [Fact]
        public void JsonSerializeAndDeserialize()
        {
            var id = Guid.NewGuid();
            var expected = new EventDescriptors { Id = id };
            var events = new List<Event>
            {
                new Event1(),
                new Event2<EventConstraintBase1>(),
                new Event3<EventConstraintBase1, EventConstraintBase2>()
            };

            expected.Add(new EventDescriptor { Id = Guid.NewGuid(), EventData = events[0], Version = 0 });
            expected.Add(new EventDescriptor { Id = Guid.NewGuid(), EventData = events[1], Version = -5 });
            expected.Add(new EventDescriptor { Id = Guid.NewGuid(), EventData = events[2], Version = 1 });

            var serializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
                ContractResolver = new PrivateMembersContractResolver(),
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateParseHandling = DateParseHandling.None,
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
            };

            var intermediate = JsonConvert.SerializeObject(
                expected, 
                serializerSettings);
            var actual = (EventDescriptors)JsonConvert.DeserializeObject(
                intermediate, 
                typeof(EventDescriptors), 
                serializerSettings);

            Assert.Equal(id, actual.Id);
            Assert.Equal(expected.Count(), actual.Count());

            for (int i = 0; i < expected.Count(); i++)
            {
                Assert.Equal(expected[i].Id, actual[i].Id);
                Assert.Equal(expected[i].EventData.GetType(), actual[i].EventData.GetType());
                Assert.Equal(expected[i].Version, actual[i].Version);
            }
        }


    }
}
