using CmdPalTranslator.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace CmdPalTranslator.Providers
{
    internal sealed partial class GoogleTranslatorProvider : ITranslatorProvider
    {
        private readonly HttpClient _httpClient;

        public GoogleTranslatorProvider() : this(CreateHttpClient()) { }

        internal GoogleTranslatorProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string Id => "google";

        public string DisplayName => "Google";

        public string Description => "Use the Google Translate web endpoint.";

        public TranslationResponse Translate(ParsedTranslationQuery query, CancellationToken cancellationToken)
        {
            string sourceLanguage = query.SourceLanguage.GetProviderCode(Id);
            string targetLanguage = query.TargetLanguage.GetProviderCode(Id);

            string requestUri =
                $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sourceLanguage}&tl={targetLanguage}&hl=en&dt=t&dt=bd&dj=1&q={Uri.EscapeDataString(query.SourceText)}";

            using HttpRequestMessage request = new(HttpMethod.Get, requestUri);
            using HttpResponseMessage response = _httpClient.Send(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            string content = response.Content.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
            Debug.WriteLine($"Translate response: {content}");
            GoogleTranslatePayload payload = JsonSerializer.Deserialize<GoogleTranslatePayload>(content, GoogleJsonContext.Default.GoogleTranslatePayload)
                ?? throw new InvalidOperationException("Google translation returned an empty response.");

            string translatedText = string.Concat(payload.Sentences?.Select(sentence => sentence.Trans) ?? []);
            if (string.IsNullOrWhiteSpace(translatedText))
            {
                throw new InvalidOperationException("Google translation did not return translated text.");
            }

            List<TranslationEntry> entries =
            [
                new TranslationEntry(
                Title: translatedText,
                Subtitle: $"{LanguageCatalog.ToDisplayName(payload.Src ?? query.SourceLanguage.Id)} -> {query.TargetLanguage.DisplayName}",
                CopyText: translatedText,
                Description: $"{translatedText}\n{query.SourceText}",
                Category: "Translation"),
            ];

            foreach (GoogleDictionaryEntry dictionaryEntry in payload.Dict ?? [])
            {
                foreach (string term in dictionaryEntry.Terms ?? [])
                {
                    entries.Add(new TranslationEntry(
                        Title: term,
                        Subtitle: dictionaryEntry.Pos ?? "Dictionary",
                        CopyText: term,
                        Description: $"{dictionaryEntry.Pos}: {term}",
                        Category: "Dictionary"));
                }
            }

            return new TranslationResponse(
                ProviderId: Id,
                ProviderDisplayName: DisplayName,
                SourceLanguage: payload.Src ?? query.SourceLanguage.Id,
                TargetLanguage: query.TargetLanguage.Id,
                SourceText: query.SourceText,
                Entries: entries,
                WebUri: BuildWebUri(query));
        }

        public Uri BuildWebUri(ParsedTranslationQuery query)
        {
            string targetLanguage = query.TargetLanguage.GetProviderCode(Id);
            return new Uri($"https://translate.google.com/?sl=auto&tl={targetLanguage}&text={Uri.EscapeDataString(query.SourceText)}&op=translate");
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        private static HttpClient CreateHttpClient()
        {
            SocketsHttpHandler handler = new()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            };

            HttpClient client = new(handler)
            {
                Timeout = TimeSpan.FromSeconds(15),
            };
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) CmdPalTranslator/1.0");
            return client;
        }

        private sealed class GoogleTranslatePayload
        {
            [JsonPropertyName("sentences")]
            public GoogleSentence[]? Sentences { get; set; }

            [JsonPropertyName("dict")]
            public GoogleDictionaryEntry[]? Dict { get; set; }

            [JsonPropertyName("src")]
            public string? Src { get; set; }
        }

        private sealed class GoogleSentence
        {
            [JsonPropertyName("trans")]
            public string? Trans { get; set; }
        }

        private sealed class GoogleDictionaryEntry
        {
            [JsonPropertyName("pos")]
            public string? Pos { get; set; }

            [JsonPropertyName("terms")]
            public string[]? Terms { get; set; }
        }

        // 使用 NativeAOT 建置應用程式時，會需要標註序列化會涉及的型別，讓應用程式可以正確序列化和反序列化這些型別。
        [JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
        [JsonSerializable(typeof(GoogleTranslatePayload))]
        [JsonSerializable(typeof(GoogleTranslatePayload[]))]
        [JsonSerializable(typeof(GoogleSentence))]
        [JsonSerializable(typeof(GoogleDictionaryEntry))]
        private sealed partial class GoogleJsonContext : JsonSerializerContext { }
    }
}
