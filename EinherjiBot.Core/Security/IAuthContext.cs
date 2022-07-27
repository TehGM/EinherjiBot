namespace TehGM.EinherjiBot.Security
{
    public interface IAuthContext
    {
        ulong ID { get; }
        string Username { get; }
        string Discriminator { get; }
        IEnumerable<string> BotRoles { get; }

        string GetAvatarURL(ushort size = 1024);
    }
}
