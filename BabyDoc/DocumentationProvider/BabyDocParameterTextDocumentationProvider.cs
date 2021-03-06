﻿// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace BabyDoc
{
    using Microsoft.CodeAnalysis;

    internal sealed class BabyDocParameterTextDocumentationProvider
    {
        /// <summary>This method does [Create]</summary>
        /// <returns>[IBabyDocDocumentationProvider]</returns>
        public static IBabyDocDocumentationProvider Create()
        {
            return new ActualProvider();
        }

        private sealed class ActualProvider : BabyDocEmptyDocumentationProvider
        {
            /// <summary>This method does [ParameterText]</summary>
            /// <param name="parameterSymbol">[parameterSymbol] of type [Microsoft.CodeAnalysis.ISymbol]</param>
            /// <returns>[String]</returns>
            public override string ParameterText(ISymbol parameterSymbol)
            {
                return string.Format("[{0}] of type [{1}]", parameterSymbol.Name, parameterSymbol.ToString());
            }
        }
    }
}
