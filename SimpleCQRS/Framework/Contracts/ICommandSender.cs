using System.Threading.Tasks;
using SimpleCQRS.Infrastructure;

namespace SimpleCQRS.Framework.Contracts
{
    public interface ICommandSender
    {
        Task Send<T>(T command) 
            where T : Command;
    }
}