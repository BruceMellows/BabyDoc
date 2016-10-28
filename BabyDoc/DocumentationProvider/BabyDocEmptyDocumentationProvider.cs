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
        public virtual string ParameterText(ISymbol parameterSymbol)
        {
            return null;
        }

        public virtual string ReturnsText(ITypeSymbol returnTypeSymbol)
        {
            return null;
        }

        public virtual string SummaryText(ISymbol symbol)
        {
            return null;
        }
    }
}
