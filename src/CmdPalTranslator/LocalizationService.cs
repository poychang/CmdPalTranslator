using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CmdPalTranslator
{
    [JsonSourceGenerationOptions(JsonSerializerDefaults.General)]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    internal sealed partial class LocalizationJsonContext : JsonSerializerContext { }

    internal sealed class LocalizationService
    {
        public const string DefaultLanguageId = "en-US";

        private const string MetaDisplayNameKey = "_meta.displayName";

        private static readonly string LocalizationDirectory =
            Path.Combine(AppContext.BaseDirectory, "Localization");

        private Dictionary<string, string> _strings = new(StringComparer.OrdinalIgnoreCase);

        public static LocalizationService Instance { get; } = new();

        /// <summary>
        /// All display languages discovered from the Localization directory at startup,
        /// sorted alphabetically by language ID. Each entry carries the ID (matching the
        /// JSON filename without extension) and the native display name read from the
        /// <c>_meta.displayName</c> key inside the file.
        /// </summary>
        public IReadOnlyList<(string Id, string DisplayName)> SupportedLanguages { get; }

        public string CurrentLanguageId { get; private set; } = DefaultLanguageId;

        private LocalizationService()
        {
            SupportedLanguages = ScanAvailableLanguages();
        }

        public void Load(string? languageId)
        {
            string id = string.IsNullOrWhiteSpace(languageId) ? DefaultLanguageId : languageId.Trim();
            string path = Path.Combine(LocalizationDirectory, $"{id}.json");

            if (!File.Exists(path))
            {
                Debug.WriteLine($"[LocalizationService] Language file not found: {path}. Falling back to {DefaultLanguageId}.");
                id = DefaultLanguageId;
                path = Path.Combine(LocalizationDirectory, $"{id}.json");
            }

            if (!File.Exists(path))
            {
                Debug.WriteLine($"[LocalizationService] Default language file not found: {path}.");
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                Dictionary<string, string>? loaded = JsonSerializer.Deserialize(
                    json,
                    LocalizationJsonContext.Default.DictionaryStringString);

                // Exclude metadata keys so they are not accidentally returned by Get().
                _strings = new Dictionary<string, string>(
                    (loaded ?? []).Where(kv => !kv.Key.StartsWith("_meta.", StringComparison.Ordinal)),
                    StringComparer.OrdinalIgnoreCase);

                CurrentLanguageId = id;
                Debug.WriteLine($"[LocalizationService] Loaded language: {id} ({_strings.Count} strings)");
            }
            catch (Exception ex) when (ex is IOException or JsonException)
            {
                Debug.WriteLine($"[LocalizationService] Failed to load language file '{path}': {ex.Message}");
            }
        }

        public string Get(string key)
        {
            if (_strings.TryGetValue(key, out string? value))
            {
                return value;
            }

            Debug.WriteLine($"[LocalizationService] Missing key: '{key}'");
            return key;
        }

        public string Get(string key, params object?[] args)
        {
            string template = Get(key);
            return args.Length == 0 ? template : string.Format(template, args);
        }

        private static IReadOnlyList<(string Id, string DisplayName)> ScanAvailableLanguages()
        {
            if (!Directory.Exists(LocalizationDirectory))
            {
                Debug.WriteLine($"[LocalizationService] Localization directory not found: {LocalizationDirectory}");
                return [];
            }

            List<(string Id, string DisplayName)> result = [];

            foreach (string file in Directory.EnumerateFiles(LocalizationDirectory, "*.json").Order())
            {
                string id = Path.GetFileNameWithoutExtension(file);
                string displayName = TryReadMetaDisplayName(file) ?? id;
                result.Add((id, displayName));
            }

            Debug.WriteLine($"[LocalizationService] Discovered {result.Count} language(s): {string.Join(", ", result.Select(r => r.Id))}");
            return result.AsReadOnly();
        }

        private static string? TryReadMetaDisplayName(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                Dictionary<string, string>? dict = JsonSerializer.Deserialize(
                    json,
                    LocalizationJsonContext.Default.DictionaryStringString);
                return dict?.GetValueOrDefault(MetaDisplayNameKey);
            }
            catch (Exception ex) when (ex is IOException or JsonException)
            {
                Debug.WriteLine($"[LocalizationService] Failed to read metadata from '{filePath}': {ex.Message}");
                return null;
            }
        }
    }
}
