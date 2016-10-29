// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace BabyDoc
{
    using Microsoft.CodeAnalysis;
    using System;

    internal sealed class BabyDocReturnsTextDocumentationProvider
    {
        /// <summary>This method does [Create]</summary>
        /// <returns>[IBabyDocDocumentationProvider]</returns>
        public static IBabyDocDocumentationProvider Create()
        {
            return new ActualProvider();
        }

        private sealed class ActualProvider : BabyDocEmptyDocumentationProvider
        {
            /// <summary>This method does [ReturnsText]</summary>
            /// <param name="returnTypeSymbol">[returnTypeSymbol] of type [Microsoft.CodeAnalysis.ITypeSymbol]</param>
            /// <returns>[String]</returns>
            public override string ReturnsText(ITypeSymbol returnTypeSymbol)
            {
                return returnTypeSymbol.Name != null && !returnTypeSymbol.Name.Equals("void", StringComparison.OrdinalIgnoreCase)
                        ? "[" + returnTypeSymbol.Name + "]"
                        : string.Empty;
            }
        }
    }
}
