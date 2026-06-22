using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CmdPalTranslator
{
    internal sealed partial class CmdPalTranslatorSettingManager
    {
        private const string DisplayLanguageSettingId = "displayLanguage";

        private ChoiceSetSetting _displayLanguageSetting = null!;

        public string DisplayLanguageId => _translatorSettingService.DisplayLanguageId;

        public bool SetDisplayLanguage(string languageId) =>
            _translatorSettingService.SetDisplayLanguage(languageId);

        private void InitializeDisplayLanguageSettings()
        {
            List<ChoiceSetSetting.Choice> choices = LocalizationService.Instance.SupportedLanguages
                .Select(lang => new ChoiceSetSetting.Choice(lang.DisplayName, lang.Id))
                .ToList();

            _displayLanguageSetting = new ChoiceSetSetting(DisplayLanguageSettingId, choices)
            {
                Label = LocalizationService.Instance.Get("Settings.DisplayLanguage.Label"),
                Description = LocalizationService.Instance.Get("Settings.DisplayLanguage.Description"),
            };

            _settings.Add(_displayLanguageSetting);
            ApplyDisplayLanguageSettingValue(_translatorSettingService.DisplayLanguageId);
            _settings.SettingsChanged += OnDisplayLanguageSettingsChanged;
        }

        private void OnDisplayLanguageSettingsChanged(object sender, Settings args)
        {
            string? selectedLanguageId = _settings.GetSetting<string>(DisplayLanguageSettingId);
            if (!string.IsNullOrWhiteSpace(selectedLanguageId))
            {
                _translatorSettingService.SetDisplayLanguage(selectedLanguageId);
            }
        }

        private void ApplyDisplayLanguageSettingValue(string languageId)
        {
            if (!string.Equals(_displayLanguageSetting.Value, languageId, StringComparison.OrdinalIgnoreCase))
            {
                _displayLanguageSetting.Value = languageId;
            }
        }
    }
}
