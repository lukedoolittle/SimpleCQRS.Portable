using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SimpleCQRS.Domain;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Infrastructure
{
    [JsonObject]
    public class EventDescriptors : IEnumerable<EventDescriptor>, IUnique
    {
        public Guid Id { get; set; }

        private readonly List<EventDescriptor> _descriptors;

        public EventDescriptor this[int key]
        {
            get { return _descriptors[key]; }
            set { _descriptors[key] = value; }
        }

        public EventDescriptors()
        {
            _descriptors = new List<EventDescriptor>();
        }

        public void Add(EventDescriptor eventDescriptor)
        {
            _descriptors.Add(eventDescriptor);
        }

        public IEnumerator<EventDescriptor> GetEnumerator()
        {
            return _descriptors.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _descriptors.GetEnumerator();
        }
    }

    public class EventDescriptor
    {
        public Event EventData { get; set; }
        public Guid Id { get; set; }
        public int Version { get; set; }

        public EventDescriptor(Guid id, Event eventData, int version)
        {
            Id = id;
            EventData = eventData;
            Version = version;
        }

        public EventDescriptor()
        {
        }
    }
}
