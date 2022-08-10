namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    /// <summary>Defines context in which placeholder can be used.</summary>
    /// <remarks>Some placeholders depend on scoped services, such as current user.
    /// This enum used with <see cref="PlaceholderAttribute"/> allows to prevent placeholder being listed where it's not possible to use it.</remarks>
    public enum PlaceholderUsage : uint
    {
        /// <summary>Placeholder cannot be used in any context.</summary>
        None = 0,
        /// <summary>Placeholder can be used in bot's status.</summary>
        Status = 1 << 1,
        /// <summary>Placeholder can be used by message triggers.</summary>
        MessageTrigger = 1 << 2,
        /// <summary>Placholder can be used by global message triggers.</summary>
        GlobalMessageTrigger = 1 << 3,

        /// <summary>Placeholder can be used in any context that doesn't depend on any scoped services.</summary>
        StaticContext = Status,
        /// <summary>Placeholder can be used in context that depends on received guild message.</summary>
        GuildMessageContext = StaticContext | MessageTrigger,
        /// <summary>Placeholder can be used in context that depend on received message of any type.</summary>
        AnyMessageContext = GuildMessageContext | GlobalMessageTrigger,
        /// <summary>Placeholder can be used in any context.</summary>
        Any = StaticContext | AnyMessageContext
    }
}
