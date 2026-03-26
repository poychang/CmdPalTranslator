using CmdPalTranslator.Models;
using CmdPalTranslator.Providers;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace CmdPalTranslator.Tests
{
    [TestClass]
    public sealed class LiveTranslationProviderTests
    {
        [TestMethod]
        [DataRow("Bing")]
        [DataRow("Google")]
        public void ProvidersTranslateBetweenTraditionalChineseAndEnglish(string providerId)
        {
            using ITranslatorProvider provider = CreateProvider(providerId);

            TranslationResponse zhToEnResponse = provider.Translate(
                CreateQuery("蘋果", sourceLanguageId: "zht", targetLanguageId: "en"),
                CancellationToken.None);

            string zhToEnJoined = string.Join(" | ", zhToEnResponse.Entries.Select(entry => entry.Title));

            TranslationResponse enToZhResponse = provider.Translate(
                CreateQuery("apple", sourceLanguageId: "en", targetLanguageId: "zht"),
                CancellationToken.None);

            string enToZhJoined = string.Join(" | ", enToZhResponse.Entries.Select(entry => entry.Title));

            Trace.WriteLine($"Provider: {provider.DisplayName}");
            Trace.WriteLine($"ZH->EN: {zhToEnJoined}");
            Trace.WriteLine($"EN->ZH: {enToZhJoined}");

            Assert.IsNotEmpty(zhToEnResponse.Entries);
            AssertContainsAny(zhToEnJoined, "apple");

            Assert.IsNotEmpty(enToZhResponse.Entries);
            AssertContainsAny(enToZhJoined, "蘋果", "苹果");
        }

        private static ParsedTranslationQuery CreateQuery(string text, string sourceLanguageId, string targetLanguageId)
        {
            return new ParsedTranslationQuery(
                SourceText: text,
                SourceLanguage: LanguageCatalog.GetById(sourceLanguageId),
                TargetLanguage: LanguageCatalog.GetById(targetLanguageId),
                HasExplicitTargetLanguage: true);
        }

        private static ITranslatorProvider CreateProvider(string providerId) => providerId switch
        {
            "Bing" => new BingTranslatorProvider(),
            "Google" => new GoogleTranslatorProvider(),
            _ => throw new ArgumentOutOfRangeException(nameof(providerId), providerId, "Unknown provider."),
        };

        private static void AssertContainsAny(string actual, params string[] expectedValues)
        {
            string normalizedActual = Normalize(actual);

            Assert.IsTrue(
                expectedValues.Any(expected => normalizedActual.Contains(Normalize(expected), StringComparison.Ordinal)),
                $"Expected one of [{string.Join(", ", expectedValues)}] in '{actual}'.");
        }

        private static string Normalize(string value)
        {
            IEnumerable<char> filtered = value.Normalize(NormalizationForm.FormKC)
                .Where(ch => !char.IsWhiteSpace(ch) && !char.IsPunctuation(ch) && !char.IsControl(ch));

            return new string([.. filtered]).ToLower(CultureInfo.InvariantCulture);
        }
    }
}
