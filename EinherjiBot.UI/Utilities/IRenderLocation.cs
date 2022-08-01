namespace TehGM.EinherjiBot.UI
{
    public interface IRenderLocation
    {
        RenderLocation RenderLocation { get; }

        bool IsClient => this.RenderLocation == RenderLocation.Client;
        bool IsServer => this.RenderLocation == RenderLocation.Server;
    }

    public enum RenderLocation
    {
        Client,
        Server
    }
}
