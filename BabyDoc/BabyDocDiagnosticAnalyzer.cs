// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace BabyDoc
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System.Xml.Linq;
    using System.Text.RegularExpressions;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BabyDocDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private const string ElementNameSummary = "summary";
        private const string ElementNameReturns = "returns";
        public const string ElementNameComment = "comment";

        private static Regex singleLineDocumentationCommentTriviaRegex = new Regex(@"^(\s*///\s*(?<content>.+))+$", RegexOptions.CultureInvariant);
        public const string DiagnosticId = "BabyDoc";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Documentation";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSymbolAction(AnalyzeSymbolKindMethod, SymbolKind.Method);
        }

        private static void AnalyzeSymbolKindMethod(SymbolAnalysisContext context)
        {
            var methodSymbol = context.Symbol as IMethodSymbol;
            var methodDeclarationSyntaxNode = methodSymbol != null ? FindNode<MethodDeclarationSyntax>(methodSymbol) : null;
            if (methodDeclarationSyntaxNode != null
                && methodDeclarationSyntaxNode.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword) || x.IsKind(SyntaxKind.InternalKeyword)))
            {
                // extract existing documentation summary (if any)
                var existingDocumentation = methodDeclarationSyntaxNode
                    .GetLeadingTrivia()
                    .SingleOrDefault(x => x.Kind() == SyntaxKind.SingleLineDocumentationCommentTrivia);

                var matches = singleLineDocumentationCommentTriviaRegex.Matches(
                    existingDocumentation != null
                        ? SpanText(methodDeclarationSyntaxNode, existingDocumentation.FullSpan)
                        : string.Empty);

                var contentCaptures = matches.Count == 1
                    ? matches[0].Groups["content"].Captures.OfType<Capture>().ToList()
                    : new List<Capture>();

                var existingCommentXml = TryXmlParse(
                    "<" + ElementNameComment + ">"
                    + string.Join(string.Empty, contentCaptures.Select(x => x.ToString().Trim()))
                    + "</" + ElementNameComment + ">");

                // add any missing documentation summary elements
                var elementComment = existingCommentXml != null ? existingCommentXml.Element(ElementNameComment) : null;

                // ... add missing documentation summary element - SUMMARY
                var elementSummary = elementComment != null ? elementComment.Element(ElementNameSummary) : null;
                if (elementSummary == null)
                {
                    elementSummary = new XElement(ElementNameSummary, string.Format("This method does [{0}]", methodSymbol.Name));
                }

                // ... add missing documentation summary element - RETURNS
                var elementReturns = elementComment != null ? elementComment.Element(ElementNameReturns) : null;
                if (elementReturns == null)
                {
                    elementReturns = new XElement(
                        ElementNameReturns,
                        methodSymbol.ReturnType.Name != null && !methodSymbol.ReturnType.Name.Equals("void", StringComparison.OrdinalIgnoreCase)
                            ? "[" + methodSymbol.ReturnType.Name + "]"
                            : string.Empty);
                }

                // ... add missing documentation summary element - REMARKS
                var elementRemarks = elementComment != null ? elementComment.Element("remarks") : null;

                // ... add missing documentation summary element - PARAM
                var elementParams = (elementComment != null ? elementComment.Elements("param") : Enumerable.Empty<XElement>())
                    .Select(x => { var attr = x.Attribute("name"); return Tuple.Create(x, attr != null ? attr.Value : null); })
                    .Where(x => x.Item2 != null)
                    .ToArray();
                var existingParamNames = new HashSet<string>(elementParams.Select(x => x.Item2).Distinct());

                var parameters = methodSymbol.Parameters.ToArray();
                var newParams = parameters.Select(x =>
                {
                    return existingParamNames.Contains(x.Name) ? elementParams.First(e => e.Item2 == x.Name).Item1 : new XElement("param", new XAttribute("name", x.Name)) { Value = "[" + x.Name + "]" };
                }).ToArray();

                if (!elementParams.Select(x => x.Item2).OrderBy(x => x).SequenceEqual(parameters.Select(x => x.Name).OrderBy(x => x)))
                {
                    elementParams = parameters.Select(x =>
                    {
                        return Tuple.Create(
                            existingParamNames.Contains(x.Name)
                                ? elementParams.First(e => e.Item2 == x.Name).Item1
                                : new XElement("param", new XAttribute("name", x.Name)) { Value = string.Format("[{0}] of type [{1}]", x.Name, GetParameterTypeText(x)) },
                            x.Name);
                    }).ToArray();
                }

                // build expected documentation summary
                var newCommentElements = new[]
                {
                    new[]
                    {
                        elementSummary
                    },
                    elementParams.Select(x => x.Item1),
                    new[]
                    {
                        elementReturns, elementRemarks
                    }
                }
                .SelectMany(x => x)
                .Where(x => x != null)
                .ToArray();

                var newCommentXml = new XDocument(new XElement(ElementNameComment, newCommentElements));

                // compare existing and expected documentation summary
                var newCommentXmlText = newCommentXml.ToString();
                if (!existingCommentXml.ToString().Equals(newCommentXmlText, StringComparison.OrdinalIgnoreCase))
                {
                    // add diagnostic for unexpected documentation summary
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Rule,
                            methodSymbol.Locations[0],
                            new[] { Tuple.Create(ElementNameComment, newCommentXmlText) }.ToDictionary(x => x.Item1, x => x.Item2).ToImmutableDictionary(),
                            methodSymbol.Name));
                }
            }
        }

        private static string GetParameterTypeText(IParameterSymbol parameterSymbol)
        {
            var result = parameterSymbol.ToString();
            return result;
        }

        private static XDocument TryXmlParse(string text)
        {
            try { return XDocument.Parse(text); }
            catch { return null; }
        }

        private static string SpanText(SyntaxNode node, Microsoft.CodeAnalysis.Text.TextSpan span)
        {
            var root = node.Ancestors().LastOrDefault() ?? node;
            return root.GetText().ToString().Substring(span.Start, span.Length);
        }

        private static T FindNode<T>(ISymbol symbol) where T : SyntaxNode
        {
            return symbol.DeclaringSyntaxReferences
                .Select(syntaxReference => GetNodes(syntaxReference.SyntaxTree.GetRoot()).Where(node => node.Span == syntaxReference.Span))
                .SelectMany(x => x)
                .Select(x => x as T)
                .Single(x => x != null);
        }

        private static IEnumerable<SyntaxNode> GetNodes(SyntaxNode node)
        {
            yield return node;

            foreach (var childNode in node.ChildNodes())
            {
                foreach (var item in GetNodes(childNode))
                {
                    yield return item;
                }
            }
        }
    }
}