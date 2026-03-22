using CmdPalTranslator.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;

namespace CmdPalTranslator.Providers
{
    internal sealed partial class BingTranslatorProvider : ITranslatorProvider
    {
        private static readonly Regex AbuseRegex = MyAbuseRegex();
        private static readonly Regex IgRegex = MyIgRegex();
        private static readonly Regex IidRegex = MyIidRegex();
        private readonly HttpClient _httpClient;
        private readonly object _authLock = new();
        private BingAuth? _cachedAuth;
        private DateTimeOffset _authExpiresAt = DateTimeOffset.MinValue;

        public BingTranslatorProvider() : this(CreateHttpClient()) { }

        internal BingTranslatorProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string Id => "bing";

        public string DisplayName => "Bing";

        public string Description => "Use the Bing Translator web endpoint.";

        public TranslationResponse Translate(ParsedTranslationQuery query, CancellationToken cancellationToken)
        {
            BingAuth auth = EnsureAuth(cancellationToken);
            return SendTranslateRequest(query, auth, retryOnAuthFailure: true, cancellationToken);
        }

        public Uri BuildWebUri(ParsedTranslationQuery query)
        {
            string targetLanguage = query.TargetLanguage.GetProviderCode(Id);
            return new Uri($"https://www.bing.com/translator?from=auto-detect&to={targetLanguage}&text={Uri.EscapeDataString(query.SourceText)}");
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        private TranslationResponse SendTranslateRequest(
            ParsedTranslationQuery query,
            BingAuth auth,
            bool retryOnAuthFailure,
            CancellationToken cancellationToken)
        {
            string sourceLanguage = query.SourceLanguage.GetProviderCode(Id);
            string targetLanguage = query.TargetLanguage.GetProviderCode(Id);

            Dictionary<string, string> form = new()
            {
                ["fromLang"] = sourceLanguage,
                ["text"] = query.SourceText,
                ["to"] = targetLanguage,
                ["token"] = auth.Token,
                ["key"] = auth.Key,
            };

            string endpoint = $"https://www.bing.com/ttranslatev3?isVertical=1&IG={auth.Ig}&IID={auth.Iid}";
            using HttpRequestMessage request = new(HttpMethod.Post, endpoint)
            {
                Content = new FormUrlEncodedContent(form),
            };

            using HttpResponseMessage response = _httpClient.Send(request, cancellationToken);
            if (!response.IsSuccessStatusCode && retryOnAuthFailure)
            {
                InvalidateAuth();
                return SendTranslateRequest(query, EnsureAuth(cancellationToken), retryOnAuthFailure: false, cancellationToken);
            }

            response.EnsureSuccessStatusCode();

            string content = response.Content.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
            Debug.WriteLine($"Translate response: {content}");
            BingTranslatePayload[] payload = JsonSerializer.Deserialize<BingTranslatePayload[]>(content, BingJsonContext.Default.BingTranslatePayloadArray)
                ?? throw new InvalidOperationException("Bing translation returned an empty response.");

            BingTranslatePayload first = payload.FirstOrDefault()
                ?? throw new InvalidOperationException("Bing translation returned no translation payload.");

            List<TranslationEntry> entries = first.Translations?
                .Select(translation => new TranslationEntry(
                    Title: translation.Text ?? string.Empty,
                    Subtitle: $"{LanguageCatalog.ToDisplayName(first.DetectedLanguage?.Language ?? query.SourceLanguage.Id)} -> {LanguageCatalog.ToDisplayName(translation.To ?? query.TargetLanguage.Id)}",
                    CopyText: translation.Text ?? string.Empty,
                    Description: $"{translation.Text}\n{query.SourceText}",
                    Category: "Translation"))
                .Where(entry => !string.IsNullOrWhiteSpace(entry.Title))
                .ToList()
                ?? [];

            if (entries.Count == 0)
            {
                throw new InvalidOperationException("Bing translation returned no translated text.");
            }

            return new TranslationResponse(
                ProviderId: Id,
                ProviderDisplayName: DisplayName,
                SourceLanguage: first.DetectedLanguage?.Language ?? query.SourceLanguage.Id,
                TargetLanguage: query.TargetLanguage.Id,
                SourceText: query.SourceText,
                Entries: entries,
                WebUri: BuildWebUri(query));
        }

        private BingAuth EnsureAuth(CancellationToken cancellationToken)
        {
            if (_cachedAuth is not null && DateTimeOffset.UtcNow < _authExpiresAt)
            {
                return _cachedAuth;
            }

            lock (_authLock)
            {
                if (_cachedAuth is not null && DateTimeOffset.UtcNow < _authExpiresAt)
                {
                    return _cachedAuth;
                }

                using HttpRequestMessage request = new(HttpMethod.Get, "https://www.bing.com/translator");
                using HttpResponseMessage response = _httpClient.Send(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                string html = response.Content.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
                Match abuseMatch = AbuseRegex.Match(html);
                Match igMatch = IgRegex.Match(html);
                Match iidMatch = IidRegex.Match(html);

                if (!abuseMatch.Success || !igMatch.Success)
                {
                    throw new InvalidOperationException("Failed to extract Bing translator authentication metadata.");
                }

                _cachedAuth = new BingAuth(
                    Key: abuseMatch.Groups["key"].Value,
                    Token: abuseMatch.Groups["token"].Value,
                    Ig: igMatch.Groups["ig"].Value,
                    Iid: iidMatch.Success ? iidMatch.Groups["iid"].Value : "translator.5028");

                _authExpiresAt = DateTimeOffset.UtcNow.AddMinutes(20);
                return _cachedAuth;
            }
        }

        private void InvalidateAuth()
        {
            lock (_authLock)
            {
                _cachedAuth = null;
                _authExpiresAt = DateTimeOffset.MinValue;
            }
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

        private sealed record BingAuth(string Key, string Token, string Ig, string Iid);

        private sealed class BingTranslatePayload
        {
            [JsonPropertyName("detectedLanguage")]
            public BingDetectedLanguage? DetectedLanguage { get; set; }

            [JsonPropertyName("translations")]
            public BingTranslation[]? Translations { get; set; }
        }

        private sealed class BingDetectedLanguage
        {
            [JsonPropertyName("language")]
            public string? Language { get; set; }
        }

        private sealed class BingTranslation
        {
            [JsonPropertyName("text")]
            public string? Text { get; set; }

            [JsonPropertyName("to")]
            public string? To { get; set; }
        }

        // 使用正則表達式從 Bing 翻譯頁面 HTML 中提取認證相關的資訊，包括 token、key、IG 和 IID。
        [GeneratedRegex(@"params_AbusePreventionHelper\s*=\s*\[(?<key>\d+),""(?<token>[^""]+)""(?:,\d+)?\]", RegexOptions.Compiled)]
        private static partial Regex MyAbuseRegex();
        [GeneratedRegex(@"IG:""(?<ig>[^""]+)""", RegexOptions.Compiled)]
        private static partial Regex MyIgRegex();
        [GeneratedRegex(@"data-iid=""(?<iid>[^""]+)""", RegexOptions.Compiled)]
        private static partial Regex MyIidRegex();

        // 使用 NativeAOT 建置應用程式時，會需要標註序列化會涉及的型別，讓應用程式可以正確序列化和反序列化這些型別。
        [JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
        [JsonSerializable(typeof(BingTranslatePayload))]
        [JsonSerializable(typeof(BingTranslatePayload[]))]
        [JsonSerializable(typeof(BingDetectedLanguage))]
        [JsonSerializable(typeof(BingTranslation))]
        private sealed partial class BingJsonContext : JsonSerializerContext { }
    }
}
