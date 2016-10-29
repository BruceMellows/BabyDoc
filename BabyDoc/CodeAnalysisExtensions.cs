namespace BabyDoc
{
    using Microsoft.CodeAnalysis;
    using System.Collections.Generic;
    using System.Linq;

    internal static class CodeAnalysisExtensions
    {
        public static string SpanText(this SyntaxNode node, Microsoft.CodeAnalysis.Text.TextSpan span)
        {
            var root = node.Ancestors().LastOrDefault() ?? node;
            return root.GetText().ToString().Substring(span.Start, span.Length);
        }

        public static IEnumerable<T> FindNodes<T>(this ISymbol symbol) where T : SyntaxNode
        {
            return symbol.DeclaringSyntaxReferences
                .Select(syntaxReference => GetNodes(syntaxReference.SyntaxTree.GetRoot()).Where(node => node.Span == syntaxReference.Span))
                .SelectMany(x => x)
                .Select(x => x as T)
                .Where(x => x != null);
        }

        public static IEnumerable<SyntaxNode> GetNodes(this SyntaxNode node)
        {
            yield return node;

            foreach (var childNode in node.ChildNodes())
            {
                foreach (var item in GetNodes(childNode))
                {
                    yield return item;
                }
            }
        }
    }
}
