using System;
using System.Collections.Generic;
using System.Reflection;
using Discord.Commands;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    public class CommandsOptions
    {
        public string Prefix { get; set; } = ".";
        public bool AcceptMentionPrefix { get; set; } = true;
        public bool AcceptBotMessages { get; set; } = false;
        public bool RequirePublicMessagePrefix { get; set; } = true;
        public bool RequirePrivateMessagePrefix { get; set; } = false;

        public bool CaseSensitive { get; set; } = false;
        public RunMode DefaultRunMode { get; set; } = RunMode.Default;
        public bool IgnoreExtraArgs { get; set; } = true;

        public IDictionary<string, CommandRestrictionGroup> RestrictionGroups { get; set; } 
            = new Dictionary<string, CommandRestrictionGroup>(StringComparer.OrdinalIgnoreCase);

        // for loading
        public ICollection<Type> Classes { get; set; } = new List<Type>();
        public ICollection<Assembly> Assemblies { get; set; } = new List<Assembly>() { Assembly.GetEntryAssembly() };
    }
}
