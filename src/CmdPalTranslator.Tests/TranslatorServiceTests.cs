using CmdPalTranslator.Core.Models;
using CmdPalTranslator.Core.Services;

namespace CmdPalTranslator.Tests
{
    [TestClass]
    [TestCategory("Unit")]
    public sealed class TranslatorServiceTests
    {
        [TestMethod]
        public void ParseQueryUsesConfiguredDefaultTargetLanguageWhenQueryHasNoOverride()
        {
            using TranslatorService service = new();

            var parsed = service.ParseQuery("hello world", LanguageCatalog.GetById("ja"));

            Assert.AreEqual("ja", parsed.TargetLanguage.Id);
            Assert.IsFalse(parsed.HasExplicitTargetLanguage);
        }

        [TestMethod]
        public void ParseQueryKeepsExplicitTargetLanguageEvenWhenDefaultIsDifferent()
        {
            using TranslatorService service = new();

            var parsed = service.ParseQuery("hello world >> fr", LanguageCatalog.GetById("ja"));

            Assert.AreEqual("fr", parsed.TargetLanguage.Id);
            Assert.IsTrue(parsed.HasExplicitTargetLanguage);
        }

        [TestMethod]
        public void ParseQuerySupportsCustomTranslateOperator()
        {
            using TranslatorService service = new();

            var parsed = service.ParseQuery("hello world => fr", LanguageCatalog.GetById("ja"), "=>");

            Assert.AreEqual("fr", parsed.TargetLanguage.Id);
            Assert.IsTrue(parsed.HasExplicitTargetLanguage);
        }
    }
}
