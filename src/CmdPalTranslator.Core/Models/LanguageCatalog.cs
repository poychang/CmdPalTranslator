namespace CmdPalTranslator.Core.Models
{
    internal sealed record LanguageOption(
    string Id,
    string DisplayName,
    string GoogleCode,
    string BingCode,
    string AliyunCode,
    params string[] Aliases)
    {
        public string GetProviderCode(string providerId) => providerId switch
        {
            "google" => GoogleCode,
            "bing" => BingCode,
            "aliyun" => AliyunCode,
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
            new("en", "English", "en", "en", "en", "english"),
            new("zht", "中文（繁體）", "zh-TW", "zh-Hant", "zh-tw", "zh-tw", "zh-hant", "traditional chinese"),
            new("zhs", "中文（简体）", "zh-CN", "zh-Hans", "zh", "zh-cn", "zh-hans", "simplified chinese"),
            new("ja", "日本語", "ja", "ja", "ja", "japanese"),
            new("ko", "한국어", "ko", "ko", "ko", "korean"),
            new("fr", "Français", "fr", "fr", "fr", "french"),
            new("de", "Deutsch", "de", "de", "de", "german"),
            new("es", "española", "es", "es", "es", "spanish"),
            new("it", "Italiano", "it", "it", "it", "italian"),
            new("ru", "Русский", "ru", "ru", "ru", "russian"),
            new("ar", "العربية", "ar", "ar", "ar", "arabic"),
            new("he", "עברית", "iw", "he", "he", "hebrew"),
            new("pt", "Português", "pt", "pt", "pt", "portuguese"),
            new("th", "ไทย", "th", "th", "th", "thai"),
        ];

        public static IReadOnlyList<LanguageOption> All => Languages;

        public static LanguageOption AutoDetect => new("auto", "Auto Detect", "auto", "auto-detect", "auto", "detect", "default");

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
                || string.Equals(item.BingCode, idOrCode, StringComparison.OrdinalIgnoreCase)
                || string.Equals(item.AliyunCode, idOrCode, StringComparison.OrdinalIgnoreCase)) is { } fromProviderCode)
            {
                return fromProviderCode.DisplayName;
            }

            return idOrCode;
        }
    }
}
