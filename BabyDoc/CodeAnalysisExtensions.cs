// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace BabyDoc
{
    using Microsoft.CodeAnalysis;
    using System.Collections.Generic;
    using System.Linq;

    internal static class CodeAnalysisExtensions
    {
        /// <summary>This method does [SpanText]</summary>
        /// <param name="node">[node] of type [Microsoft.CodeAnalysis.SyntaxNode]</param>
        /// <param name="span">[span] of type [Microsoft.CodeAnalysis.Text.TextSpan]</param>
        /// <returns>[String]</returns>
        public static string SpanText(this SyntaxNode node, Microsoft.CodeAnalysis.Text.TextSpan span)
        {
            var root = node.Ancestors().LastOrDefault() ?? node;
            return root.GetText().ToString().Substring(span.Start, span.Length);
        }

        /// <summary>This method does [FindNodes]</summary>
        /// <param name="symbol">[symbol] of type [Microsoft.CodeAnalysis.ISymbol]</param>
        /// <returns>[IEnumerable]</returns>
        public static IEnumerable<T> FindNodes<T>(this ISymbol symbol) where T : SyntaxNode
        {
            return symbol.DeclaringSyntaxReferences
                .Select(syntaxReference => GetNodes(syntaxReference.SyntaxTree.GetRoot()).Where(node => node.Span == syntaxReference.Span))
                .SelectMany(x => x)
                .Select(x => x as T)
                .Where(x => x != null);
        }

        /// <summary>This method does [GetNodes]</summary>
        /// <param name="node">[node] of type [Microsoft.CodeAnalysis.SyntaxNode]</param>
        /// <returns>[IEnumerable]</returns>
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
