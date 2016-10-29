// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace BabyDoc
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Globalization;
    using System.Linq;

    internal sealed class BabyDocPropertyDocumentationProvider
    {
        public static IBabyDocDocumentationProvider Create(PropertyDeclarationSyntax syntaxNode)
        {
            return new BabyDocDocumentationProvider(new ActualProvider(syntaxNode), BabyDocStandardDocumentationProvider.Create());
        }

        private sealed class ActualProvider : BabyDocEmptyDocumentationProvider
        {
            private readonly PropertyDeclarationSyntax syntaxNode;

            public ActualProvider(PropertyDeclarationSyntax syntaxNode)
            {
                this.syntaxNode = syntaxNode;
            }

            public override string SummaryText(ISymbol symbol)
            {
                var accessors = this.syntaxNode.AccessorList.Accessors
                    .Where(a => a.Modifiers.All(m => !m.IsKind(SyntaxKind.PrivateKeyword)))
                    .ToArray();
                var hasGet = accessors.Any(x => x.IsKind(SyntaxKind.GetAccessorDeclaration));
                var hasSet = accessors.Any(x => x.IsKind(SyntaxKind.SetAccessorDeclaration));
                return string.Format(CultureInfo.InvariantCulture, (!hasGet || !hasSet) ? !hasGet ? "Sets the [{0}]" : "Gets the [{0}]" : "Gets or sets the [{0}]", symbol.Name);
            }
        }
    }
}
