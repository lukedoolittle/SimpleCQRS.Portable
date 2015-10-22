using System;
using Newtonsoft.Json;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Domain
{
    public class Event : IMessage, IUnique
    {
        //TOD: refactor this so that these setters are private
        public Guid Id { get; set; }

        [JsonProperty("personId")]
        public Guid AggregateId { get; set; }

        public int Version { get; set; }
    }
}
