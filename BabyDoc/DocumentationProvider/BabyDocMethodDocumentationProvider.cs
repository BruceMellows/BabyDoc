﻿// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace BabyDoc
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Globalization;

    internal sealed class BabyDocMethodDocumentationProvider
    {
        /// <summary>This method does [Create]</summary>
        /// <param name="syntaxNode">[syntaxNode] of type [Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax]</param>
        /// <returns>[IBabyDocDocumentationProvider]</returns>
        public static IBabyDocDocumentationProvider Create(MethodDeclarationSyntax syntaxNode)
        {
            return new BabyDocDocumentationProvider(
                new ActualProvider(syntaxNode),
                BabyDocBasicSymbolNameDocumentationProvider.Create(),
                BabyDocReturnsTextDocumentationProvider.Create(),
                BabyDocParameterTextDocumentationProvider.Create());
        }

        private sealed class ActualProvider : BabyDocEmptyDocumentationProvider
        {
            private readonly MethodDeclarationSyntax syntaxNode;

            /// <summary>Constructor for [ActualProvider]</summary>
            /// <param name="syntaxNode">[syntaxNode] of type [Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax]</param>
            public ActualProvider(MethodDeclarationSyntax syntaxNode)
            {
                this.syntaxNode = syntaxNode;
            }

            /// <summary>This method does [SummaryText]</summary>
            /// <param name="symbol">[symbol] of type [Microsoft.CodeAnalysis.ISymbol]</param>
            /// <returns>[String]</returns>
            public override string SummaryText(ISymbol symbol)
            {
                return string.Format(CultureInfo.InvariantCulture, "This method does [{0}]", symbol.Name);
            }
        }
    }
}
