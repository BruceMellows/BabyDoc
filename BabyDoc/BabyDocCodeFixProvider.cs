// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace BabyDoc
{
    using System;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CSharp;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BabyDocCodeFixProvider)), Shared]
    public class BabyDocCodeFixProvider : CodeFixProvider
    {
        private const string title = "Code Documentation";
        private const string commentLeader = "/// ";

        /// <summary>Gets the [FixableDiagnosticIds]</summary>
        /// <returns>[ImmutableArray]</returns>
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(BabyDocDiagnosticAnalyzer.DiagnosticId); }
        }

        /// <summary>This method does [GetFixAllProvider]</summary>
        /// <returns>[FixAllProvider]</returns>
        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <summary>This method does [RegisterCodeFixesAsync]</summary>
        /// <param name="context">[context] of type [Microsoft.CodeAnalysis.CodeFixes.CodeFixContext]</param>
        /// <returns>[Task]</returns>
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                var synraxNode = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent.AncestorsAndSelf().OfType<SyntaxNode>().First();

                context.RegisterCodeFix(
                    CodeAction.Create(title, c => UpdateDocumentationAsync(context.Document, synraxNode, diagnostic.Properties, c)),
                    diagnostic);
            }
        }

        private Task<Document> UpdateDocumentationAsync(Document document, SyntaxNode syntaxNode, ImmutableDictionary<string, string> diagnosticProperties, CancellationToken cancellationToken)
        {
            var getRootTask = document.GetSyntaxRootAsync(cancellationToken);
            var firstToken = syntaxNode.GetFirstToken();

            if (firstToken == null)
            {
                return null;
            }

            var indentText = firstToken.LeadingTrivia.Where(x => x.Kind() == SyntaxKind.WhitespaceTrivia).LastOrDefault();
            var documentationCommentLeaderText = (indentText != null ? indentText.ToFullString() : string.Empty) + commentLeader;
            var commentLinesFromProperty = diagnosticProperties.ContainsKey(BabyDocDiagnosticAnalyzer.ElementNameComment)
                ? diagnosticProperties[BabyDocDiagnosticAnalyzer.ElementNameComment].Replace("\r", string.Empty).Split('\n').Select(x => x.Trim()).ToArray()
                : null;
            if (commentLinesFromProperty == null)
            {
                return null;
            }

            var documentationCommentTriviaText =
                string.Join(
                    Environment.NewLine,
                    commentLinesFromProperty
                        .Skip(1)
                        .Take(commentLinesFromProperty.Length - 2)
                        .Select(x => documentationCommentLeaderText + x))
                + Environment.NewLine + indentText;

            var newMethodDeclaration = syntaxNode.ReplaceToken(
                firstToken,
                firstToken.WithLeadingTrivia(
                    SyntaxFactory.ParseLeadingTrivia(documentationCommentTriviaText)
                    .Insert(0, SyntaxFactory.LineFeed)));

            return getRootTask.ContinueWith(t => document.WithSyntaxRoot(t.Result.ReplaceNode(syntaxNode, newMethodDeclaration)));
        }
    }
}