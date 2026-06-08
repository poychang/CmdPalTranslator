using CmdPalTranslator.Models;

namespace CmdPalTranslator.Services
{
    internal sealed class CmdPalTranslatorSettingService
    {
        private readonly SettingService _settingService;
        private string _targetLanguageId;
        private string _preferredProviderId;

        public CmdPalTranslatorSettingService(string? settingsFilePath = null)
        {
            _settingService = new SettingService(settingsFilePath);

            TranslatorSettings? settings = _settingService.LoadSettings();
            _targetLanguageId = LoadTargetLanguageId(settings);
            _preferredProviderId = LoadPreferredProviderId(settings);
        }

        public event EventHandler? SettingsChanged;

        public LanguageOption TargetLanguage => ResolveTargetLanguage(_targetLanguageId);

        public string PreferredProviderId => _preferredProviderId;

        public bool SetTargetLanguage(LanguageOption language)
        {
            ArgumentNullException.ThrowIfNull(language);

            LanguageOption normalizedLanguage = ResolveTargetLanguage(language.Id);
            if (string.Equals(_targetLanguageId, normalizedLanguage.Id, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            _targetLanguageId = normalizedLanguage.Id;
            SaveSettings();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public bool SetPreferredProvider(string providerId)
        {
            ArgumentNullException.ThrowIfNull(providerId);

            if (string.Equals(_preferredProviderId, providerId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            _preferredProviderId = providerId;
            SaveSettings();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        private void SaveSettings()
        {
            _settingService.SaveSettings(_targetLanguageId, _preferredProviderId);
        }

        private static string LoadTargetLanguageId(TranslatorSettings? settings)
        {
            if (settings is not null && !string.IsNullOrWhiteSpace(settings.TargetLanguageId))
            {
                return ResolveTargetLanguage(settings.TargetLanguageId).Id;
            }

            return LanguageCatalog.BuiltInDefaultTarget.Id;
        }

        private static string LoadPreferredProviderId(TranslatorSettings? settings)
        {
            if (settings is not null && !string.IsNullOrWhiteSpace(settings.PreferredProviderId))
            {
                return settings.PreferredProviderId;
            }

            return TranslatorService.DefaultProviderId;
        }

        private static LanguageOption ResolveTargetLanguage(string? languageId)
        {
            if (!string.IsNullOrWhiteSpace(languageId)
                && LanguageCatalog.TryResolve(languageId, out var language)
                && !string.Equals(language!.Id, LanguageCatalog.AutoDetect.Id, StringComparison.OrdinalIgnoreCase))
            {
                return language;
            }

            return LanguageCatalog.BuiltInDefaultTarget;
        }
    }
}
