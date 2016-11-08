// NO LICENSE
// ==========
// There is no copyright, you can use and abuse this source without limit.
// There is no warranty, you are responsible for the consequences of your use of this source.
// There is no burden, you do not need to acknowledge this source in your use of this source.

namespace BabyDoc.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PropertyTests : BabyDocCodeFixVerifier
    {
        [TestMethod]
        public void Private()
        {
            this.BabyDocTester(
                new[]
                {
                    "private static string $Text${ get; set; }"
                },
                null);
        }

        [TestMethod]
        public void MissingDocumentationGet()
        {
            this.BabyDocTester(
                new[]
                {
                    "public static string $Text${ get; private set; }"
                },
                new[]
                {
                    "", //// FIXME = remove this line
                    "/// <summary>Gets the [Text]</summary>",
                    "/// <returns>[String]</returns>",
                    "public static string Text{ get; private set; }"
                });
        }

        [TestMethod]
        public void MissingDocumentationGetSet()
        {
            this.BabyDocTester(
                new[]
                {
                    "public static string $Text${ get; set; }"
                },
                new[]
                {
                    "", //// FIXME = remove this line
                    "/// <summary>Gets or sets the [Text]</summary>",
                    "/// <returns>[String]</returns>",
                    "public static string Text{ get; set; }"
                });
        }
    }
}
