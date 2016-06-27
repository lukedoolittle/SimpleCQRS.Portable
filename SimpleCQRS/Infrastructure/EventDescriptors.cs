using System;
using System.Collections.Generic;
using System.Linq;
using SimpleCQRS.Domain;

namespace SimpleCQRS.Infrastructure
{
    public class EventDescriptors : Entity
    {
        private readonly List<EventDescriptor> descriptors;

        public EventDescriptor this[int key]
        {
            get { return descriptors[key]; }
            set { descriptors[key] = value; }
        }

        public EventDescriptors()
        {
            descriptors = new List<EventDescriptor>();
        }

        public void Add(EventDescriptor eventDescriptor)
        {
            descriptors.Add(eventDescriptor);
        }

        public IEnumerator<EventDescriptor> GetEnumerator()
        {
            return descriptors.GetEnumerator();
        }

        public IEnumerable<EventDescriptor> AsEnumerable()
        {
            return descriptors;
        }

        public int Count()
        {
            return descriptors.Count;
        }

        public IEnumerable<EventDescriptor> Where(
            Func<EventDescriptor, bool> predicate)
        {
            return descriptors.Where(predicate);
        }

        public IEnumerable<TResult> Select<TResult>(
            Func<EventDescriptor, TResult> selector)
        {
            return descriptors.Select(selector);
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
