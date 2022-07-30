using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Linq;

namespace TehGM.Analyzers.PlaceholdersEngine
{
    internal class PlaceholderDeclarationContext
    {
        public SyntaxNodeAnalysisContext NodeContext { get; }
        public TypeDeclarationSyntax Declaration { get; }
        public INamedTypeSymbol Symbol { get; }
        public bool HasRequiredAttribute { get; }
        public bool HasRequiredInterface { get; }
        public SyntaxToken AbstractToken { get; }

        public bool IsGeneric => this.Symbol.IsGenericType;
        public bool IsAbstract => this.AbstractToken != default;
        public bool IsClass => this.Declaration is ClassDeclarationSyntax;

        public PlaceholderDeclarationContext(SyntaxNodeAnalysisContext context, TypeDeclarationSyntax declaration, INamedTypeSymbol symbol,
            bool hasRequiredAttribute, bool hasRequiredInterface, SyntaxToken abstractToken)
        {
            this.NodeContext = context;
            this.Declaration = declaration;
            this.Symbol = symbol;
            this.HasRequiredAttribute = hasRequiredAttribute;
            this.HasRequiredInterface = hasRequiredInterface;
            this.AbstractToken = abstractToken;
        }

        public static bool TryGetFromContext(SyntaxNodeAnalysisContext context, out PlaceholderDeclarationContext result)
        {
            result = null;
            if (!(context.Node is TypeDeclarationSyntax declaration))
                return false;

            bool hasRequiredAttribute = declaration.AttributeLists.SelectMany(list => list.Attributes)
                .Any(attr => attr.Name.ToString() == RequiredTypeName.PlaceholderAttribute || attr.Name.ToString() == RequiredTypeName.PlaceholderAttribute + "Attribute");
            bool hasRequiredInterface = declaration.BaseList?.Types.Any(t => t.Type.ToString() == RequiredTypeName.PlaceholderInterface) == true;

            if (!hasRequiredAttribute && !hasRequiredInterface)
                return false;

            SyntaxToken abstractToken = declaration.Modifiers.FirstOrDefault(modifier => modifier.IsKind(SyntaxKind.AbstractKeyword));
            INamedTypeSymbol symbol = context.SemanticModel.GetDeclaredSymbol(declaration);

            result = new PlaceholderDeclarationContext(context, declaration, symbol, hasRequiredAttribute, hasRequiredInterface, abstractToken);
            return true;
        }

        public void ReportDiagnostic(DiagnosticDescriptor diagnostic, Location location)
            => this.NodeContext.ReportDiagnostic(Diagnostic.Create(diagnostic, location, this.Declaration.Identifier.ToString()));
        public void ReportDiagnostic(DiagnosticDescriptor diagnostic)
            => this.ReportDiagnostic(diagnostic, this.Symbol.Locations.First());
        public void ReportDiagnostic(DiagnosticDescriptor diagnostic, TextSpan locationSpan)
            => this.ReportDiagnostic(diagnostic, Location.Create(this.Declaration.SyntaxTree, locationSpan));
        public void ReportDiagnostic(DiagnosticDescriptor diagnostic, SyntaxNode locationNode)
            => this.ReportDiagnostic(diagnostic, locationNode.Span);
        public void ReportDiagnostic(DiagnosticDescriptor diagnostic, SyntaxToken locationToken)
            => this.ReportDiagnostic(diagnostic, locationToken.Span);
    }
}
