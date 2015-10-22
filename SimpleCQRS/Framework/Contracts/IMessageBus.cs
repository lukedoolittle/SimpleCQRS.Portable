namespace SimpleCQRS.Framework.Contracts
{
    public interface IMessageBus : 
        IEventPublisher, 
        ICommandSender, 
        ISubscriptionManager
    {
    }
}
