namespace TehGM.EinherjiBot.DiscordClient
{
    public class DiscordOptions
    {
        /// <summary>Bot token from Discord API portal.</summary>
        public string BotToken { get; set; }
        /// <summary>Compiles commands, which improves their execution speed, but increases memory use.</summary>
        public bool CompileCommands { get; set; }
        /// <summary>Forces all commands to be registered in a specific guild. Meant for in-development testing.</summary>
        public ulong? OverrideCommandsGuildID { get; set; }
    }
}