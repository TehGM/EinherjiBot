namespace TehGM.EinherjiBot.DiscordClient
{
    /// <summary>During development and shortly after startup, pre-rendering might happen before the client is connected to Discord, which might break some features.
    /// This service is designed to help with that scenario.</summary>
    public interface IDiscordConnection
    {
        bool IsConnected { get; }
        Task WaitForConnectionAsync(CancellationToken cancellationToken = default);
    }
}
