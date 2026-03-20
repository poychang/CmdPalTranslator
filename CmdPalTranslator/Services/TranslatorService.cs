using CmdPalTranslator.Models;
using CmdPalTranslator.Providers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CmdPalTranslator.Services
{
    internal sealed partial class TranslatorService : IDisposable
    {
        public const string DefaultProviderId = "bing";
        private readonly TranslatorSettingsService _settings;
        private readonly IReadOnlyList<ITranslatorProvider> _providers;
        private readonly Dictionary<string, ITranslatorProvider> _providerMap;

        public TranslatorService(TranslatorSettingsService? settings = null)
        {
            _settings = settings ?? new TranslatorSettingsService();
            _providers =
            [
                new BingTranslatorProvider(),
                new GoogleTranslatorProvider(),
            ];

            _providerMap = _providers.ToDictionary(provider => provider.Id, StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyList<ITranslatorProvider> Providers => _providers;

        public TranslatorSettingsService Settings => _settings;

        public ParsedTranslationQuery ParseQuery(string searchText)
        {
            string trimmed = searchText.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return new ParsedTranslationQuery(string.Empty, LanguageCatalog.AutoDetect, _settings.TargetLanguage, false);
            }

            int splitIndex = trimmed.LastIndexOf("->", StringComparison.Ordinal);
            if (splitIndex > 0)
            {
                string candidateText = trimmed[..splitIndex].Trim();
                string candidateLanguage = trimmed[(splitIndex + 2)..].Trim();
                if (!string.IsNullOrWhiteSpace(candidateText) && LanguageCatalog.TryResolve(candidateLanguage, out var language))
                {
                    return new ParsedTranslationQuery(candidateText, LanguageCatalog.AutoDetect, language!, true);
                }
            }

            return new ParsedTranslationQuery(trimmed, LanguageCatalog.AutoDetect, _settings.TargetLanguage, false);
        }

        public ITranslatorProvider GetProvider(string? providerId)
        {
            if (!string.IsNullOrWhiteSpace(providerId) && _providerMap.TryGetValue(providerId, out var provider))
            {
                return provider;
            }

            return _providerMap[DefaultProviderId];
        }

        public void Dispose()
        {
            foreach (ITranslatorProvider provider in _providers)
            {
                provider.Dispose();
            }
        }
    }
}
