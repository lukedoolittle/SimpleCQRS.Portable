using System;
using SimpleCQRS.Framework.Contracts;

namespace SimpleCQRS.Domain
{
    public abstract class Entity : IUnique
    {
        public virtual Guid Id { get; set; }

        public virtual Type Type => GetType();
    }
}
