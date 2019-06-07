using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using TehGM.EinherjiBot.DataModels;

namespace TehGM.EinherjiBot.Extensions
{
    public static class EmbedGenerator
    {
        public static EmbedBuilder ToEmbed(this EliteCG cg)
        {
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle(cg.Name)
                .WithDescription($"__**{cg.Objective}**__\n{cg.Description}")
                .AddField("System", cg.SystemName, true)
                .AddField("Station", cg.StationName, true)
                .AddField("Tier Reached", cg.TierReached.ToString())
                .AddField("Contributing Pilots", cg.ContributingPilotsCount.ToString(), true)
                .AddField("Contributions Count", cg.ContributionsCount.ToString(), true)
                .AddField("Last Updated", $"{(DateTime.UtcNow - cg.LastUpdateTime.ToUniversalTime()).ToLongFriendlyString()} ago")
                .WithUrl(cg.InaraURL)
                .WithColor(cg.IsCompleted ? Color.Green : (Color)System.Drawing.Color.Cyan)
                .WithFooter("Powered by Inara | CG expires in ")
                .WithTimestamp(cg.ExpirationTime);
            if (!string.IsNullOrWhiteSpace(cg.Reward))
                builder.AddField("Reward", cg.Reward);
            return builder;
        }
    }
}
