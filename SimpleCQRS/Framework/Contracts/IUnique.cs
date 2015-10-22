using System;

namespace SimpleCQRS.Framework.Contracts
{
    public interface IUnique
    {
        Guid Id { get; }
    }
}
