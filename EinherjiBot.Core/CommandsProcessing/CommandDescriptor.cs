using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Commands;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
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
        public IEnumerable<CommandCheckAttribute> CommandChecks { get; }

        public CommandDescriptor(RegexCommandInstance command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            this.DisplayName = GetAttribute<NameAttribute>(command)?.Text;
            this.Summary = GetAttribute<SummaryAttribute>(command)?.Text;
            this.HelpCategory = GetAttribute<HelpCategoryAttribute>(command);
            this.IsHidden = GetAttribute<HiddenAttribute>(command) != null;
            this.Restrictions = GetAttribute<RestrictCommandAttribute>(command);
            this.Priority = GetAttribute<DSharpPlus.CommandsNext.Attributes.PriorityAttribute>(command)?.Priority ?? 0;
            this.CommandChecks = GetAllAttributes<CommandCheckAttribute>(command);
        }

        public CommandDescriptor(CommandInfo command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            this.DisplayName = command.Name;
            this.Summary = command.Summary;
            this.HelpCategory = GetAttribute<HelpCategoryAttribute>(command);
            this.IsHidden = GetAttribute<HiddenAttribute>(command) != null;
            this.Restrictions = GetAttribute<RestrictCommandAttribute>(command);
            this.Priority = command.Priority;
            this.CommandChecks = GetAllAttributes<CommandCheckAttribute>(command);
        }

        public CommandDescriptor(Command command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            this.DisplayName = command.Name;
            this.Summary = GetAttribute<DescriptionAttribute>(command)?.Description;
            this.HelpCategory = GetAttribute<HelpCategoryAttribute>(command);
            this.IsHidden = GetAttribute<HiddenAttribute>(command) != null;
            this.Restrictions = GetAttribute<RestrictCommandAttribute>(command);
            this.Priority = GetAttribute<DSharpPlus.CommandsNext.Attributes.PriorityAttribute>(command)?.Priority ?? 0;
            this.CommandChecks = GetAllAttributes<CommandCheckAttribute>(command);
        }

        private static T GetAttribute<T>(RegexCommandInstance command) where T : Attribute
            => GetAttribute<T>(command.Attributes);
        private static T GetAttribute<T>(CommandInfo command) where T : Attribute
            => GetAttribute<T>(command.Attributes);
        private static T GetAttribute<T>(Command command) where T : Attribute
            => GetAttribute<T>(command.CustomAttributes);
        private static T GetAttribute<T>(IEnumerable<Attribute> attributes) where T : Attribute
            => attributes?.LastOrDefault(attr => attr is T) as T;

        private static IEnumerable<T> GetAllAttributes<T>(RegexCommandInstance command) where T : Attribute
            => GetAllAttributes<T>(command.Attributes);
        private static IEnumerable<T> GetAllAttributes<T>(CommandInfo command) where T : Attribute
            => GetAllAttributes<T>(command.Attributes);
        private static IEnumerable<T> GetAllAttributes<T>(Command command) where T : Attribute
            => GetAllAttributes<T>(command.CustomAttributes);
        private static IEnumerable<T> GetAllAttributes<T>(IEnumerable<Attribute> attributes) where T : Attribute
            => attributes?.Where(attr => attr is T).Cast<T>() ?? Enumerable.Empty<T>();
    }
}
