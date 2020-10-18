using Discord;
using System;
using TehGM.EinherjiBot.DataModels;

namespace TehGM.EinherjiBot
{
    public static class EmbedGenerator
    {
        public static EmbedBuilder ToEmbed(this EliteCG cg, string thubnailUrl = null, string authorIconUrl = null)
        {
            string descriptionTrimmed = cg.Description.Length <= EmbedBuilder.MaxDescriptionLength ? cg.Description :
                $"{cg.Description.Remove(EmbedBuilder.MaxDescriptionLength - 3)}...";
            EmbedBuilder builder = new EmbedBuilder()
                .WithAuthor("Elite Dangerous Community Goals", authorIconUrl)
                .WithTitle(cg.Name)
                //.WithDescription($"**Objective**: `{cg.Objective}`\n\n{descriptionTrimmed}")
                .WithDescription($"```\n{cg.Objective}\n```\n{descriptionTrimmed}")
                .AddField("System", cg.SystemName, true)
                .AddField("Station", cg.StationName, true)
                .AddField("Tier Reached", $"*{cg.TierReached.ToString()}* / {cg.TierMax.ToString()}")
                .AddField("Contributing Pilots", cg.ContributingPilotsCount.ToString(), true)
                .AddField("Contributions Count", cg.ContributionsCount.ToString(), true)
                .AddField("Last Updated", $"{(DateTime.UtcNow - cg.LastUpdateTime.ToUniversalTime()).ToLongFriendlyString()} ago")
                .AddField("Is Completed?", cg.IsCompleted ? "\u2705" : "\u274C", true)
                .WithUrl(cg.InaraURL)
                .WithColor(cg.IsCompleted ? Color.Green : (Color)System.Drawing.Color.Cyan)
                .WithFooter("Powered by Inara | CG expiration time: ")
                .WithTimestamp(cg.ExpirationTime);
            if (thubnailUrl != null)
                builder.WithThumbnailUrl(thubnailUrl);
            if (!cg.IsCompleted)
                builder.AddField("Time Left", (cg.ExpirationTime - DateTimeOffset.Now).ToLongFriendlyString());

            if (!string.IsNullOrWhiteSpace(cg.Reward))
            {
                string rewardTrimmed = cg.Reward.Length <= EmbedFieldBuilder.MaxFieldValueLength ? cg.Reward :
                    $"{cg.Reward.Remove(EmbedFieldBuilder.MaxFieldValueLength - 3)}...";
                builder.AddField("Reward", rewardTrimmed);
            }
            return builder;
        }
    }
}
