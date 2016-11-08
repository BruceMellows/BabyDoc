// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace BabyDoc.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MethodTests : BabyDocCodeFixVerifier
    {
        [TestMethod]
        public void Private()
        {
            this.BabyDocTester(
                new[]
                {
                    "private static void $Main$(string[] args){}"
                },
                null);
        }

        [TestMethod]
        public void MissingDocumentation()
        {
            this.BabyDocTester(
                new[]
                {
                    "public static void $Main$(string[] args){}"
                },
                new[]
                {
                    "", //// FIXME = remove this line
                    "/// <summary>This method does [Main]</summary>",
                    "/// <param name=\"args\">[args] of type [string[]]</param>",
                    "/// <returns></returns>",
                    "public static void Main(string[] args){}"
                });
        }

        [TestMethod]
        public void MissingParameters()
        {
            this.BabyDocTester(
                new[]
                {
                    "/// <summary>This method is the entry point for the program</summary>",
                    "/// <returns></returns>",
                    "public static void $Main$(string[] args){}"
                },
                new[]
                {
                    "", //// FIXME = remove this line
                    "/// <summary>This method is the entry point for the program</summary>",
                    "/// <param name=\"args\">[args] of type [string[]]</param>",
                    "/// <returns></returns>",
                    "public static void Main(string[] args){}"
                });
        }

        [TestMethod]
        public void MissingDocumentationWithAttribute()
        {
            this.BabyDocTester(
                new[]
                {
                    "[Something]",
                    "public static void $Main$(string[] args){}"
                },
                new[]
                {
                    "", //// FIXME = remove this line
                    "/// <summary>This method does [Main]</summary>",
                    "/// <param name=\"args\">[args] of type [string[]]</param>",
                    "/// <returns></returns>",
                    "[Something]",
                    "public static void Main(string[] args){}"
                });
        }

        [TestMethod]
        public void ReturnType()
        {
            this.BabyDocTester(
                new[]
                {
                    "public static int $Main$(string[] args){}"
                },
                new[]
                {
                    "", //// FIXME = remove this line
                    "/// <summary>This method does [Main]</summary>",
                    "/// <param name=\"args\">[args] of type [string[]]</param>",
                    "/// <returns>[Int32]</returns>",
                    "public static int Main(string[] args){}"
                });
        }
    }
}