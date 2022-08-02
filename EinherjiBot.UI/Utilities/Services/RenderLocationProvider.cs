namespace TehGM.EinherjiBot.UI.Services
{
    public class RenderLocationProvider : IRenderLocation
    {
        public RenderLocation RenderLocation { get; }
        public string EnvironmentName { get; }

        public RenderLocationProvider(RenderLocation location, string environment)
        {
            this.RenderLocation = location;
            this.EnvironmentName = environment;
        }
    }
}
