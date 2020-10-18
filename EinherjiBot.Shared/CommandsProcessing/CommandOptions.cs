using System;
using System.Collections.Generic;
using System.Reflection;
using Discord.Commands;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    public class CommandOptions
    {
        public string Prefix { get; set; } = ".";
        public bool AcceptMentionPrefix { get; set; } = true;
        public bool AcceptBotMessages { get; set; } = false;

        public bool CaseSensitive { get; set; } = false;
        public RunMode DefaultRunMode { get; set; } = RunMode.Default;
        public bool IgnoreExtraArgs { get; set; } = true;

        // for loading
        public ICollection<Type> Classes { get; set; } = new List<Type>();
        public ICollection<Assembly> Assemblies { get; set; } = new List<Assembly>() { Assembly.GetEntryAssembly() };
    }
}
