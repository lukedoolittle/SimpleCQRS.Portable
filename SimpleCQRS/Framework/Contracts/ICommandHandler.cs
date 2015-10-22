using SimpleCQRS.Infrastructure;

namespace SimpleCQRS.Framework.Contracts
{
    public interface ICommandHandler<TCommand>
        where TCommand : Command
    {
        void Handle(TCommand command);
    }
}
