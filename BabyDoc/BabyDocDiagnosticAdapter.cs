// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace BabyDoc
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class BabyDocDiagnosticAdapter : IBabyDocDiagnosticAdapter
    {
        private readonly Func<IEnumerable<ISymbol>> parametersCallback;

        private IEnumerable<ISymbol> parameters;

        private readonly Func<ITypeSymbol> returnTypeCallback;

        private ITypeSymbol returnType;

        public BabyDocDiagnosticAdapter(CSharpSyntaxNode syntaxNode, Func<IEnumerable<ISymbol>> parametersCallback, Func<ITypeSymbol> returnTypeCallback)
        {
            this.SyntaxNode = syntaxNode;
            this.parametersCallback = parametersCallback;
            this.returnTypeCallback = returnTypeCallback;
        }

        public CSharpSyntaxNode SyntaxNode { get; private set; }

        public IEnumerable<ISymbol> Parameters
        {
            get
            {
                return this.parameters ?? (this.parameters = this.parametersCallback().ToArray());
            }
        }

        public ITypeSymbol ReturnType
        {
            get
            {
                return this.returnType ?? (this.returnType = this.returnTypeCallback());
            }
        }
    }

}
