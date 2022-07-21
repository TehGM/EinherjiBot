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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IsClassCodeFixProvider)), Shared]
    public class IsClassCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(DiagnosticID.IsClass);

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
                    title: CodeFixResources.IsClass_ChangeToClassTitle,
                    createChangedDocument: c => this.ChangeToClass(context.Document, declaration, c),
                    equivalenceKey: nameof(CodeFixResources.IsClass_ChangeToClassTitle)),
                diagnostic);
        }

        private async Task<Document> ChangeToClass(Document document, TypeDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            SyntaxNode node = SyntaxFactory.ClassDeclaration(
                declaration.AttributeLists, declaration.Modifiers,
                SyntaxFactory.Token(SyntaxKind.ClassKeyword).WithTriviaFrom(declaration.Keyword),
                declaration.Identifier,
                declaration.TypeParameterList,
                declaration.BaseList,
                declaration.ConstraintClauses,
                declaration.OpenBraceToken,
                declaration.Members,
                declaration.CloseBraceToken,
                declaration.SemicolonToken);
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            return document.WithSyntaxRoot(root.ReplaceNode(declaration, node));
        }
    }
}
