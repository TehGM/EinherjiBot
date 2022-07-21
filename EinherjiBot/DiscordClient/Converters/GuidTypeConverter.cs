using Discord;
using Discord.Interactions;

namespace TehGM.EinherjiBot.DiscordClient.Converters
{
    public class GuidTypeConverter : TypeConverter<Guid>
    {
        public override ApplicationCommandOptionType GetDiscordType()
            => ApplicationCommandOptionType.String;

        public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
        {
            string input = option.Value.ToString();
            if (Guid.TryParse(input, out Guid guid))
                return Task.FromResult(TypeConverterResult.FromSuccess(guid));
            return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, $"{input} is not a valid GUID."));
        }
    }
}
