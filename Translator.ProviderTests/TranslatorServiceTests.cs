using CmdPalTranslator.Services;

namespace Translator.ProviderTests
{
    [TestClass]
    public sealed class TranslatorServiceTests
    {
        [TestMethod]
        public void ParseQueryUsesConfiguredDefaultTargetLanguageWhenQueryHasNoOverride()
        {
            string settingsFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");

            try
            {
                TranslatorSettingsService settings = new(settingsFilePath);
                settings.SetTargetLanguage(CmdPalTranslator.Models.LanguageCatalog.GetById("ja"));

                using TranslatorService service = new(settings);
                var parsed = service.ParseQuery("hello world");

                Assert.AreEqual("ja", parsed.TargetLanguage.Id);
                Assert.IsFalse(parsed.HasExplicitTargetLanguage);
            }
            finally
            {
                if (File.Exists(settingsFilePath))
                {
                    File.Delete(settingsFilePath);
                }
            }
        }

        [TestMethod]
        public void ParseQueryKeepsExplicitTargetLanguageEvenWhenDefaultIsDifferent()
        {
            string settingsFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");

            try
            {
                TranslatorSettingsService settings = new(settingsFilePath);
                settings.SetTargetLanguage(CmdPalTranslator.Models.LanguageCatalog.GetById("ja"));

                using TranslatorService service = new(settings);
                var parsed = service.ParseQuery("hello world -> fr");

                Assert.AreEqual("fr", parsed.TargetLanguage.Id);
                Assert.IsTrue(parsed.HasExplicitTargetLanguage);
            }
            finally
            {
                if (File.Exists(settingsFilePath))
                {
                    File.Delete(settingsFilePath);
                }
            }
        }
    }
}
