namespace TehGM.EinherjiBot.Security
{
    /// <summary>Represents user in relation to bot settings.</summary>
    public interface IUserContext
    {
        ulong ID { get; }
        string DisplayName { get; }
        string AvatarURL { get; }

        IEnumerable<string> Roles { get; }
    }
}
