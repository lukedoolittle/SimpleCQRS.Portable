using System;
using SimpleCQRS.Framework.Contracts;
using SimpleCQRS.Infrastructure;

namespace SimpleCQRS.Autofac.Test.Eventing
{
    public class NonGenericCommandMock : Command {
        public NonGenericCommandMock() : base(Guid.Empty, 0)
        {
        }
    }

    public class CommandHandlerMock1 : ICommandHandler<NonGenericCommandMock>
    {
        public static Action<object> Action { get; set; }

        public void Handle(NonGenericCommandMock command)
        {
            Action(command);
        }
    }

    public class CommandHandlerMock<TGeneric> : ICommandHandler<CommandMock<TGeneric>>
        where TGeneric : GenericBase
    {
        public static Action<object> Action { get; set; }

        public void Handle(CommandMock<TGeneric> @event)
        {
            Action(@event);
        }
    }

    public class CommandMock<TGeneric> : Command where TGeneric : GenericBase {
        public CommandMock() : base(Guid.Empty, 0)
        {
        }
    }
}
