using Microsoft.CodeAnalysis;

namespace TehGM.Analyzers.SlashCommands
{
    internal static class DiagnosticRule
    {
        public static readonly DiagnosticDescriptor NotEinherjiBaseClass = new DiagnosticDescriptor(DiagnosticID.NotEinherjiBaseClass,
            GetResourceString(nameof(Resources.NotEinherjiBaseClass_AnalyzerTitle)),
            GetResourceString(nameof(Resources.NotEinherjiBaseClass_AnalyzerMessageFormat)),
            "SlashCommands", DiagnosticSeverity.Warning, true,
            GetResourceString(nameof(Resources.NotEinherjiBaseClass_AnalyzerDescription)));


        private static LocalizableString GetResourceString(string name)
            => new LocalizableResourceString(name, Resources.ResourceManager, typeof(Resources));
    }
}
