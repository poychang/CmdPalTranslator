using CmdPalTranslator.Models;
using System;
using System.Diagnostics;
using System.IO;

namespace CmdPalTranslator.Services
{
    internal sealed class TranslatorSettingsService
    {
        private readonly string _settingsFilePath;
        private string _targetLanguageId;

        public TranslatorSettingsService(string? settingsFilePath = null)
        {
            _settingsFilePath = settingsFilePath ?? GetSettingsFilePath();
            _targetLanguageId = LoadTargetLanguageId();
        }

        public event EventHandler? SettingsChanged;

        public LanguageOption TargetLanguage => ResolveTargetLanguage(_targetLanguageId);

        public bool SetTargetLanguage(LanguageOption language)
        {
            ArgumentNullException.ThrowIfNull(language);

            LanguageOption normalizedLanguage = ResolveTargetLanguage(language.Id);
            if (string.Equals(_targetLanguageId, normalizedLanguage.Id, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            _targetLanguageId = normalizedLanguage.Id;
            SaveTargetLanguageId();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        private static string GetSettingsFilePath()
        {
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(basePath, "CmdPalTranslator", "default-target-language.txt");
        }

        private string LoadTargetLanguageId()
        {
            if (!File.Exists(_settingsFilePath))
            {
                return LanguageCatalog.BuiltInDefaultTarget.Id;
            }

            try
            {
                string languageId = File.ReadAllText(_settingsFilePath).Trim();
                return ResolveTargetLanguage(languageId).Id;
            }
            catch (IOException ex)
            {
                Debug.WriteLine($"Failed to load target language setting: {ex.Message}");
            }

            return LanguageCatalog.BuiltInDefaultTarget.Id;
        }

        private void SaveTargetLanguageId()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath)!);
            File.WriteAllText(_settingsFilePath, _targetLanguageId);
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
