namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("CurrentMessageContent", PlaceholderUsage.AnyMessageContext)]
    [Description("Is replaced with contents of sent message.")]
    public class CurrentMessageContentPlaceholder
    {
        public class CurrentMessageContentPlaceholderHandler : PlaceholderHandler<CurrentMessageContentPlaceholder>
        {
            private readonly PlaceholderConvertContext _context;

            public CurrentMessageContentPlaceholderHandler(PlaceholderConvertContext context)
            {
                this._context = context;
            }

            protected override Task<string> GetReplacementAsync(CurrentMessageContentPlaceholder placeholder, CancellationToken cancellationToken = default)
                => Task.FromResult(this._context.MessageContent);
        }
    }
}
