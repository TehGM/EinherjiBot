using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RestrictCommandAttribute : PreconditionAttribute
    {
        private readonly HashSet<string> _groupNames;
        private IReadOnlyCollection<string> _readonlyGroupNames;
        public IReadOnlyCollection<string> GroupNames
        {
            get
            {
                if (this._readonlyGroupNames == null)
                    this._readonlyGroupNames = this._groupNames.ToArray();
                return this._readonlyGroupNames;
            }
        }

        public RestrictCommandAttribute(IEnumerable<string> groupNames)
        {
            if (groupNames?.Any() != true)
                throw new ArgumentNullException(nameof(groupNames));
            this._groupNames = new HashSet<string>(groupNames, StringComparer.OrdinalIgnoreCase);
        }

        public RestrictCommandAttribute(params string[] groupNames)
            : this(groupNames as IEnumerable<string>) { }

        public RestrictCommandAttribute()
            : this(CommandRestrictionGroup.MainGuild) { }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            CommandsOptions options = services.GetRequiredService<IOptionsMonitor<CommandsOptions>>().CurrentValue;
            if (!this.CheckRestriction(context, options.RestrictionGroups))
                return Task.FromResult(PreconditionResult.FromError("Command restricted"));
            return Task.FromResult(PreconditionResult.FromSuccess());
        }

        public bool CheckRestriction(ICommandContext context, IDictionary<string, CommandRestrictionGroup> allRestrictionGroups)
        {
            if (allRestrictionGroups?.Any() != true)
                return false;

            IEnumerable<CommandRestrictionGroup> groups = this.GetRestrictionGroups(allRestrictionGroups);

            if (groups?.Any() != true)
                return false;
            if (context.Guild == null)
                return false;
            return groups.Any(group => group.GuildIDs.Contains(context.Guild.Id));
        }

        public bool CheckRestriction(CommandContext context, IDictionary<string, CommandRestrictionGroup> allRestrictionGroups)
        {
            if (allRestrictionGroups?.Any() != true)
                return false;

            IEnumerable<CommandRestrictionGroup> groups = this.GetRestrictionGroups(allRestrictionGroups);

            if (groups?.Any() != true)
                return false;
            if (context.Guild == null)
                return false;
            return groups.Any(group => group.GuildIDs.Contains(context.Guild.Id));
        }

        private IEnumerable<CommandRestrictionGroup> GetRestrictionGroups(IDictionary<string, CommandRestrictionGroup> allRestrictionGroups)
        {
            if (allRestrictionGroups?.Any() != true)
                return null;

            List<CommandRestrictionGroup> results = new List<CommandRestrictionGroup>(this._groupNames.Count);
            foreach (string groupName in this.GroupNames)
            {
                if (allRestrictionGroups.TryGetValue(groupName, out CommandRestrictionGroup group))
                    results.Add(group);
            }
            return results.AsEnumerable();
        }
    }
}
