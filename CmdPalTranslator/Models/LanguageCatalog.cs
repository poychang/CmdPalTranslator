using System;
using System.Collections.Generic;
using System.Linq;

namespace CmdPalTranslator.Models
{
    internal sealed record LanguageOption(
    string Id,
    string DisplayName,
    string GoogleCode,
    string BingCode,
    params string[] Aliases)
    {
        public string GetProviderCode(string providerId) => providerId switch
        {
            "google" => GoogleCode,
            "bing" => BingCode,
            _ => BingCode,
        };

        public bool Matches(string value)
        {
            return string.Equals(Id, value, StringComparison.OrdinalIgnoreCase)
                || string.Equals(DisplayName, value, StringComparison.OrdinalIgnoreCase)
                || Aliases.Any(alias => string.Equals(alias, value, StringComparison.OrdinalIgnoreCase));
        }
    }

    internal static class LanguageCatalog
    {
        private static readonly IReadOnlyList<LanguageOption> Languages =
        [
            new("auto", "Auto Detect", "auto", "auto-detect", "detect", "default"),
            new("zhs", "Chinese (Simplified)", "zh-CN", "zh-Hans", "zh-cn", "zh-hans", "simplified chinese"),
            new("zht", "Chinese (Traditional)", "zh-TW", "zh-Hant", "zh-tw", "zh-hant", "traditional chinese"),
            new("en", "English", "en", "en", "english"),
            new("ja", "Japanese", "ja", "ja", "japanese"),
            new("ko", "Korean", "ko", "ko", "korean"),
            new("fr", "French", "fr", "fr", "french"),
            new("de", "German", "de", "de", "german"),
            new("es", "Spanish", "es", "es", "spanish"),
            new("it", "Italian", "it", "it", "italian"),
            new("ru", "Russian", "ru", "ru", "russian"),
            new("ar", "Arabic", "ar", "ar", "arabic"),
            new("he", "Hebrew", "iw", "he", "hebrew"),
            new("pt", "Portuguese", "pt", "pt", "portuguese"),
            new("th", "Thai", "th", "th", "thai"),
        ];

        public static IReadOnlyList<LanguageOption> All => Languages;

        public static LanguageOption AutoDetect => GetById("auto");

        public static LanguageOption BuiltInDefaultTarget => GetById("zht");

        public static LanguageOption GetById(string id)
        {
            return Languages.First(language => string.Equals(language.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public static bool TryResolve(string value, out LanguageOption? language)
        {
            language = Languages.FirstOrDefault(item => item.Matches(value.Trim()));
            return language is not null;
        }

        public static string ToDisplayName(string idOrCode)
        {
            if (TryResolve(idOrCode, out var language))
            {
                return language!.DisplayName;
            }

            if (Languages.FirstOrDefault(item =>
                string.Equals(item.GoogleCode, idOrCode, StringComparison.OrdinalIgnoreCase)
                || string.Equals(item.BingCode, idOrCode, StringComparison.OrdinalIgnoreCase)) is { } fromProviderCode)
            {
                return fromProviderCode.DisplayName;
            }

            return idOrCode;
        }
    }
}
