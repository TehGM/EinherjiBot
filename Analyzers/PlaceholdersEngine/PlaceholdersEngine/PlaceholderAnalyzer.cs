using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace TehGM.Analyzers.PlaceholdersEngine
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PlaceholderAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(DiagnosticRule.MissingInterface, DiagnosticRule.MissingAttribute, DiagnosticRule.IsAbstract, DiagnosticRule.IsClass, DiagnosticRule.IsGeneric);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration, 
                SyntaxKind.StructDeclaration, SyntaxKind.InterfaceDeclaration, SyntaxKind.EnumDeclaration);
        }

        private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext nodeContext)
        {
            if (!PlaceholderDeclarationContext.TryGetFromContext(nodeContext, out PlaceholderDeclarationContext context))
                return;

            AnalyzeMissingInterface(context);
            AnalyzeMissingAttribute(context);
            AnalyzeIsAbstract(context);
            AnalyzeIsClass(context);
            AnalyzeIsGeneric(context);
        }

        private static void AnalyzeMissingInterface(PlaceholderDeclarationContext context)
        {
            if (context.HasRequiredInterface)
                return;
            context.ReportDiagnostic(DiagnosticRule.MissingInterface);
        }

        private static void AnalyzeMissingAttribute(PlaceholderDeclarationContext context)
        {
            if (context.HasRequiredAttribute || context.IsAbstract)
                return;
            context.ReportDiagnostic(DiagnosticRule.MissingAttribute);
        }

        private static void AnalyzeIsAbstract(PlaceholderDeclarationContext context)
        {
            if (!context.IsAbstract || !context.HasRequiredAttribute)
                return;
            context.ReportDiagnostic(DiagnosticRule.IsAbstract, context.AbstractToken);
        }

        private static void AnalyzeIsClass(PlaceholderDeclarationContext context)
        {
            if (context.IsClass)
                return;
            context.ReportDiagnostic(DiagnosticRule.IsClass, context.Declaration.Keyword);
        }

        private static void AnalyzeIsGeneric(PlaceholderDeclarationContext context)
        {
            if (!context.IsGeneric || context.IsAbstract)
                return;
            TextSpan location = new TextSpan(context.Declaration.Identifier.SpanStart, 
                context.Declaration.Identifier.FullSpan.Length + context.Declaration.TypeParameterList.Span.Length);
            context.ReportDiagnostic(DiagnosticRule.IsGeneric, location);
        }
    }
}
