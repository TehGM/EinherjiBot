namespace TehGM.EinherjiBot.MessageTriggers
{
    public interface IMessageTriggersStore
    {
        Task<MessageTrigger> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<MessageTrigger>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<MessageTrigger>> GetForGuildAsync(ulong guildID, CancellationToken cancellationToken = default);
        Task<IEnumerable<MessageTrigger>> GetGlobalsAsync(CancellationToken cancellationToken = default);

        Task UpdateAsync(MessageTrigger trigger, CancellationToken cancellationToken = default);
    }
}
