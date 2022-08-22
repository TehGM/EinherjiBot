namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    public abstract class PlaceholderHandler<TPlaceholder> : IPlaceholderHandler
    {
        public Task<string> GetReplacementAsync(object placeholder, CancellationToken cancellationToken = default)
            => this.GetReplacementAsync((TPlaceholder)placeholder, cancellationToken);

        protected abstract Task<string> GetReplacementAsync(TPlaceholder placeholder, CancellationToken cancellationToken = default);
    }
}
