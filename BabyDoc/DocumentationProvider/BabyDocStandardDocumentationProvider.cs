// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace BabyDoc
{
    using Microsoft.CodeAnalysis;
    using System;

    internal sealed class BabyDocStandardDocumentationProvider
    {
        public static IBabyDocDocumentationProvider Create()
        {
            return new ActualProvider();
        }

        private sealed class ActualProvider : BabyDocEmptyDocumentationProvider
        {
            public override string ParameterText(ISymbol parameterSymbol)
            {
                return string.Format("[{0}] of type [{1}]", parameterSymbol.Name, parameterSymbol.ToString());
            }

            public override string ReturnsText(ITypeSymbol returnTypeSymbol)
            {
                return returnTypeSymbol.Name != null && !returnTypeSymbol.Name.Equals("void", StringComparison.OrdinalIgnoreCase)
                        ? "[" + returnTypeSymbol.Name + "]"
                        : string.Empty;
            }
        }
    }
}
