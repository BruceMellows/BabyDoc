// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace BabyDoc
{
    using Microsoft.CodeAnalysis;
    using System.Globalization;

    internal sealed class BabyDocMethodDocumentationProvider
    {
        public static IBabyDocDocumentationProvider Create()
        {
            return new BabyDocDocumentationProvider(new ActualProvider(), BabyDocStandardDocumentationProvider.Create());
        }

        private sealed class ActualProvider : BabyDocEmptyDocumentationProvider
        {
            public override string SummaryText(ISymbol symbol)
            {
                return string.Format(CultureInfo.InvariantCulture, "This method does [{0}]", symbol.Name);
            }
        }
    }
}
