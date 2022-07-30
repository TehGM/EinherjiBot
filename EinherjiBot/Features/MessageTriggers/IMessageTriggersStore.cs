namespace TehGM.EinherjiBot.MessageTriggers
{
    public interface IMessageTriggersStore
    {
        Task<MessageTrigger> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<MessageTrigger>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<MessageTrigger>> GetForGuild(ulong guildID, CancellationToken cancellationToken = default);

        Task UpdateAsync(MessageTrigger trigger, CancellationToken cancellationToken = default);
    }
}
