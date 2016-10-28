// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace BabyDoc
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using System.Collections.Generic;

    internal interface IBabyDocDiagnosticAdapter
    {
        CSharpSyntaxNode SyntaxNode { get; }

        IEnumerable<ISymbol> Parameters { get; }

        ITypeSymbol ReturnType { get; }
    }
}
