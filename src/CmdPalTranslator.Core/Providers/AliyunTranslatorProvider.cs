using CmdPalTranslator.Models;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CmdPalTranslator.Providers
{
    internal sealed partial class AliyunTranslatorProvider : ITranslatorProvider
    {
        private const string TranslateEndpoint = "https://translate.alibaba.com/api/translate/text";
        private const string CsrfEndpoint = "https://translate.alibaba.com/api/translate/csrftoken";
        private const string Referer = "https://translate.alibaba.com/";
        private const string Origin = "https://translate.alibaba.com";
        private readonly HttpClient _httpClient;
        private readonly object _csrfLock = new();
        private CachedCsrf? _cachedCsrf;
        private DateTimeOffset _csrfExpiresAt = DateTimeOffset.MinValue;

        public AliyunTranslatorProvider() : this(TranslatorHttpClient.Create()) { }

        internal AliyunTranslatorProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Referrer = new Uri(Referer);
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Origin", Origin);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public string Id => "aliyun";

        public string DisplayName => "Aliyun";

        public string Description => "Use the Alibaba Translate web endpoint.";

        public TranslationResponse Translate(ParsedTranslationQuery query, CancellationToken cancellationToken)
        {
            string sourceLanguage = query.SourceLanguage.GetProviderCode(Id);
            string targetLanguage = query.TargetLanguage.GetProviderCode(Id);
            CachedCsrf csrf = EnsureCsrf(cancellationToken);
            return SendTranslateRequest(query, sourceLanguage, targetLanguage, csrf, retryOnCsrfFailure: true, cancellationToken);
        }

        public Uri BuildWebUri(ParsedTranslationQuery query)
        {
            string sourceLanguage = query.SourceLanguage.GetProviderCode(Id);
            string targetLanguage = query.TargetLanguage.GetProviderCode(Id);
            return new Uri(
                $"https://translate.alibaba.com/?sourceLanguage={Uri.EscapeDataString(sourceLanguage)}&targetLanguage={Uri.EscapeDataString(targetLanguage)}&sourceText={Uri.EscapeDataString(query.SourceText)}");
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        private TranslationResponse SendTranslateRequest(
            ParsedTranslationQuery query,
            string sourceLanguage,
            string targetLanguage,
            CachedCsrf csrf,
            bool retryOnCsrfFailure,
            CancellationToken cancellationToken)
        {
            using MultipartFormDataContent formData = new()
            {
                { new StringContent(sourceLanguage), "srcLang" },
                { new StringContent(targetLanguage), "tgtLang" },
                { new StringContent("general"), "domain" },
                { new StringContent(query.SourceText), "query" },
                { new StringContent(csrf.Token), "_csrf" },
            };

            using HttpRequestMessage request = new(HttpMethod.Post, TranslateEndpoint)
            {
                Content = formData,
            };

            using HttpResponseMessage response = _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode)
            {
                if (retryOnCsrfFailure && (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized))
                {
                    InvalidateCsrf();
                    return SendTranslateRequest(
                        query,
                        sourceLanguage,
                        targetLanguage,
                        EnsureCsrf(cancellationToken),
                        retryOnCsrfFailure: false,
                        cancellationToken);
                }

                response.EnsureSuccessStatusCode();
            }

            string content = response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
            Debug.WriteLine($"Translate response: {content}");

            AliyunTranslatePayload payload = JsonSerializer.Deserialize(content, AliyunJsonContext.Default.AliyunTranslatePayload)
                ?? throw new InvalidOperationException("Aliyun translation returned an empty response.");

            if (!payload.Success || string.IsNullOrWhiteSpace(payload.Data?.TranslateText))
            {
                throw new InvalidOperationException(
                    $"Aliyun translation returned no translated text. Code: {payload.Code ?? "unknown"}");
            }

            string translatedText = payload.Data.TranslateText;
            string detectedLanguage = payload.Data.DetectLanguage ?? query.SourceLanguage.Id;
            string targetDisplayName = LanguageCatalog.ToDisplayName(query.TargetLanguage.Id);

            return new TranslationResponse(
                ProviderId: Id,
                ProviderDisplayName: DisplayName,
                SourceLanguage: detectedLanguage,
                TargetLanguage: query.TargetLanguage.Id,
                SourceText: query.SourceText,
                Entries:
                [
                    new TranslationEntry(
                        Title: translatedText,
                        Subtitle: $"{LanguageCatalog.ToDisplayName(detectedLanguage)} -> {targetDisplayName}",
                        CopyText: translatedText,
                        Description: $"{translatedText}\n{query.SourceText}",
                        Category: "Translation"),
                ],
                WebUri: BuildWebUri(query));
        }

        private CachedCsrf EnsureCsrf(CancellationToken cancellationToken)
        {
            if (_cachedCsrf is not null && DateTimeOffset.UtcNow < _csrfExpiresAt)
            {
                return _cachedCsrf;
            }

            lock (_csrfLock)
            {
                if (_cachedCsrf is not null && DateTimeOffset.UtcNow < _csrfExpiresAt)
                {
                    return _cachedCsrf;
                }

                using HttpRequestMessage request = new(HttpMethod.Get, CsrfEndpoint);
                using HttpResponseMessage response = _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();

                string content = response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
                AliyunCsrfPayload payload = JsonSerializer.Deserialize(content, AliyunJsonContext.Default.AliyunCsrfPayload)
                    ?? throw new InvalidOperationException("Aliyun CSRF endpoint returned an empty response.");

                if (string.IsNullOrWhiteSpace(payload.Token) || string.IsNullOrWhiteSpace(payload.HeaderName))
                {
                    throw new InvalidOperationException("Aliyun CSRF endpoint did not return the required authentication metadata.");
                }

                if (_cachedCsrf is not null && _httpClient.DefaultRequestHeaders.Contains(_cachedCsrf.HeaderName))
                {
                    _httpClient.DefaultRequestHeaders.Remove(_cachedCsrf.HeaderName);
                }

                _cachedCsrf = new CachedCsrf(payload.Token, payload.HeaderName);
                _httpClient.DefaultRequestHeaders.Remove(payload.HeaderName);
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(payload.HeaderName, payload.Token);
                _csrfExpiresAt = DateTimeOffset.UtcNow.AddMinutes(20);
                return _cachedCsrf;
            }
        }

        private void InvalidateCsrf()
        {
            lock (_csrfLock)
            {
                if (_cachedCsrf is not null)
                {
                    _httpClient.DefaultRequestHeaders.Remove(_cachedCsrf.HeaderName);
                }

                _cachedCsrf = null;
                _csrfExpiresAt = DateTimeOffset.MinValue;
            }
        }

        private sealed record CachedCsrf(string Token, string HeaderName);

        private sealed class AliyunCsrfPayload
        {
            [JsonPropertyName("token")]
            public string? Token { get; set; }

            [JsonPropertyName("headerName")]
            public string? HeaderName { get; set; }
        }

        private sealed class AliyunTranslatePayload
        {
            [JsonPropertyName("code")]
            public string? Code { get; set; }

            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("data")]
            public AliyunTranslateData? Data { get; set; }
        }

        private sealed class AliyunTranslateData
        {
            [JsonPropertyName("detectLanguage")]
            public string? DetectLanguage { get; set; }

            [JsonPropertyName("translateText")]
            public string? TranslateText { get; set; }
        }

        // 使用 NativeAOT 建置應用程式時，會需要標註序列化會涉及的型別，讓應用程式可以正確序列化和反序列化這些型別。
        [JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
        [JsonSerializable(typeof(AliyunCsrfPayload))]
        [JsonSerializable(typeof(AliyunTranslatePayload))]
        [JsonSerializable(typeof(AliyunTranslateData))]
        private sealed partial class AliyunJsonContext : JsonSerializerContext { }
    }
}
