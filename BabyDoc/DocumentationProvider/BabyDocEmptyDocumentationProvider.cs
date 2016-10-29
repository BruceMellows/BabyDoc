// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace BabyDoc
{
    using Microsoft.CodeAnalysis;

    internal class BabyDocEmptyDocumentationProvider : IBabyDocDocumentationProvider
    {
        /// <summary>This method does [ParameterText]</summary>
        /// <param name="parameterSymbol">[parameterSymbol] of type [Microsoft.CodeAnalysis.ISymbol]</param>
        /// <returns>[String]</returns>
        public virtual string ParameterText(ISymbol parameterSymbol)
        {
            return null;
        }
        /// <summary>This method does [ReturnsText]</summary>
        /// <param name="returnTypeSymbol">[returnTypeSymbol] of type [Microsoft.CodeAnalysis.ITypeSymbol]</param>
        /// <returns>[String]</returns>
        public virtual string ReturnsText(ITypeSymbol returnTypeSymbol)
        {
            return null;
        }
        /// <summary>This method does [SummaryText]</summary>
        /// <param name="symbol">[symbol] of type [Microsoft.CodeAnalysis.ISymbol]</param>
        /// <returns>[String]</returns>
        public virtual string SummaryText(ISymbol symbol)
        {
            return null;
        }
    }
}
