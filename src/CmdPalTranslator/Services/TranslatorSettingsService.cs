using CmdPalTranslator.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CmdPalTranslator.Services
{
    internal sealed partial class TranslatorSettingsService
    {
        private readonly string _settingsFilePath;
        private string _targetLanguageId;
        private string _preferredProviderId;

        public TranslatorSettingsService(string? settingsFilePath = null)
        {
            _settingsFilePath = settingsFilePath ?? GetSettingsFilePath();

            TranslatorSettings? settings = LoadSettings();
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

        private static string GetSettingsFilePath()
        {
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(basePath, "CmdPalTranslator", "settings.json");
        }

        private void SaveSettings()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath)!);

            TranslatorSettings settings = new() { TargetLanguageId = _targetLanguageId, PreferredProviderId = _preferredProviderId };
            string json = JsonSerializer.Serialize(settings, SettingsJsonContext.Default.TranslatorSettings);
            File.WriteAllText(_settingsFilePath, json);
        }

        private TranslatorSettings? LoadSettings()
        {
            if (!File.Exists(_settingsFilePath))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(_settingsFilePath);
                Debug.WriteLine($"Loaded settings JSON: {json}");
                return JsonSerializer.Deserialize(json, SettingsJsonContext.Default.TranslatorSettings);
            }
            catch (Exception ex) when (ex is IOException or JsonException)
            {
                Debug.WriteLine($"Failed to load settings: {ex.Message}");
                return null;
            }
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

        private sealed class TranslatorSettings
        {
            [JsonPropertyName("targetLanguageId")]
            public string TargetLanguageId { get; set; } = string.Empty;

            [JsonPropertyName("preferredProviderId")]
            public string PreferredProviderId { get; set; } = string.Empty;
        }

        // 使用 NativeAOT 建置應用程式時，會需要標註序列化會涉及的型別，讓應用程式可以正確序列化和反序列化這些型別。
        [JsonSourceGenerationOptions(WriteIndented = true)]
        [JsonSerializable(typeof(TranslatorSettings))]
        private sealed partial class SettingsJsonContext : JsonSerializerContext { }
    }
}
