namespace TehGM.EinherjiBot.UI
{
    public interface IRenderLocation
    {
        RenderLocation RenderLocation { get; }
        string EnvironmentName { get; }

        bool IsClient => this.RenderLocation == RenderLocation.Client;
        bool IsServer => this.RenderLocation == RenderLocation.Server;

        bool IsDevelopment => this.EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase);
        bool IsProduction => this.EnvironmentName.Equals("Production", StringComparison.OrdinalIgnoreCase);
    }

    public enum RenderLocation
    {
        Client,
        Server
    }
}
