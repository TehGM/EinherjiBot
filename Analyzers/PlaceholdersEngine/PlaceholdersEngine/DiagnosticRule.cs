using Microsoft.CodeAnalysis;

namespace TehGM.Analyzers.PlaceholdersEngine
{
    internal static class DiagnosticRule
    {
        public static readonly DiagnosticDescriptor IsAbstract = new DiagnosticDescriptor(DiagnosticID.IsAbstract,
            GetResourceString(nameof(Resources.IsAbstract_AnalyzerTitle)),
            GetResourceString(nameof(Resources.IsAbstract_AnalyzerMessageFormat)),
            "StatusPlaceholder", DiagnosticSeverity.Error, true,
            GetResourceString(nameof(Resources.IsAbstract_AnalyzerDescription)));

        public static readonly DiagnosticDescriptor IsClass = new DiagnosticDescriptor(DiagnosticID.IsClass,
            GetResourceString(nameof(Resources.IsClass_AnalyzerTitle)),
            GetResourceString(nameof(Resources.IsClass_AnalyzerMessageFormat)),
            "StatusPlaceholder", DiagnosticSeverity.Error, true,
            GetResourceString(nameof(Resources.IsClass_AnalyzerDescription)));

        public static readonly DiagnosticDescriptor IsGeneric = new DiagnosticDescriptor(DiagnosticID.IsGeneric,
            GetResourceString(nameof(Resources.IsGeneric_AnalyzerTitle)),
            GetResourceString(nameof(Resources.IsGeneric_AnalyzerMessageFormat)),
            "StatusPlaceholder", DiagnosticSeverity.Error, true,
            GetResourceString(nameof(Resources.IsGeneric_AnalyzerDescription)));


        private static LocalizableString GetResourceString(string name)
            => new LocalizableResourceString(name, Resources.ResourceManager, typeof(Resources));
    }
}
