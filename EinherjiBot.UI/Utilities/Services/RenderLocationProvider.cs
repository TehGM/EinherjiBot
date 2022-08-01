namespace TehGM.EinherjiBot.UI.Services
{
    public class RenderLocationProvider : IRenderLocation
    {
        public RenderLocation RenderLocation { get; }

        public RenderLocationProvider(RenderLocation location)
        {
            this.RenderLocation = location;
        }
    }
}
