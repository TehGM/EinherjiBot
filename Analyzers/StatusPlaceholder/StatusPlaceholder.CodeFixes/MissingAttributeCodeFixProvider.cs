using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TehGM.Analyzers.StatusPlaceholder
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingAttributeCodeFixProvider)), Shared]
    public class MissingAttributeCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(DiagnosticID.MissingAttribute);

        public sealed override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            Diagnostic diagnostic = context.Diagnostics.First();
            TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;
            TypeDeclarationSyntax declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.MissingAttribute_AddAttributeTitle,
                    createChangedDocument: c => this.AddAttribute(context.Document, declaration, c),
                    equivalenceKey: nameof(CodeFixResources.MissingAttribute_AddAttributeTitle)),
                diagnostic);
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.MissingAttribute_AddAbstractKeywordTitle,
                    createChangedDocument: c => this.AddAbstractKeyword(context.Document, declaration, c),
                    equivalenceKey: nameof(CodeFixResources.MissingAttribute_AddAbstractKeywordTitle)),
                diagnostic);
        }

        private async Task<Document> AddAttribute(Document document, TypeDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            AttributeSyntax attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(RequiredTypeName.StatusPlaceholderAttribute))
                .WithArgumentList(SyntaxFactory.AttributeArgumentList());
            SyntaxNode node = declaration.WithAttributeLists(declaration.AttributeLists.Add(
                SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(attribute))));
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            return document.WithSyntaxRoot(root.ReplaceNode(declaration, node));
        }

        private async Task<Document> AddAbstractKeyword(Document document, TypeDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            SyntaxToken keyword = SyntaxFactory.Token(SyntaxKind.AbstractKeyword);
            SyntaxTokenList modifiers = declaration.Modifiers.Add(keyword);
            SyntaxNode node = declaration.WithModifiers(modifiers);
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            return document.WithSyntaxRoot(root.ReplaceNode(declaration, node));
        }
    }
}
