// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace BabyDoc.Test
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TestHelper;

    public class BabyDocCodeFixVerifier : CodeFixVerifier
    {
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new BabyDocCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new BabyDocDiagnosticAnalyzer();
        }

        protected void BabyDocTester(string messageFormat, IEnumerable<string> inputLines, IEnumerable<string> outputLines)
        {
            var doc = inputLines.ToArray();
            var line = doc.Length + 1;
            var col = doc.Last().IndexOf("$");
            var length = doc.Last().Substring(col + 1).IndexOf("$");
            var input = doc.Last().Remove(col, 1).Remove(col + length, 1);
            var target = doc.Last().Substring(col + 1, length);
            var sourceText = "using System;namespace TestNamespace{internal class TestClass{" + Environment.NewLine + string.Join(Environment.NewLine, doc.Take(doc.Length - 1).Concat(new[] { input })) + Environment.NewLine + "}}";

            if (outputLines != null)
            {
                var targetText = "using System;namespace TestNamespace{internal class TestClass{" + Environment.NewLine + string.Join(Environment.NewLine, outputLines) + Environment.NewLine + "}}";

                VerifyCSharpDiagnostic(
                  sourceText,
                  new DiagnosticResult
                  {
                      Id = BabyDocDiagnosticAnalyzer.DiagnosticId,
                      Message = String.Format(messageFormat, target),
                      Severity = DiagnosticSeverity.Warning,
                      Locations = new[]
                      {
                        new DiagnosticResultLocation("Test0.cs", line, col + 1)
                      }
                  });

                VerifyCSharpFix(sourceText, targetText);
            }
            else
            {
                VerifyCSharpDiagnostic(sourceText);
            }
        }
    }
}
