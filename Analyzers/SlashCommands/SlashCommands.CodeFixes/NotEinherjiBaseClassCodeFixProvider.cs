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

namespace TehGM.Analyzers.SlashCommands
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NotEinherjiBaseClassCodeFixProvider)), Shared]
    public class NotEinherjiBaseClassCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(DiagnosticID.NotEinherjiBaseClass);

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
                    title: CodeFixResources.NotEinherjiBaseClass_ChangeBaseClass,
                    createChangedDocument: c => this.ChangeBaseClass(context.Document, declaration, c),
                    equivalenceKey: nameof(CodeFixResources.NotEinherjiBaseClass_ChangeBaseClass)),
                diagnostic);
        }

        private async Task<Document> ChangeBaseClass(Document document, TypeDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            SyntaxNode oldNode = declaration.BaseList?.Types.FirstOrDefault()?.ChildNodes()
                .FirstOrDefault(t => (t as SimpleNameSyntax)?.Identifier.ToString() == DisallowedTypeName.InteractionModuleBase).Parent;
            SeparatedSyntaxList<BaseTypeSyntax> types = declaration.BaseList.Types;
            types = types.Remove(oldNode as BaseTypeSyntax);
            types = types.Insert(0, SyntaxFactory.SimpleBaseType(
                SyntaxFactory.ParseTypeName(RequiredTypeName.EinherjiInteractionModule)
                .WithTriviaFrom(oldNode)));

            SyntaxNode node = declaration.WithBaseList(SyntaxFactory.BaseList(declaration.BaseList.ColonToken, types));
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            return document.WithSyntaxRoot(root.ReplaceNode(declaration, node));
        }
    }
}
