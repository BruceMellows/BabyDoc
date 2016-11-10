// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace BabyDoc
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Globalization;
    using System.Linq;

    internal sealed class BabyDocPropertyDocumentationProvider
    {
        /// <summary>This method does [Create]</summary>
        /// <param name="syntaxNode">[syntaxNode] of type [Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax]</param>
        /// <returns>[IBabyDocDocumentationProvider]</returns>
        public static IBabyDocDocumentationProvider Create(PropertyDeclarationSyntax syntaxNode)
        {
            return new BabyDocDocumentationProvider(
                new ActualProvider(syntaxNode),
                BabyDocBasicSymbolNameDocumentationProvider.Create(),
                BabyDocParameterTextDocumentationProvider.Create());
        }

        private sealed class ActualProvider : BabyDocEmptyDocumentationProvider
        {
            private readonly PropertyDeclarationSyntax syntaxNode;

            /// <summary>Constructor for [ActualProvider]</summary>
            /// <param name="syntaxNode">[syntaxNode] of type [Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax]</param>
            public ActualProvider(PropertyDeclarationSyntax syntaxNode)
            {
                this.syntaxNode = syntaxNode;
            }

            /// <summary>This method does [SummaryText]</summary>
            /// <param name="symbol">[symbol] of type [Microsoft.CodeAnalysis.ISymbol]</param>
            /// <returns>[String]</returns>
            public override string SummaryText(ISymbol symbol)
            {
                var accessors = this.syntaxNode.AccessorList.Accessors
                    .Where(a => a.Modifiers.All(m => !m.IsKind(SyntaxKind.PrivateKeyword)))
                    .ToArray();
                var hasGet = accessors.Any(x => x.IsKind(SyntaxKind.GetAccessorDeclaration));
                var hasSet = accessors.Any(x => x.IsKind(SyntaxKind.SetAccessorDeclaration));
                return string.Format(CultureInfo.InvariantCulture, (!hasGet || !hasSet) ? !hasGet ? "Sets the [{0}]" : "Gets the [{0}]" : "Gets or sets the [{0}]", symbol.Name);
            }

            /// <summary>This method does [ReturnsText]</summary>
            /// <param name="returnTypeSymbol">[returnTypeSymbol] of type [Microsoft.CodeAnalysis.ITypeSymbol]</param>
            /// <returns>[String]</returns>
            public override string ReturnsText(ITypeSymbol returnTypeSymbol)
            {
                return returnTypeSymbol.Name != null && !returnTypeSymbol.Name.Equals("void", StringComparison.OrdinalIgnoreCase)
                        ? string.Format(CultureInfo.InvariantCulture, "The [{0}] of type [{1}]", this.syntaxNode.Identifier.Text, returnTypeSymbol.Name)
                        : string.Empty;
            }
        }
    }
}
