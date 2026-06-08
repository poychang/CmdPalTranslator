using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CmdPalTranslator.Services
{
    internal sealed class SettingService
    {
        private readonly string _settingsFilePath;

        public SettingService(string? settingsFilePath = null)
        {
            _settingsFilePath = settingsFilePath ?? GetSettingsFilePath();
        }

        public TranslatorSettings? LoadSettings()
        {
            if (!File.Exists(_settingsFilePath))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(_settingsFilePath);
                Debug.WriteLine($"Loaded settings JSON: {json}");
                return JsonSerializer.Deserialize<TranslatorSettings>(json);
            }
            catch (Exception ex) when (ex is IOException or JsonException)
            {
                Debug.WriteLine($"Failed to load settings: {ex.Message}");
                return null;
            }
        }

        public void SaveSettings(string targetLanguageId, string preferredProviderId, string translateOperator)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath)!);

            TranslatorSettings settings = new()
            {
                TargetLanguageId = targetLanguageId,
                PreferredProviderId = preferredProviderId,
                TranslateOperator = translateOperator,
            };

            string json = JsonSerializer.Serialize(settings);
            File.WriteAllText(_settingsFilePath, json);
        }

        private static string GetSettingsFilePath()
        {
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(basePath, "CmdPalTranslator", "settings.json");
        }
    }

    internal sealed class TranslatorSettings
    {
        [JsonPropertyName("targetLanguageId")]
        public string TargetLanguageId { get; set; } = string.Empty;

        [JsonPropertyName("preferredProviderId")]
        public string PreferredProviderId { get; set; } = string.Empty;

        [JsonPropertyName("translateOperator")]
        public string TranslateOperator { get; set; } = string.Empty;
    }
}
