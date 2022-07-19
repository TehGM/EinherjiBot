using Discord.Commands;
using TehGM.EinherjiBot.CommandsProcessing.Services;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    /// <summary>A shared command descriptor for simple and regex commands.</summary>
    /// <remarks>Used for generating help text regardless of command type.</remarks>
    public class CommandDescriptor
    {
        public string DisplayName { get; }
        public string Summary { get; }
        public HelpCategoryAttribute HelpCategory { get; }
        public bool IsHidden { get; }
        public RestrictCommandAttribute Restrictions { get; }
        public int Priority { get; }

        public CommandDescriptor(RegexCommandInstance command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            this.DisplayName = GetAttribute<NameAttribute>(command)?.Text;
            this.Summary = GetAttribute<SummaryAttribute>(command)?.Text;
            this.HelpCategory = GetAttribute<HelpCategoryAttribute>(command);
            this.IsHidden = GetAttribute<HiddenAttribute>(command)?.Hide ?? false;
            this.Restrictions = GetAttribute<RestrictCommandAttribute>(command);
            this.Priority = command.Priority;
        }

        public CommandDescriptor(CommandInfo command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            this.DisplayName = command.Name;
            this.Summary = command.Summary;
            this.HelpCategory = GetAttribute<HelpCategoryAttribute>(command);
            this.IsHidden = GetAttribute<HiddenAttribute>(command)?.Hide ?? false;
            this.Restrictions = GetAttribute<RestrictCommandAttribute>(command);
            this.Priority = command.Priority;
        }

        private static T GetAttribute<T>(RegexCommandInstance command) where T : Attribute
            => GetAttribute<T>(command.Attributes);
        private static T GetAttribute<T>(CommandInfo command) where T : Attribute
            => GetAttribute<T>(command.Attributes);
        private static T GetAttribute<T>(IEnumerable<Attribute> attributes) where T : Attribute
            => attributes.LastOrDefault(attr => attr is T) as T;
    }
}
