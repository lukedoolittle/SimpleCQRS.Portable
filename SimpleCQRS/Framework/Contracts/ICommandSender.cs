using SimpleCQRS.Infrastructure;

namespace SimpleCQRS.Framework.Contracts
{
    public interface ICommandSender
    {
        void Send<T>(T command) 
            where T : Command;
    }
}