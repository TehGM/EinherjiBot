namespace TehGM.EinherjiBot.Security.API
{
    public interface IRefreshTokenGenerator
    {
        RefreshToken Generate(ulong userID, string discordRefreshToken);
    }
}
