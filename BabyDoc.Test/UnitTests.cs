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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using TestHelper;
    using BabyDoc;

    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void BabyDocEmptySource()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void BabyDocMissingDocumentation()
        {
            VerifyDiagnosticAndFix(@"
using System;

namespace ConsoleApplication1
{
    internal class TypeName
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(""Hello"");
        }
    }
}",
            new DiagnosticResult
            {
                Id = BabyDocDiagnosticAnalyzer.DiagnosticId,
                Message = String.Format("Externally visible method '{0}' does not have a documentation comment", "Main"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 8, 28)
                        }
            }, @"
using System;

namespace ConsoleApplication1
{
    internal class TypeName
    {
        /// <summary>This method does [Main]</summary>
        /// <param name=""args"">[args] of type [string[]]</param>
        /// <returns></returns>
        public static void Main(string[] args)
        {
            Console.WriteLine(""Hello"");
        }
    }
}");
        }

        [TestMethod]
        public void BabyDocMissingParameters()
        {
            VerifyDiagnosticAndFix(@"
using System;

namespace ConsoleApplication1
{
    internal class TypeName
    {
        /// <summary>This method is the entry point for the program</summary>
        /// <returns></returns>
        public static void Main(string[] args)
        {
            Console.WriteLine(""Hello"");
        }
    }
}",
            new DiagnosticResult
            {
                Id = BabyDocDiagnosticAnalyzer.DiagnosticId,
                Message = String.Format("Externally visible method '{0}' does not have a documentation comment", "Main"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 10, 28)
                        }
            }, @"
using System;

namespace ConsoleApplication1
{
    internal class TypeName
    {
        /// <summary>This method is the entry point for the program</summary>
        /// <param name=""args"">[args] of type [string[]]</param>
        /// <returns></returns>
        public static void Main(string[] args)
        {
            Console.WriteLine(""Hello"");
        }
    }
}");
        }

        [TestMethod]
        public void BabyDocMissingDocumentationWithAttribute()
        {
            VerifyDiagnosticAndFix(@"
using System;

namespace ConsoleApplication1
{
    internal class TypeName
    {
        [Something]
        public static void Main(string[] args)
        {
            Console.WriteLine(""Hello"");
        }
    }
}",
            new DiagnosticResult
            {
                Id = BabyDocDiagnosticAnalyzer.DiagnosticId,
                Message = String.Format("Externally visible method '{0}' does not have a documentation comment", "Main"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 28)
                        }
            }, @"
using System;

namespace ConsoleApplication1
{
    internal class TypeName
    {
        /// <summary>This method does [Main]</summary>
        /// <param name=""args"">[args] of type [string[]]</param>
        /// <returns></returns>
        [Something]
        public static void Main(string[] args)
        {
            Console.WriteLine(""Hello"");
        }
    }
}");
        }

        [TestMethod]
        public void BabyDocReturnType()
        {
            VerifyDiagnosticAndFix(@"
using System;

namespace ConsoleApplication1
{
    internal class TypeName
    {
        public static int Main(string[] args)
        {
            Console.WriteLine(""Hello"");
        }
    }
}",
            new DiagnosticResult
            {
                Id = BabyDocDiagnosticAnalyzer.DiagnosticId,
                Message = String.Format("Externally visible method '{0}' does not have a documentation comment", "Main"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 8, 27)
                        }
            }, @"
using System;

namespace ConsoleApplication1
{
    internal class TypeName
    {
        /// <summary>This method does [Main]</summary>
        /// <param name=""args"">[args] of type [string[]]</param>
        /// <returns>[Int32]</returns>
        public static int Main(string[] args)
        {
            Console.WriteLine(""Hello"");
        }
    }
}");
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new BabyDocCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new BabyDocDiagnosticAnalyzer();
        }

        private void VerifyDiagnosticAndFix(string sourceText, DiagnosticResult diagnostic, string fixedText)
        {
            VerifyCSharpDiagnostic(sourceText, diagnostic);
            VerifyCSharpFix(sourceText, fixedText);
        }
    }
}