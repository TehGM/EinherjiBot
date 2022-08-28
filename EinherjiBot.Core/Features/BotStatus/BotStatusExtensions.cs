namespace TehGM.EinherjiBot.BotStatus
{
    public static class BotStatusExtensions
    {
        /// <summary>Checks if provided request would make any changes to given bot status.</summary>
        /// <param name="status">Existing bot status.</param>
        /// <param name="request">Data of new bot status state.</param>
        /// <returns>Whether <paramref name="request"/> would change <paramref name="status"/> in any way.</returns>
        public static bool HasChanges(this IBotStatus status, BotStatusRequest request)
        {
            return status.IsEnabled != request.IsEnabled
                || status.ActivityType != request.ActivityType
                || status.Text != request.Text
                || status.Link != request.Link;
        }
    }
}
