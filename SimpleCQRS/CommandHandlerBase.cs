using System;
using SimpleCQRS.Domain;
using SimpleCQRS.Framework.Contracts;
using SimpleCQRS.Infrastructure;

namespace SimpleCQRS
{
    public abstract class CommandHandlerBase<TAggregate, TCommand> : 
        ICommandHandler<TCommand> 
        where TAggregate : AggregateRoot, new()
        where TCommand : Command
    {
        protected readonly IRepository<TAggregate> _repository;

        protected CommandHandlerBase(IRepository<TAggregate> repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            _repository = repository;
        }

        public abstract void Handle(TCommand command);
    }
}
