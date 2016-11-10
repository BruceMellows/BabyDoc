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
        public const string PropertyKeyComment = "comment";
        public const string PropertyKeyIsFirstChild = "isFirstChild";

        private static Regex singleLineDocumentationCommentTriviaRegex = new Regex(@"^(\s*///\s*(?<content>.+))+$", RegexOptions.CultureInvariant);
        public const string DiagnosticId = "BabyDoc";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private const string Category = "Documentation";

        private static DiagnosticDescriptor ConstructorRule = new DiagnosticDescriptor(
            DiagnosticId,
            new LocalizableResourceString(nameof(Resources.BabyDocConstructorAnalyzerTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.BabyDocConstructorAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Warning, isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.BabyDocConstructorAnalyzerDescription), Resources.ResourceManager, typeof(Resources)),
            customTags: new[] { nameof(ConstructorRule) });

        private static DiagnosticDescriptor MethodRule = new DiagnosticDescriptor(
            DiagnosticId,
            new LocalizableResourceString(nameof(Resources.BabyDocMethodAnalyzerTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.BabyDocMethodAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Warning, isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.BabyDocMethodAnalyzerDescription), Resources.ResourceManager, typeof(Resources)),
            customTags: new[] { nameof(MethodRule) });

        private static DiagnosticDescriptor PropertyRule = new DiagnosticDescriptor(
            DiagnosticId,
            new LocalizableResourceString(nameof(Resources.BabyDocPropertyAnalyzerTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.BabyDocPropertyAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)),
            Category,
            DiagnosticSeverity.Warning, isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.BabyDocPropertyAnalyzerDescription), Resources.ResourceManager, typeof(Resources)),
            customTags: new[] { nameof(PropertyRule) });

        /// <summary>Gets the [SupportedDiagnostics]</summary>
        /// <returns>[ImmutableArray]</returns>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(ConstructorRule, MethodRule, PropertyRule);
            }
        }

        /// <summary>This method does [Initialize]</summary>
        /// <param name="context">[context] of type [Microsoft.CodeAnalysis.Diagnostics.AnalysisContext]</param>
        /// <returns></returns>
        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSymbolAction(AnalyzeSymbolKindMethod, SymbolKind.Method);
            context.RegisterSymbolAction(AnalyzeSymbolKindProperty, SymbolKind.Property);
        }

        private static void AnalyzeSymbolKindMethod(SymbolAnalysisContext context)
        {
            var symbol = context.Symbol as IMethodSymbol;

            var constructorNode = symbol != null ? symbol.FindNodes<ConstructorDeclarationSyntax>().SingleOrDefault() : null;
            if (constructorNode != null
                && (symbol.ContainingType.TypeKind == TypeKind.Interface
                    || constructorNode.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword) || x.IsKind(SyntaxKind.InternalKeyword))))
            {
                AnalyzeWrappedSymbol(
                    context,
                    new BabyDocDiagnosticAdapter(constructorNode, () => symbol.Parameters, () => symbol.ReturnType),
                    BabyDocConstructorDocumentationProvider.Create(constructorNode),
                    ConstructorRule);

                return;
            }

            var methodNode = symbol != null ? symbol.FindNodes<MethodDeclarationSyntax>().SingleOrDefault() : null;
            if (methodNode != null)
            {
                if (methodNode != null
                    && (symbol.ContainingType.TypeKind == TypeKind.Interface
                        || methodNode.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword) || x.IsKind(SyntaxKind.InternalKeyword))))
                {
                    AnalyzeWrappedSymbol(
                        context,
                        new BabyDocDiagnosticAdapter(methodNode, () => symbol.Parameters, () => symbol.ReturnType),
                        BabyDocMethodDocumentationProvider.Create(methodNode),
                        MethodRule);
                }

                return;
            }
        }

        private static void AnalyzeSymbolKindProperty(SymbolAnalysisContext context)
        {
            var symbol = context.Symbol as IPropertySymbol;
            var syntaxNode = symbol != null ? symbol.FindNodes<PropertyDeclarationSyntax>().SingleOrDefault() : null;
            if (syntaxNode != null
                && (symbol.ContainingType.TypeKind == TypeKind.Interface
                    || syntaxNode.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword) || x.IsKind(SyntaxKind.InternalKeyword))))
            {
                AnalyzeWrappedSymbol(
                    context,
                    new BabyDocDiagnosticAdapter(syntaxNode, () => symbol.Parameters, () => symbol.Type),
                    BabyDocPropertyDocumentationProvider.Create(syntaxNode),
                    PropertyRule);
            }
        }

        private static void AnalyzeWrappedSymbol(
            SymbolAnalysisContext context,
            IBabyDocDiagnosticAdapter diagnosticAdapter,
            IBabyDocDocumentationProvider documentationProvider,
            DiagnosticDescriptor diagnosticDescriptor)
        {
            var syntaxNode = diagnosticAdapter.SyntaxNode;

            // extract existing documentation summary (if any)
            var existingDocumentation = syntaxNode
                .GetLeadingTrivia()
                .SingleOrDefault(x => x.Kind() == SyntaxKind.SingleLineDocumentationCommentTrivia);

            var matches = singleLineDocumentationCommentTriviaRegex.Matches(
                existingDocumentation != null
                    ? syntaxNode.SpanText(existingDocumentation.FullSpan)
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
                elementSummary = new XElement(ElementNameSummary, documentationProvider.SummaryText(context.Symbol));
            }

            // ... add missing documentation summary element - RETURNS
            var elementReturns = elementComment != null ? elementComment.Element(ElementNameReturns) : null;
            if (elementReturns == null)
            {
                var returnsText = documentationProvider.ReturnsText(diagnosticAdapter.ReturnType);
                elementReturns = returnsText != null ? new XElement(ElementNameReturns, returnsText) : null;
            }

            // ... add missing documentation summary element - REMARKS
            var elementRemarks = elementComment != null ? elementComment.Element("remarks") : null;

            // ... add missing documentation summary element - PARAM
            var elementParams = (elementComment != null ? elementComment.Elements("param") : Enumerable.Empty<XElement>())
                .Select(x => { var attr = x.Attribute("name"); return Tuple.Create(x, attr != null ? attr.Value : null); })
                .Where(x => x.Item2 != null)
                .ToArray();
            var existingParamNames = new HashSet<string>(elementParams.Select(x => x.Item2).Distinct());

            var parameters = diagnosticAdapter.Parameters.ToArray();
            var newParams = parameters.Select(x =>
            {
                return existingParamNames.Contains(x.Name) ? elementParams.First(e => e.Item2 == x.Name).Item1 : new XElement("param", new XAttribute("name", x.Name)) { Value = "[" + x.Name + "]" };
            }).ToArray();

            if (!elementParams.Select(x => x.Item2).OrderBy(x => x).SequenceEqual(parameters.Select(x => x.Name).OrderBy(x => x)))
            {
                elementParams = parameters
                    .Select(x =>
                    {
                        var parameterText = documentationProvider.ParameterText(x);
                        return parameterText != null
                            ? Tuple.Create(
                                existingParamNames.Contains(x.Name)
                                    ? elementParams.First(e => e.Item2 == x.Name).Item1
                                    : new XElement("param", new XAttribute("name", x.Name)) { Value = parameterText },
                                x.Name)
                            : null;
                    })
                    .Where(x => x != null)
                    .ToArray();
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
                        diagnosticDescriptor,
                        context.Symbol.Locations[0],
                        new[]
                        {
                            Tuple.Create(PropertyKeyComment, newCommentXmlText),
                            Tuple.Create(PropertyKeyIsFirstChild, diagnosticAdapter.IsFirstChild.ToString())
                        }.ToDictionary(x => x.Item1, x => x.Item2).ToImmutableDictionary(),
                        documentationProvider.SymbolName(context.Symbol)));
            }
        }

        private static XDocument TryXmlParse(string text)
        {
            try { return XDocument.Parse(text); }
            catch { return null; }
        }
    }
}
