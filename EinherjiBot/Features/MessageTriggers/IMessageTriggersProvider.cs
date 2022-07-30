namespace TehGM.EinherjiBot.MessageTriggers
{
    public interface IMessageTriggersProvider
    {
        Task<MessageTrigger> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<MessageTrigger>> GetForGuild(ulong guildID, CancellationToken cancellationToken = default);

        Task UpdateAsync(MessageTrigger trigger, CancellationToken cancellationToken = default);
    }
}
