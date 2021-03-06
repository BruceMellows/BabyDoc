﻿// NO LICENSE
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
                "Externally visible property '{0}' does not have a documentation comment",
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
                "Externally visible property '{0}' does not have a documentation comment",
                new[]
                {
                    "public static string $Text${ get; private set; }"
                },
                new[]
                {
                    "/// <summary>Gets the [Text]</summary>",
                    "/// <returns>The [Text] of type [String]</returns>",
                    "public static string Text{ get; private set; }"
                });
        }

        [TestMethod]
        public void MissingDocumentationGetSet()
        {
            this.BabyDocTester(
                "Externally visible property '{0}' does not have a documentation comment",
                new[]
                {
                    "public static string $Text${ get; set; }"
                },
                new[]
                {
                    "/// <summary>Gets or sets the [Text]</summary>",
                    "/// <returns>The [Text] of type [String]</returns>",
                    "public static string Text{ get; set; }"
                });
        }
    }
}
