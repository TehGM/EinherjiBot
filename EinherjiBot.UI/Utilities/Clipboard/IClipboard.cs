namespace TehGM.EinherjiBot.UI
{
    public interface IClipboard
    {
        ValueTask WriteTextAsync(string text, CancellationToken cancellationToken = default);
    }
}
