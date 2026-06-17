using CmdPalTranslator.Core.Models;
using CmdPalTranslator.Core.Providers;
using System.Net;
using System.Text;

namespace CmdPalTranslator.Tests
{
    [TestClass]
    [TestCategory("Unit")]
    public sealed class ProviderTranslationUnitTests
    {
        [TestMethod]
        public void BingProviderTranslatesTraditionalChineseAndEnglish()
        {
            int authRequests = 0;
            List<Dictionary<string, string>> formPayloads = [];

            using HttpClient httpClient = new(new StubHttpMessageHandler(request =>
            {
                if (request.Method == HttpMethod.Get)
                {
                    authRequests++;
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("""
                    <html>
                    <body data-iid="translator.5023">
                    <script>
                    var params_AbusePreventionHelper = [123456789,"bing-token",3600000];
                    var _IG="bing-ig";
                    IG:"bing-ig"
                    </script>
                    </body>
                    </html>
                    """, Encoding.UTF8, "text/html"),
                    };
                }

                Dictionary<string, string> formValues = ReadFormValues(request);
                formPayloads.Add(formValues);

                string translatedText = formValues["text"] == "蘋果" ? "apple" : "蘋果";
                string detectedLanguage = formValues["fromLang"] == "zh-Hant" ? "zh-Hant" : "en";
                string targetLanguage = formValues["to"];

                string json = $$"""
            [
              {
                "detectedLanguage": { "language": "{{detectedLanguage}}" },
                "translations": [
                  { "text": "{{translatedText}}", "to": "{{targetLanguage}}" }
                ]
              }
            ]
            """;

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                };
            }));

            using BingTranslatorProvider provider = new(httpClient);

            TranslationResponse zhToEn = provider.Translate(
                CreateQuery("蘋果", sourceLanguageId: "zht", targetLanguageId: "en"),
                CancellationToken.None);

            TranslationResponse enToZh = provider.Translate(
                CreateQuery("apple", sourceLanguageId: "en", targetLanguageId: "zht"),
                CancellationToken.None);

            CollectionAssert.Contains([.. zhToEn.Entries.Select(entry => entry.Title)], "apple", StringComparer.OrdinalIgnoreCase);
            CollectionAssert.Contains([.. enToZh.Entries.Select(entry => entry.Title)], "蘋果");
            Assert.AreEqual(1, authRequests);
            Assert.AreEqual("zh-Hant", formPayloads[0]["fromLang"]);
            Assert.AreEqual("en", formPayloads[0]["to"]);
            Assert.AreEqual("en", formPayloads[1]["fromLang"]);
            Assert.AreEqual("zh-Hant", formPayloads[1]["to"]);
            foreach (Dictionary<string, string> payload in formPayloads)
            {
                Assert.AreEqual("bing-token", payload["token"]);
                Assert.AreEqual("123456789", payload["key"]);
            }
        }

        [TestMethod]
        public void GoogleProviderTranslatesTraditionalChineseAndEnglish()
        {
            List<string> requestUris = [];
            using HttpClient httpClient = new(new StubHttpMessageHandler(request =>
            {
                requestUris.Add(request.RequestUri!.ToString());

                string json = request.RequestUri!.Query.Contains("q=%E8%98%8B%E6%9E%9C", StringComparison.Ordinal)
                    ? """
                {
                  "sentences": [{ "trans": "apple" }],
                  "dict": [{ "pos": "noun", "terms": ["apple", "pome"] }],
                  "src": "zh-TW"
                }
                """
                    : """
                {
                  "sentences": [{ "trans": "蘋果" }],
                  "dict": [{ "pos": "noun", "terms": ["蘋果", "苹果"] }],
                  "src": "en"
                }
                """;

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                };
            }));

            using GoogleTranslatorProvider provider = new(httpClient);

            TranslationResponse zhToEn = provider.Translate(
                CreateQuery("蘋果", sourceLanguageId: "zht", targetLanguageId: "en"),
                CancellationToken.None);

            TranslationResponse enToZh = provider.Translate(
                CreateQuery("apple", sourceLanguageId: "en", targetLanguageId: "zht"),
                CancellationToken.None);

            CollectionAssert.Contains([.. zhToEn.Entries.Select(entry => entry.Title)], "apple", StringComparer.OrdinalIgnoreCase);
            CollectionAssert.Contains([.. enToZh.Entries.Select(entry => entry.Title)], "蘋果");
            Assert.Contains(uri => uri.Contains("sl=zh-TW", StringComparison.Ordinal) && uri.Contains("tl=en", StringComparison.Ordinal), requestUris);
            Assert.Contains(uri => uri.Contains("sl=en", StringComparison.Ordinal) && uri.Contains("tl=zh-TW", StringComparison.Ordinal), requestUris);
        }

        [TestMethod]
        public void AliyunProviderTranslatesTraditionalChineseAndEnglish()
        {
            int csrfRequests = 0;
            List<Dictionary<string, string>> formPayloads = [];
            List<string?> csrfHeaders = [];

            using HttpClient httpClient = new(new StubHttpMessageHandler(request =>
            {
                if (request.Method == HttpMethod.Get)
                {
                    csrfRequests++;
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("""
                    {
                      "token": "aliyun-csrf-token",
                      "headerName": "x-csrf-token"
                    }
                    """, Encoding.UTF8, "application/json"),
                    };
                }

                formPayloads.Add(ReadFormValues(request));
                request.Headers.TryGetValues("x-csrf-token", out IEnumerable<string>? headerValues);
                csrfHeaders.Add(headerValues?.FirstOrDefault());

                Dictionary<string, string> latestPayload = formPayloads[^1];
                bool isZhToEn = latestPayload.TryGetValue("srcLang", out string? srcLang)
                    && string.Equals(srcLang, "zh-tw", StringComparison.Ordinal);
                string translatedText = isZhToEn ? "apple" : "蘋果";
                string detectedLanguage = isZhToEn ? "zh-tw" : "en";

                string json = $$"""
            {
              "code": "200",
              "success": true,
              "data": {
                "detectLanguage": "{{detectedLanguage}}",
                "translateText": "{{translatedText}}"
              }
            }
            """;

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                };
            }));

            using AliyunTranslatorProvider provider = new(httpClient);

            TranslationResponse zhToEn = provider.Translate(
                CreateQuery("蘋果", sourceLanguageId: "zht", targetLanguageId: "en"),
                CancellationToken.None);

            TranslationResponse enToZh = provider.Translate(
                CreateQuery("apple", sourceLanguageId: "en", targetLanguageId: "zht"),
                CancellationToken.None);

            CollectionAssert.Contains([.. zhToEn.Entries.Select(entry => entry.Title)], "apple", StringComparer.OrdinalIgnoreCase);
            CollectionAssert.Contains([.. enToZh.Entries.Select(entry => entry.Title)], "蘋果");
            Assert.AreEqual(1, csrfRequests);

            Assert.AreEqual("zh-tw", formPayloads[0]["srcLang"]);
            Assert.AreEqual("en", formPayloads[0]["tgtLang"]);
            Assert.AreEqual("en", formPayloads[1]["srcLang"]);
            Assert.AreEqual("zh-tw", formPayloads[1]["tgtLang"]);

            foreach (Dictionary<string, string> payload in formPayloads)
            {
                Assert.AreEqual("general", payload["domain"]);
                Assert.AreEqual("aliyun-csrf-token", payload["_csrf"]);
            }

            Assert.IsTrue(csrfHeaders.All(value => string.Equals(value, "aliyun-csrf-token", StringComparison.Ordinal)));
        }

        private static ParsedTranslationQuery CreateQuery(string text, string sourceLanguageId, string targetLanguageId)
        {
            return new ParsedTranslationQuery(
                SourceText: text,
                SourceLanguage: LanguageCatalog.GetById(sourceLanguageId),
                TargetLanguage: LanguageCatalog.GetById(targetLanguageId),
                HasExplicitTargetLanguage: true);
        }

        private static Dictionary<string, string> ReadFormValues(HttpRequestMessage request)
        {
            if (request.Content is MultipartContent multipartContent)
            {
                Dictionary<string, string> values = new(StringComparer.Ordinal);
                foreach (HttpContent part in multipartContent)
                {
                    string? name = part.Headers.ContentDisposition?.Name?.Trim('"');
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    values[name] = part.ReadAsStringAsync().GetAwaiter().GetResult();
                }

                return values;
            }

            string formBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
            return formBody.Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split('=', 2))
                .ToDictionary(
                    pair => Uri.UnescapeDataString(pair[0]),
                    pair => pair.Length > 1 ? Uri.UnescapeDataString(pair[1]) : string.Empty,
                    StringComparer.Ordinal);
        }

        private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
        {
            protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return handler(request);
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(handler(request));
            }
        }

        private static class CollectionAssert
        {
            public static void Contains(IReadOnlyCollection<string> actual, string expected)
            {
                bool found = actual.Any(item => string.Equals(item, expected, StringComparison.Ordinal));
                Assert.IsTrue(found, $"Expected '{expected}' in [{string.Join(", ", actual)}].");
            }

            public static void Contains(IReadOnlyCollection<string> actual, string expected, StringComparer comparer)
            {
                bool found = actual.Any(item => comparer.Equals(item, expected));
                Assert.IsTrue(found, $"Expected '{expected}' in [{string.Join(", ", actual)}].");
            }
        }
    }
}
