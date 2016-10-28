// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace BabyDoc
{
    using Microsoft.CodeAnalysis;
    using System.Linq;

    internal class BabyDocDocumentationProvider : IBabyDocDocumentationProvider
    {
        private readonly IBabyDocDocumentationProvider[] providers;

        public BabyDocDocumentationProvider(params IBabyDocDocumentationProvider[] providers)
        {
            this.providers = providers.ToArray();
        }

        public string ParameterText(ISymbol parameterSymbol)
        {
            return this.providers.Select(x => x.ParameterText(parameterSymbol)).FirstOrDefault(x => x != null);
        }

        public string ReturnsText(ITypeSymbol returnTypeSymbol)
        {
            return this.providers.Select(x => x.ReturnsText(returnTypeSymbol)).FirstOrDefault(x => x != null);
        }

        public string SummaryText(ISymbol symbol)
        {
            return this.providers.Select(x => x.SummaryText(symbol)).FirstOrDefault(x => x != null);
        }
    }
}
