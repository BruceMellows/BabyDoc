// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace BabyDoc
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class BabyDocBasicSymbolNameDocumentationProvider
    {
        /// <summary>This method does [Create]</summary>
        /// <param name="syntaxNode">[syntaxNode] of type [Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax]</param>
        /// <returns>[IBabyDocDocumentationProvider]</returns>
        public static IBabyDocDocumentationProvider Create()
        {
            return new BabyDocDocumentationProvider(new ActualProvider());
        }

        private sealed class ActualProvider : BabyDocEmptyDocumentationProvider
        {
            /// <summary>This method does [SymbolName]</summary>
            /// <param name="symbol">[symbol] of type [Microsoft.CodeAnalysis.ISymbol]</param>
            /// <returns>[String]</returns>
            public override string SymbolName(ISymbol symbol)
            {
                return symbol.Name;
            }
        }
    }
}
