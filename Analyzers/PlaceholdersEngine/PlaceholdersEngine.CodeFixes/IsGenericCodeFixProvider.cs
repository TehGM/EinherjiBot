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

namespace TehGM.Analyzers.PlaceholdersEngine
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IsGenericCodeFixProvider)), Shared]
    public class IsGenericCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(DiagnosticID.IsGeneric);

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
                    title: CodeFixResources.IsGeneric_MakeAbstractTitle,
                    createChangedDocument: c => this.MakeAbstract(context.Document, declaration, c),
                    equivalenceKey: nameof(CodeFixResources.IsGeneric_MakeAbstractTitle)),
                diagnostic);
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.IsGeneric_RemoveGenericTitle,
                    createChangedDocument: c => this.RemoveGeneric(context.Document, declaration, c),
                    equivalenceKey: nameof(CodeFixResources.IsGeneric_RemoveGenericTitle)),
                diagnostic);
        }

        private async Task<Document> RemoveGeneric(Document document, TypeDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            SyntaxNode node = declaration.RemoveNode(declaration.TypeParameterList, SyntaxRemoveOptions.KeepTrailingTrivia);
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            return document.WithSyntaxRoot(root.ReplaceNode(declaration, node));
        }

        private async Task<Document> MakeAbstract(Document document, TypeDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            // remove attribute
            IEnumerable<AttributeListSyntax> attributeLists = declaration.AttributeLists
                .Select(list =>
                {
                    AttributeSyntax attribute = list.Attributes.FirstOrDefault(attr
                        => attr.Name.ToString() == RequiredTypeName.PlaceholderAttribute || attr.Name.ToString() == RequiredTypeName.PlaceholderAttribute + "Attribute");
                    SeparatedSyntaxList<AttributeSyntax> attributes = attribute != null ? list.Attributes.Remove(attribute) : list.Attributes;
                    return SyntaxFactory.AttributeList(attributes);
                })
                .Where(list => list.Attributes.Any());

            // add abstract keyword
            SyntaxToken keyword = SyntaxFactory.Token(SyntaxKind.AbstractKeyword);
            SyntaxTokenList modifiers = declaration.Modifiers.Add(keyword);

            //combine
            SyntaxNode node = declaration.WithModifiers(modifiers)
                .WithAttributeLists(SyntaxFactory.List(attributeLists));
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            return document.WithSyntaxRoot(root.ReplaceNode(declaration, node));
        }
    }
}
